using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameOfLife.API.Models;

public class BoardState
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    [Column(TypeName = "nvarchar(max)")]
    public string GridData { get; set; } = string.Empty;
    
    [Required]
    public int Rows { get; set; }
    
    [Required]
    public int Columns { get; set; }
    
    [Required]
    public DateTime CreatedAt { get; set; }
    
    [Required]
    public int Generation { get; set; }
    
    [Required]
    public DateTime LastModifiedAt { get; set; }
    
    // Navigation property for board history
    public virtual ICollection<BoardStateHistory> History { get; set; } = new List<BoardStateHistory>();
    
    // Computed property to convert GridData back to bool[,]
    [NotMapped]
    public bool[,] Grid
    {
        get => DeserializeGrid(GridData, Rows, Columns);
        set
        {
            GridData = SerializeGrid(value);
            Rows = value.GetLength(0);
            Columns = value.GetLength(1);
        }
    }
    
    public static string SerializeGrid(bool[,] grid)
    {
        var rows = grid.GetLength(0);
        var cols = grid.GetLength(1);
        var result = new List<string>();
        
        for (int i = 0; i < rows; i++)
        {
            var row = new List<string>();
            for (int j = 0; j < cols; j++)
            {
                row.Add(grid[i, j] ? "1" : "0");
            }
            result.Add(string.Join(",", row));
        }
        
        return string.Join(";", result);
    }
    
    public static bool[,] DeserializeGrid(string gridData, int rows, int cols)
    {
        var grid = new bool[rows, cols];
        var rowsData = gridData.Split(';');
        
        for (int i = 0; i < rows && i < rowsData.Length; i++)
        {
            var colsData = rowsData[i].Split(',');
            for (int j = 0; j < cols && j < colsData.Length; j++)
            {
                grid[i, j] = colsData[j] == "1";
            }
        }
        
        return grid;
    }
}

public class BoardStateHistory
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    public Guid BoardStateId { get; set; }
    
    [Required]
    public string GridData { get; set; } = string.Empty;
    
    [Required]
    public int Generation { get; set; }
    
    [Required]
    public DateTime CreatedAt { get; set; }
    
    // Navigation property
    [ForeignKey(nameof(BoardStateId))]
    public virtual BoardState BoardState { get; set; } = null!;
    

}

public class UploadBoardRequest
{
    [Required]
    [Range(1, 1000, ErrorMessage = "Rows must be between 1 and 1000")]
    public int Rows { get; set; }
    
    [Required]
    [Range(1, 1000, ErrorMessage = "Columns must be between 1 and 1000")]
    public int Columns { get; set; }
    
    // Internal 2D representation. Ignored during JSON de/serialization for requests.
    [JsonIgnore]
    public bool[,] Grid { get; set; } = new bool[0, 0];

    // JSON-binding surrogate for 'grid' using jagged arrays which System.Text.Json supports.
    [JsonPropertyName("grid")]
    public bool[][] GridJson
    {
        get => GridConverters.ToJagged(Grid);
        set
        {
            if (value == null)
            {
                Grid = new bool[0, 0];
                Rows = 0;
                Columns = 0;
                return;
            }
            Grid = GridConverters.FromJagged(value);
            Rows = Grid.GetLength(0);
            Columns = Grid.GetLength(1);
        }
    }
    
    public bool IsValid()
    {
        if (Grid == null) return false;
        if (Grid.GetLength(0) != Rows || Grid.GetLength(1) != Columns) return false;
        if (Rows <= 0 || Columns <= 0) return false;
        if (Rows > 1000 || Columns > 1000) return false;
        return true;
    }
}

public class UploadBoardResponse
{
    public Guid BoardId { get; set; }
    public required string Message { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class BoardStateResponse
{
    public Guid BoardId { get; set; }
    [JsonIgnore]
    public required bool[,] Grid { get; set; }
    
    // JSON-binding surrogate to output jagged array
    [JsonPropertyName("grid")]
    public bool[][] GridJson
    {
        get => GridConverters.ToJagged(Grid);
        set => Grid = GridConverters.FromJagged(value);
    }
    public int Rows { get; set; }
    public int Columns { get; set; }
    public int Generation { get; set; }
    public DateTime LastModifiedAt { get; set; }
}

public class GetNStatesRequest
{
    [Required]
    [Range(1, 100, ErrorMessage = "Generations must be between 1 and 100")]
    public int Generations { get; set; }
}

public class ErrorResponse
{
    public required string Error { get; set; }
    public required string Message { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? RequestId { get; set; }
}

/// <summary>
/// System.Text.Json converter for serializing and deserializing multidimensional bool arrays (bool[,]).
/// It writes as a JSON array of arrays and reads the same, validating rectangular shape.
/// </summary>
public sealed class Bool2DArrayJsonConverter : JsonConverter<bool[,]>
{
    public override bool[,] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException("Expected start of array for 2D grid");
        }

        var rows = new List<List<bool>>();
        int? expectedCols = null;

        // Read outer array
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
            {
                break;
            }

            if (reader.TokenType != JsonTokenType.StartArray)
            {
                throw new JsonException("Expected start of inner array for grid row");
            }

            var row = new List<bool>();
            // Read inner array
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                {
                    break;
                }
                if (reader.TokenType == JsonTokenType.True)
                {
                    row.Add(true);
                }
                else if (reader.TokenType == JsonTokenType.False)
                {
                    row.Add(false);
                }
                else
                {
                    throw new JsonException("Expected boolean value in grid row");
                }
            }

            if (expectedCols is null)
            {
                expectedCols = row.Count;
            }
            else if (row.Count != expectedCols)
            {
                throw new JsonException("All rows in the grid must have the same number of columns");
            }

            rows.Add(row);
        }

        var numRows = rows.Count;
        var numCols = expectedCols ?? 0;

        var result = new bool[numRows, numCols];
        for (int i = 0; i < numRows; i++)
        {
            for (int j = 0; j < numCols; j++)
            {
                result[i, j] = rows[i][j];
            }
        }

        return result;
    }

    public override void Write(Utf8JsonWriter writer, bool[,] value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        int rows = value.GetLength(0);
        int cols = value.GetLength(1);
        for (int i = 0; i < rows; i++)
        {
            writer.WriteStartArray();
            for (int j = 0; j < cols; j++)
            {
                writer.WriteBooleanValue(value[i, j]);
            }
            writer.WriteEndArray();
        }
        writer.WriteEndArray();
    }
}

public static class GridConverters
{
    public static bool[][] ToJagged(bool[,] grid)
    {
        var rows = grid.GetLength(0);
        var cols = grid.GetLength(1);
        var result = new bool[rows][];
        for (int i = 0; i < rows; i++)
        {
            result[i] = new bool[cols];
            for (int j = 0; j < cols; j++)
            {
                result[i][j] = grid[i, j];
            }
        }
        return result;
    }

    public static bool[,] FromJagged(bool[][] jagged)
    {
        var rows = jagged.Length;
        var cols = rows == 0 ? 0 : jagged[0].Length;
        for (int i = 1; i < rows; i++)
        {
            if (jagged[i].Length != cols)
            {
                throw new ArgumentException("All rows must have the same number of columns");
            }
        }
        var result = new bool[rows, cols];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                result[i, j] = jagged[i][j];
            }
        }
        return result;
    }
}

// (Removed file-level wrapper methods to avoid top-level members in this file.)
