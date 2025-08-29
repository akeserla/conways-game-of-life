using GameOfLife.API.Models;

namespace GameOfLife.API.Services;

public class BoardValidationService : IBoardValidationService
{
    private const int MaxGridSize = 1000;
    private const int MaxGenerations = 100;
    
    public ValidationResult ValidateUploadRequest(UploadBoardRequest request)
    {
        var errors = new List<string>();
        
        if (request == null)
        {
            errors.Add("Request cannot be null");
            return ValidationResult.Failure(errors.ToArray());
        }
        
        if (request.Grid == null)
        {
            errors.Add("Grid cannot be null");
            return ValidationResult.Failure(errors.ToArray());
        }
        
        if (request.Rows <= 0)
        {
            errors.Add("Rows must be greater than 0");
        }
        
        if (request.Columns <= 0)
        {
            errors.Add("Columns must be greater than 0");
        }
        
        if (request.Rows > MaxGridSize)
        {
            errors.Add($"Rows cannot exceed {MaxGridSize}");
        }
        
        if (request.Columns > MaxGridSize)
        {
            errors.Add($"Columns cannot exceed {MaxGridSize}");
        }
        
        if (request.Grid.GetLength(0) != request.Rows)
        {
            errors.Add("Grid row count does not match specified rows");
        }
        
        if (request.Grid.GetLength(1) != request.Columns)
        {
            errors.Add("Grid column count does not match specified columns");
        }
        
        if (errors.Any())
        {
            return ValidationResult.Failure(errors.ToArray());
        }
        
        return ValidationResult.Success();
    }
    
    public ValidationResult ValidateGenerations(int generations)
    {
        if (generations <= 0)
        {
            return ValidationResult.Failure("Generations must be greater than 0");
        }
        
        if (generations > MaxGenerations)
        {
            return ValidationResult.Failure($"Generations cannot exceed {MaxGenerations}");
        }
        
        return ValidationResult.Success();
    }
    
    public ValidationResult ValidateBoardId(Guid boardId)
    {
        if (boardId == Guid.Empty)
        {
            return ValidationResult.Failure("Board ID cannot be empty");
        }
        
        return ValidationResult.Success();
    }
}
