using GameOfLife.API.Models;
using GameOfLife.API.Repositories;
using Microsoft.Extensions.Logging;

namespace GameOfLife.API.Services;

public class GameOfLifeService : IGameOfLifeService
{
    private readonly IBoardStateRepository _repository;
    private readonly IBoardValidationService _validationService;
    private readonly ILogger<GameOfLifeService> _logger;
    private const int MaxIterations = 1000; // Reasonable limit for finding stable state

    public GameOfLifeService(
        IBoardStateRepository repository,
        IBoardValidationService validationService,
        ILogger<GameOfLifeService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<UploadBoardResponse> UploadBoardStateAsync(UploadBoardRequest request)
    {
        try
        {
            // Validate request
            var validationResult = _validationService.ValidateUploadRequest(request);
            if (!validationResult.IsValid)
            {
                var errorMessage = string.Join("; ", validationResult.Errors);
                throw new ArgumentException(errorMessage);
            }

            var boardId = Guid.NewGuid();
            var boardState = new BoardState
            {
                Id = boardId,
                GridData = BoardState.SerializeGrid(request.Grid),
                Rows = request.Rows,
                Columns = request.Columns,
                CreatedAt = DateTime.UtcNow,
                Generation = 0,
                LastModifiedAt = DateTime.UtcNow
            };

            var createdBoard = await _repository.CreateAsync(boardState);

            _logger.LogInformation("Board uploaded with ID: {BoardId}, Size: {Rows}x{Columns}", 
                boardId, request.Rows, request.Columns);

            return new UploadBoardResponse
            {
                BoardId = boardId,
                Message = $"Board uploaded successfully. Size: {request.Rows}x{request.Columns}",
                CreatedAt = createdBoard.CreatedAt
            };
        }
        catch (Exception ex) when (ex is not ArgumentException)
        {
            _logger.LogError(ex, "Error uploading board state");
            throw new InvalidOperationException("Failed to upload board state", ex);
        }
    }

    public async Task<BoardStateResponse> GetNextStateAsync(Guid boardId)
    {
        try
        {
            _logger.LogInformation("Starting GetNextStateAsync for board {BoardId}", boardId);
            
            // Validate board ID
            var validationResult = _validationService.ValidateBoardId(boardId);
            if (!validationResult.IsValid)
            {
                throw new ArgumentException(validationResult.Errors.First());
            }

            _logger.LogInformation("Validation passed for board {BoardId}", boardId);

            var currentBoard = await _repository.GetByIdAsync(boardId);
            if (currentBoard == null)
            {
                throw new KeyNotFoundException($"Board with ID {boardId} not found");
            }

            _logger.LogInformation("Retrieved board {BoardId} with GridData: {GridData}, Rows: {Rows}, Columns: {Columns}", 
                boardId, currentBoard.GridData, currentBoard.Rows, currentBoard.Columns);

            _logger.LogInformation("About to deserialize grid data: {GridData}", currentBoard.GridData);
            
            bool[,] currentGrid;
            try
            {
                currentGrid = BoardState.DeserializeGrid(currentBoard.GridData, currentBoard.Rows, currentBoard.Columns);
                _logger.LogInformation("Grid deserialized successfully: {Rows}x{Columns}", currentGrid.GetLength(0), currentGrid.GetLength(1));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deserialize grid data: {GridData}", currentBoard.GridData);
                throw;
            }
            
            var nextGrid = CalculateNextGeneration(currentGrid);
            
            // Update the existing entity instead of creating a new one
            currentBoard.GridData = BoardState.SerializeGrid(nextGrid);
            currentBoard.Generation = currentBoard.Generation + 1;
            currentBoard.LastModifiedAt = DateTime.UtcNow;

            // Save the updated state
            var updatedBoard = await _repository.UpdateAsync(currentBoard);

            // Add to history
            var historyEntry = new BoardStateHistory
            {
                Id = Guid.NewGuid(),
                BoardStateId = boardId,
                GridData = currentBoard.GridData,
                Generation = currentBoard.Generation,
                CreatedAt = DateTime.UtcNow
            };
            await _repository.AddHistoryEntryAsync(historyEntry);

            _logger.LogInformation("Advanced board {BoardId} to generation {Generation}", 
                boardId, currentBoard.Generation);

            return new BoardStateResponse
            {
                BoardId = boardId,
                Grid = BoardState.DeserializeGrid(updatedBoard.GridData, updatedBoard.Rows, updatedBoard.Columns),
                Rows = updatedBoard.Rows,
                Columns = updatedBoard.Columns,
                Generation = updatedBoard.Generation,
                LastModifiedAt = updatedBoard.LastModifiedAt
            };
        }
        catch (Exception ex) when (ex is not ArgumentException && ex is not KeyNotFoundException)
        {
            _logger.LogError(ex, "Error getting next state for board {BoardId}", boardId);
            throw new InvalidOperationException($"Failed to get next state for board {boardId}", ex);
        }
    }

    public async Task<BoardStateResponse> GetNStatesAheadAsync(Guid boardId, int generations)
    {
        try
        {
            // Validate inputs
            var boardValidation = _validationService.ValidateBoardId(boardId);
            if (!boardValidation.IsValid)
            {
                throw new ArgumentException(boardValidation.Errors.First());
            }

            var generationsValidation = _validationService.ValidateGenerations(generations);
            if (!generationsValidation.IsValid)
            {
                throw new ArgumentException(generationsValidation.Errors.First());
            }

            var currentBoard = await _repository.GetByIdAsync(boardId);
            if (currentBoard == null)
            {
                throw new KeyNotFoundException($"Board with ID {boardId} not found");
            }

            var currentGrid = (bool[,])BoardState.DeserializeGrid(currentBoard.GridData, currentBoard.Rows, currentBoard.Columns).Clone();
            var currentGeneration = currentBoard.Generation;

            for (int i = 0; i < generations; i++)
            {
                currentGrid = CalculateNextGeneration(currentGrid);
                currentGeneration++;
                
                // Add history entry for this generation
                var historyEntry = new BoardStateHistory
                {
                    Id = Guid.NewGuid(),
                    BoardStateId = boardId,
                    GridData = BoardState.SerializeGrid(currentGrid),
                    Generation = currentGeneration,
                    CreatedAt = DateTime.UtcNow
                };
                await _repository.AddHistoryEntryAsync(historyEntry);
            }

            // Update the existing entity instead of creating a new one
            currentBoard.GridData = BoardState.SerializeGrid(currentGrid);
            currentBoard.Generation = currentGeneration;
            currentBoard.LastModifiedAt = DateTime.UtcNow;

            // Update the board state
            var updatedBoard = await _repository.UpdateAsync(currentBoard);

            _logger.LogInformation("Advanced board {BoardId} by {Generations} generations to generation {Generation}", 
                boardId, generations, currentGeneration);

            return new BoardStateResponse
            {
                BoardId = boardId,
                Grid = currentGrid,
                Rows = updatedBoard.Rows,
                Columns = updatedBoard.Columns,
                Generation = updatedBoard.Generation,
                LastModifiedAt = updatedBoard.LastModifiedAt
            };
        }
        catch (Exception ex) when (ex is not ArgumentException && ex is not KeyNotFoundException)
        {
            _logger.LogError(ex, "Error getting {Generations} states ahead for board {BoardId}", generations, boardId);
            throw new InvalidOperationException($"Failed to get {generations} states ahead for board {boardId}", ex);
        }
    }

    public async Task<BoardStateResponse> GetFinalStateAsync(Guid boardId)
    {
        try
        {
            // Validate board ID
            var validationResult = _validationService.ValidateBoardId(boardId);
            if (!validationResult.IsValid)
            {
                throw new ArgumentException(validationResult.Errors.First());
            }

            var currentBoard = await _repository.GetByIdAsync(boardId);
            if (currentBoard == null)
            {
                throw new KeyNotFoundException($"Board with ID {boardId} not found");
            }

            var currentGrid = (bool[,])BoardState.DeserializeGrid(currentBoard.GridData, currentBoard.Rows, currentBoard.Columns).Clone();
            var previousGrid = new bool[currentBoard.Rows, currentBoard.Columns];
            var iterations = 0;
            var currentGeneration = currentBoard.Generation;

            while (iterations < MaxIterations)
            {
                // Check if current state is the same as previous state
                if (AreGridsEqual(currentGrid, previousGrid))
                {
                    _logger.LogInformation("Stable state found after {Iterations} iterations for board {BoardId}", 
                        iterations, boardId);
                    break;
                }

                // Store current state as previous
                Array.Copy(currentGrid, previousGrid, currentGrid.Length);
                
                // Calculate next generation
                currentGrid = CalculateNextGeneration(currentGrid);
                iterations++;
                currentGeneration++;
                
                // Add history entry for this generation
                var historyEntry = new BoardStateHistory
                {
                    Id = Guid.NewGuid(),
                    BoardStateId = boardId,
                    GridData = BoardState.SerializeGrid(currentGrid),
                    Generation = currentGeneration,
                    CreatedAt = DateTime.UtcNow
                };
                await _repository.AddHistoryEntryAsync(historyEntry);
            }

            if (iterations >= MaxIterations)
            {
                _logger.LogWarning("Maximum iterations reached for board {BoardId}. Returning current state.", boardId);
            }

            // Update the existing entity instead of creating a new one
            currentBoard.GridData = BoardState.SerializeGrid(currentGrid);
            currentBoard.Generation = currentGeneration;
            currentBoard.LastModifiedAt = DateTime.UtcNow;

            // Update the board state
            var updatedBoard = await _repository.UpdateAsync(currentBoard);

            _logger.LogInformation("Calculated final state for board {BoardId} after {Iterations} iterations", 
                boardId, iterations);

            return new BoardStateResponse
            {
                BoardId = boardId,
                Grid = BoardState.DeserializeGrid(updatedBoard.GridData, updatedBoard.Rows, updatedBoard.Columns),
                Rows = updatedBoard.Rows,
                Columns = updatedBoard.Columns,
                Generation = updatedBoard.Generation,
                LastModifiedAt = updatedBoard.LastModifiedAt
            };
        }
        catch (Exception ex) when (ex is not ArgumentException && ex is not KeyNotFoundException)
        {
            _logger.LogError(ex, "Error getting final state for board {BoardId}", boardId);
            throw new InvalidOperationException($"Failed to get final state for board {boardId}", ex);
        }
    }

    private bool[,] CalculateNextGeneration(bool[,] currentGrid)
    {
        try
        {
            _logger.LogInformation("CalculateNextGeneration called with grid dimensions: {Rows}x{Columns}", 
                currentGrid.GetLength(0), currentGrid.GetLength(1));
            
            var rows = currentGrid.GetLength(0);
            var columns = currentGrid.GetLength(1);
            var nextGrid = new bool[rows, columns];

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < columns; col++)
                {
                    var liveNeighbors = CountLiveNeighbors(currentGrid, row, col);
                    var isCurrentlyAlive = currentGrid[row, col];

                    // Apply Game of Life rules
                    if (isCurrentlyAlive)
                    {
                        // Any live cell with fewer than two live neighbors dies (underpopulation)
                        // Any live cell with two or three live neighbors lives
                        // Any live cell with more than three live neighbors dies (overpopulation)
                        nextGrid[row, col] = liveNeighbors == 2 || liveNeighbors == 3;
                    }
                    else
                    {
                        // Any dead cell with exactly three live neighbors becomes alive (reproduction)
                        nextGrid[row, col] = liveNeighbors == 3;
                    }
                }
            }

            _logger.LogInformation("CalculateNextGeneration completed successfully");
            return nextGrid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CalculateNextGeneration");
            throw;
        }
    }

    private int CountLiveNeighbors(bool[,] grid, int row, int col)
    {
        var rows = grid.GetLength(0);
        var columns = grid.GetLength(1);
        var count = 0;

        // Check all 8 neighboring cells
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0) continue; // Skip the current cell

                var neighborRow = row + i;
                var neighborCol = col + j;

                // Check bounds
                if (neighborRow >= 0 && neighborRow < rows && 
                    neighborCol >= 0 && neighborCol < columns)
                {
                    if (grid[neighborRow, neighborCol])
                    {
                        count++;
                    }
                }
            }
        }

        return count;
    }

    private bool AreGridsEqual(bool[,] grid1, bool[,] grid2)
    {
        var rows = grid1.GetLength(0);
        var columns = grid1.GetLength(1);

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                if (grid1[row, col] != grid2[row, col])
                {
                    return false;
                }
            }
        }

        return true;
    }
}