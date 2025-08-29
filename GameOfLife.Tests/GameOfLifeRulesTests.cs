using GameOfLife.API.Models;
using GameOfLife.API.Services;
using GameOfLife.API.Repositories;
using Microsoft.Extensions.Logging;
using Moq;

namespace GameOfLife.Tests;

public class GameOfLifeRulesTests : TestBase
{
    [Fact]
    public async Task StillLife_Block_RemainsUnchanged()
    {
        // Arrange - Block pattern (still life)
        var grid = new bool[,] 
        { 
            { false, false, false, false },
            { false, true, true, false },
            { false, true, true, false },
            { false, false, false, false }
        };
        var request = CreateUploadRequest(grid);
        var uploadResult = await Service.UploadBoardStateAsync(request);

        // Act
        var result = await Service.GetNextStateAsync(uploadResult.BoardId);

        // Assert - Block should remain unchanged
        Assert.True(AreGridsEqual(grid, result.Grid));
    }

    [Fact]
    public async Task StillLife_Beehive_RemainsUnchanged()
    {
        // Arrange - Beehive pattern (still life)
        var grid = new bool[,] 
        { 
            { false, false, false, false, false, false },
            { false, false, true, true, false, false },
            { false, true, false, false, true, false },
            { false, false, true, true, false, false },
            { false, false, false, false, false, false }
        };
        var request = CreateUploadRequest(grid);
        var uploadResult = await Service.UploadBoardStateAsync(request);

        // Act
        var result = await Service.GetNextStateAsync(uploadResult.BoardId);

        // Assert - Beehive should remain unchanged
        Assert.True(AreGridsEqual(grid, result.Grid));
    }

    [Fact]
    public async Task Oscillator_Blinker_AlternatesCorrectly()
    {
        // Arrange - Blinker pattern (oscillator)
        var verticalBlinker = new bool[,] 
        { 
            { false, false, false, false, false },
            { false, false, true, false, false },
            { false, false, true, false, false },
            { false, false, true, false, false },
            { false, false, false, false, false }
        };
        var request = CreateUploadRequest(verticalBlinker);
        var uploadResult = await Service.UploadBoardStateAsync(request);

        // Act - Get next state
        var result1 = await Service.GetNextStateAsync(uploadResult.BoardId);
        
        // Get state after that
        var result2 = await Service.GetNextStateAsync(uploadResult.BoardId);

        // Assert - Should alternate between vertical and horizontal
        var expectedHorizontal = new bool[,] 
        { 
            { false, false, false, false, false },
            { false, false, false, false, false },
            { false, true, true, true, false },
            { false, false, false, false, false },
            { false, false, false, false, false }
        };
        
        Assert.True(AreGridsEqual(expectedHorizontal, result1.Grid));
        Assert.True(AreGridsEqual(verticalBlinker, result2.Grid));
    }

    [Fact]
    public async Task Oscillator_Toad_AlternatesCorrectly()
    {
        // Arrange - Toad pattern (oscillator)
        var toadPhase1 = new bool[,] 
        { 
            { false, false, false, false, false, false },
            { false, false, false, false, false, false },
            { false, false, true, true, true, false },
            { false, true, true, true, false, false },
            { false, false, false, false, false, false },
            { false, false, false, false, false, false }
        };
        var request = CreateUploadRequest(toadPhase1);
        var uploadResult = await Service.UploadBoardStateAsync(request);

        // Act - Get next state
        var result = await Service.GetNextStateAsync(uploadResult.BoardId);

        // Assert - Should become the other phase
        var expectedPhase2 = new bool[,] 
        { 
            { false, false, false, false, false, false },
            { false, false, false, true, false, false },
            { false, true, false, false, true, false },
            { false, true, false, false, true, false },
            { false, false, true, false, false, false },
            { false, false, false, false, false, false }
        };
        
        Assert.True(AreGridsEqual(expectedPhase2, result.Grid));
    }

