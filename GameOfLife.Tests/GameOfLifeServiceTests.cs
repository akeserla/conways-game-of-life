using GameOfLife.API.Models;
using GameOfLife.API.Services;
using Moq;

namespace GameOfLife.Tests;

public class GameOfLifeServiceTests : TestBase
{
    [Fact]
    public async Task UploadBoardStateAsync_ValidGrid_ReturnsSuccessResponse()
    {
        // Arrange
        var grid = new bool[,] { { true, false }, { false, true } };
        var request = CreateUploadRequest(grid);

        // Act
        var result = await Service.UploadBoardStateAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.BoardId);
        Assert.Contains("Board uploaded successfully", result.Message);
        Assert.Contains("2x2", result.Message);
    }

    [Fact]
    public async Task UploadBoardStateAsync_NullGrid_ThrowsArgumentException()
    {
        // Arrange
        var request = new UploadBoardRequest 
        { 
            Grid = null!,
            Rows = 0,
            Columns = 0
        };

        // Setup validation to fail
        SetupValidationFailure("Grid cannot be null");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => Service.UploadBoardStateAsync(request));
        Assert.Contains("Grid cannot be null", exception.Message);
    }

    [Fact]
    public async Task UploadBoardStateAsync_EmptyGrid_ThrowsArgumentException()
    {
        // Arrange
        var grid = new bool[0, 0];
        var request = new UploadBoardRequest 
        { 
            Grid = grid,
            Rows = 0,
            Columns = 0
        };

        // Setup validation to fail
        SetupValidationFailure("Grid cannot be empty");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => Service.UploadBoardStateAsync(request));
        Assert.Contains("Grid cannot be empty", exception.Message);
    }

    [Fact]
    public async Task GetNextStateAsync_ValidBoardId_ReturnsNextGeneration()
    {
        // Arrange
        var grid = new bool[,] { { true, true }, { true, false } };
        var boardId = Guid.NewGuid();
        var currentBoard = CreateBoardState(grid, 0);
        
        SetupBoardExists(boardId, currentBoard);

        // Act
        var result = await Service.GetNextStateAsync(boardId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(boardId, result.BoardId);
        Assert.Equal(1, result.Generation);
        Assert.Equal(2, result.Rows);
        Assert.Equal(2, result.Columns);
    }

    [Fact]
    public async Task GetNextStateAsync_InvalidBoardId_ThrowsKeyNotFoundException()
    {
        // Arrange
        var invalidId = Guid.NewGuid();
        SetupBoardNotFound(invalidId);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => Service.GetNextStateAsync(invalidId));
        Assert.Contains(invalidId.ToString(), exception.Message);
    }

    [Fact]
    public async Task GetNStatesAheadAsync_ValidRequest_ReturnsCorrectGeneration()
    {
        // Arrange
        var grid = new bool[,] { { true, false }, { false, true } };
        var boardId = Guid.NewGuid();
        var currentBoard = CreateBoardState(grid, 0);
        
        SetupBoardExists(boardId, currentBoard);

        // Act
        var result = await Service.GetNStatesAheadAsync(boardId, 3);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(boardId, result.BoardId);
        Assert.Equal(3, result.Generation);
        Assert.Equal(2, result.Rows);
        Assert.Equal(2, result.Columns);
    }

    [Fact]
    public async Task GetNStatesAheadAsync_InvalidGenerations_ThrowsArgumentException()
    {
        // Arrange
        var grid = new bool[,] { { true, false }, { false, true } };
        var boardId = Guid.NewGuid();
        var currentBoard = CreateBoardState(grid, 0);
        
        SetupBoardExists(boardId, currentBoard);

        // Setup validation to fail for generations
        MockValidationService.Setup(v => v.ValidateGenerations(0))
            .Returns(ValidationResult.Failure("Generations must be greater than 0"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => Service.GetNStatesAheadAsync(boardId, 0));
        Assert.Contains("Generations must be greater than 0", exception.Message);
    }

    [Fact]
    public async Task GetNStatesAheadAsync_InvalidBoardId_ThrowsKeyNotFoundException()
    {
        // Arrange
        var invalidId = Guid.NewGuid();
        SetupBoardNotFound(invalidId);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => Service.GetNStatesAheadAsync(invalidId, 1));
        Assert.Contains(invalidId.ToString(), exception.Message);
    }

    [Fact]
    public async Task GetFinalStateAsync_ValidBoardId_ReturnsFinalState()
    {
        // Arrange
        var grid = new bool[,] { { true, false }, { false, true } };
        var boardId = Guid.NewGuid();
        var currentBoard = CreateBoardState(grid, 0);
        
        SetupBoardExists(boardId, currentBoard);

        // Act
        var result = await Service.GetFinalStateAsync(boardId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(boardId, result.BoardId);
        Assert.True(result.Generation >= 0);
        Assert.Equal(2, result.Rows);
        Assert.Equal(2, result.Columns);
    }

    [Fact]
    public async Task GetFinalStateAsync_InvalidBoardId_ThrowsKeyNotFoundException()
    {
        // Arrange
        var invalidId = Guid.NewGuid();
        SetupBoardNotFound(invalidId);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => Service.GetFinalStateAsync(invalidId));
        Assert.Contains(invalidId.ToString(), exception.Message);
    }

    [Fact]
    public async Task GameOfLifeRules_StillLife_RemainsUnchanged()
    {
        // Arrange - Block pattern (still life)
        var grid = new bool[,] 
        { 
            { false, false, false, false },
            { false, true, true, false },
            { false, true, true, false },
            { false, false, false, false }
        };
        var boardId = Guid.NewGuid();
        var currentBoard = CreateBoardState(grid, 0);
        
        SetupBoardExists(boardId, currentBoard);

        // Act
        var result = await Service.GetNextStateAsync(boardId);

        // Assert - Block should remain unchanged
        Assert.Equal(1, result.Generation);
        Assert.True(AreGridsEqual(grid, result.Grid));
    }

    [Fact]
    public async Task GameOfLifeRules_Oscillator_ChangesCorrectly()
    {
        // Arrange - Blinker pattern (oscillator)
        var grid = new bool[,] 
        { 
            { false, false, false, false, false },
            { false, false, true, false, false },
            { false, false, true, false, false },
            { false, false, true, false, false },
            { false, false, false, false, false }
        };
        var boardId = Guid.NewGuid();
        var currentBoard = CreateBoardState(grid, 0);
        
        SetupBoardExists(boardId, currentBoard);

        // Act
        var result = await Service.GetNextStateAsync(boardId);

        // Assert - Should become horizontal
        var expectedHorizontal = new bool[,] 
        { 
            { false, false, false, false, false },
            { false, false, false, false, false },
            { false, true, true, true, false },
            { false, false, false, false, false },
            { false, false, false, false, false }
        };
        Assert.True(AreGridsEqual(expectedHorizontal, result.Grid));
    }

    [Fact]
    public async Task GameOfLifeRules_Underpopulation_CellDies()
    {
        // Arrange - Single live cell (should die from underpopulation)
        var grid = new bool[,] { { true } };
        var boardId = Guid.NewGuid();
        var currentBoard = CreateBoardState(grid, 0);
        
        SetupBoardExists(boardId, currentBoard);

        // Act
        var result = await Service.GetNextStateAsync(boardId);

        // Assert - Cell should die
        Assert.False(result.Grid[0, 0]);
    }

    [Fact]
    public async Task GameOfLifeRules_Reproduction_DeadCellBecomesAlive()
    {
        // Arrange - Pattern that creates new life
        var grid = new bool[,] 
        { 
            { false, true, false },
            { false, true, false },
            { false, true, false }
        };
        var boardId = Guid.NewGuid();
        var currentBoard = CreateBoardState(grid, 0);
        
        SetupBoardExists(boardId, currentBoard);

        // Act
        var result = await Service.GetNextStateAsync(boardId);

        // Assert - Middle cell should become alive
        Assert.True(result.Grid[1, 1]);
    }

    [Fact]
    public async Task GameOfLifeRules_Overpopulation_CellDies()
    {
        // Arrange - Live cell with 4 neighbors (should die from overpopulation)
        var grid = new bool[,] 
        { 
            { true, true, true },
            { true, true, true },
            { true, true, true }
        };
        var boardId = Guid.NewGuid();
        var currentBoard = CreateBoardState(grid, 0);
        
        SetupBoardExists(boardId, currentBoard);

        // Act
        var result = await Service.GetNextStateAsync(boardId);

        // Assert - Center cell should die
        Assert.False(result.Grid[1, 1]);
    }

    [Fact]
    public async Task MultipleGenerations_ConsistentResults()
    {
        // Arrange
        var grid = new bool[,] { { true, false }, { false, true } };
        var boardId = Guid.NewGuid();
        var currentBoard = CreateBoardState(grid, 0);
        
        SetupBoardExists(boardId, currentBoard);

        // Setup the repository to return the board state as-is (the service handles generation calculation)
        MockRepository.Setup(r => r.UpdateAsync(It.IsAny<BoardState>()))
            .ReturnsAsync((BoardState bs) => bs);

        // Act - Get 3 generations ahead
        var result3 = await Service.GetNStatesAheadAsync(boardId, 3);

        // Assert - Should return generation 3 (starting from 0, advancing 3 generations)
        Assert.Equal(3, result3.Generation);
        Assert.Equal(2, result3.Rows);
        Assert.Equal(2, result3.Columns);
    }

    [Fact]
    public async Task EdgeCase_1x1Grid_HandlesCorrectly()
    {
        // Arrange
        var grid = new bool[,] { { true } };
        var boardId = Guid.NewGuid();
        var currentBoard = CreateBoardState(grid, 0);
        
        SetupBoardExists(boardId, currentBoard);

        // Act
        var result = await Service.GetNextStateAsync(boardId);

        // Assert
        Assert.Equal(1, result.Generation);
        Assert.Equal(1, result.Rows);
        Assert.Equal(1, result.Columns);
        Assert.False(result.Grid[0, 0]); // Should die from underpopulation
    }

    [Fact]
    public async Task EdgeCase_2x2Grid_HandlesCorrectly()
    {
        // Arrange
        var grid = new bool[,] { { true, true }, { true, true } };
        var boardId = Guid.NewGuid();
        var currentBoard = CreateBoardState(grid, 0);
        
        SetupBoardExists(boardId, currentBoard);

        // Act
        var result = await Service.GetNextStateAsync(boardId);

        // Assert
        Assert.Equal(1, result.Generation);
        Assert.Equal(2, result.Rows);
        Assert.Equal(2, result.Columns);
        // Each cell has 3 neighbors, so they should all survive
        Assert.True(result.Grid[0, 0]);
        Assert.True(result.Grid[0, 1]);
        Assert.True(result.Grid[1, 0]);
        Assert.True(result.Grid[1, 1]);
    }

    [Fact]
    public async Task GetNStatesAheadAsync_AddsHistoryEntries()
    {
        // Arrange
        var grid = new bool[,] { { true, false }, { false, true } };
        var boardId = Guid.NewGuid();
        var currentBoard = CreateBoardState(grid, 0);
        
        SetupBoardExists(boardId, currentBoard);

        // Act
        var result = await Service.GetNStatesAheadAsync(boardId, 3);

        // Assert
        Assert.Equal(3, result.Generation);
        
        // Verify that AddHistoryEntryAsync was called 3 times (once for each generation)
        MockRepository.Verify(r => r.AddHistoryEntryAsync(It.IsAny<BoardStateHistory>()), Times.Exactly(3));
    }

    [Fact]
    public async Task GetFinalStateAsync_AddsHistoryEntries()
    {
        // Arrange
        var grid = new bool[,] { { true, false }, { false, true } };
        var boardId = Guid.NewGuid();
        var currentBoard = CreateBoardState(grid, 0);
        
        SetupBoardExists(boardId, currentBoard);

        // Act
        var result = await Service.GetFinalStateAsync(boardId);

        // Assert
        Assert.True(result.Generation > 0);
        
        // Verify that AddHistoryEntryAsync was called at least once
        MockRepository.Verify(r => r.AddHistoryEntryAsync(It.IsAny<BoardStateHistory>()), Times.AtLeastOnce());
    }

    private bool AreGridsEqual(bool[,] grid1, bool[,] grid2)
    {
        if (grid1.GetLength(0) != grid2.GetLength(0) || 
            grid1.GetLength(1) != grid2.GetLength(1))
            return false;

        for (int row = 0; row < grid1.GetLength(0); row++)
        {
            for (int col = 0; col < grid1.GetLength(1); col++)
            {
                if (grid1[row, col] != grid2[row, col])
                    return false;
            }
        }
        return true;
    }
}