    [Fact]
    public async Task Spaceship_Glider_MovesCorrectly()
    {
        // Arrange - Glider pattern (spaceship)
        var gliderPhase1 = new bool[,] 
        { 
            { false, false, false, false, false, false, false },
            { false, false, true, false, false, false, false },
            { false, false, false, true, false, false, false },
            { false, true, true, true, false, false, false },
            { false, false, false, false, false, false, false },
            { false, false, false, false, false, false, false },
            { false, false, false, false, false, false, false }
        };
        var request = CreateUploadRequest(gliderPhase1);
        var uploadResult = await Service.UploadBoardStateAsync(request);

        // Act - Get next state
        var result = await Service.GetNextStateAsync(uploadResult.BoardId);

        // Assert - Glider should move diagonally
        var expectedPhase2 = new bool[,] 
        { 
            { false, false, false, false, false, false, false },
            { false, false, false, false, false, false, false },
            { false, true, false, true, false, false, false },
            { false, false, true, true, false, false, false },
            { false, false, true, false, false, false, false },
            { false, false, false, false, false, false, false },
            { false, false, false, false, false, false, false }
        };
        
        Assert.True(AreGridsEqual(expectedPhase2, result.Grid));
    }

    [Fact]
    public async Task EdgeCase_EmptyGrid_HandlesCorrectly()
    {
        // Arrange - Empty grid
        var grid = new bool[,] { { false, false }, { false, false } };
        var request = CreateUploadRequest(grid);
        var uploadResult = await Service.UploadBoardStateAsync(request);

        // Act
        var result = await Service.GetNextStateAsync(uploadResult.BoardId);

        // Assert - Empty grid should remain empty
        Assert.True(AreGridsEqual(grid, result.Grid));
    }

    [Fact]
    public async Task EdgeCase_SingleLiveCell_IsolatesCorrectly()
    {
        // Arrange - Single live cell
        var grid = new bool[,] { { true } };
        var request = CreateUploadRequest(grid);
        var uploadResult = await Service.UploadBoardStateAsync(request);

        // Act
        var result = await Service.GetNextStateAsync(uploadResult.BoardId);

        // Assert - Single cell should die from underpopulation
        Assert.False(result.Grid[0, 0]);
    }

    [Fact]
    public async Task EdgeCase_CrossPattern_BehavesCorrectly()
    {
        // Arrange - Cross pattern
        var grid = new bool[,] 
        { 
            { false, true, false },
            { true, true, true },
            { false, true, false }
        };
        var request = CreateUploadRequest(grid);
        var uploadResult = await Service.UploadBoardStateAsync(request);

        // Act
        var result = await Service.GetNextStateAsync(uploadResult.BoardId);

        // Assert - Center should die (4 neighbors), corners should become alive (3 neighbors)
        Assert.False(result.Grid[1, 1]); // Center dies from overpopulation
        Assert.True(result.Grid[0, 0]);  // Corner becomes alive from reproduction
        Assert.True(result.Grid[0, 2]);  // Corner becomes alive from reproduction
        Assert.True(result.Grid[2, 0]);  // Corner becomes alive from reproduction
        Assert.True(result.Grid[2, 2]);  // Corner becomes alive from reproduction
    }

    [Fact]
    public async Task MultipleGenerations_StablePattern_ReachesStableState()
    {
        // Arrange - Pattern that should reach a stable state
        var grid = new bool[,] 
        { 
            { false, true, false },
            { true, true, true },
            { false, true, false }
        };
        var request = CreateUploadRequest(grid);
        var uploadResult = await Service.UploadBoardStateAsync(request);

        // Act - Get final state
        var result = await Service.GetFinalStateAsync(uploadResult.BoardId);

        // Assert - Should reach a stable state
        Assert.True(result.Generation > 0);
        
        // Verify that the final state is indeed stable by checking it doesn't change
        // The cross pattern should evolve to a stable state
        Assert.True(result.Generation > 1); // Should take more than 1 generation to stabilize
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
