using GameOfLife.API.Models;
using GameOfLife.API.Repositories;
using GameOfLife.API.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace GameOfLife.Tests;

public abstract class TestBase
{
    protected readonly Mock<ILogger<GameOfLifeService>> MockLogger;
    protected readonly Mock<IBoardStateRepository> MockRepository;
    protected readonly Mock<IBoardValidationService> MockValidationService;
    protected readonly GameOfLifeService Service;

    protected TestBase()
    {
        MockLogger = new Mock<ILogger<GameOfLifeService>>();
        MockRepository = new Mock<IBoardStateRepository>();
        MockValidationService = new Mock<IBoardValidationService>();

        // Setup default validation responses
        MockValidationService.Setup(v => v.ValidateUploadRequest(It.IsAny<UploadBoardRequest>()))
            .Returns(ValidationResult.Success());
        MockValidationService.Setup(v => v.ValidateBoardId(It.IsAny<Guid>()))
            .Returns(ValidationResult.Success());
        MockValidationService.Setup(v => v.ValidateGenerations(It.IsAny<int>()))
            .Returns(ValidationResult.Success());

        // Setup default repository responses with proper state tracking
        var boardStates = new Dictionary<Guid, BoardState>();
        
        MockRepository.Setup(r => r.CreateAsync(It.IsAny<BoardState>()))
            .ReturnsAsync((BoardState bs) => 
            {
                boardStates[bs.Id] = bs;
                return bs;
            });
            
        MockRepository.Setup(r => r.UpdateAsync(It.IsAny<BoardState>()))
            .ReturnsAsync((BoardState bs) => 
            {
                boardStates[bs.Id] = bs;
                return bs;
            });
            
        MockRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Guid id) => boardStates.TryGetValue(id, out var board) ? board : null);
            
        MockRepository.Setup(r => r.AddHistoryEntryAsync(It.IsAny<BoardStateHistory>()))
            .ReturnsAsync((BoardStateHistory h) => h);

        Service = new GameOfLifeService(MockRepository.Object, MockValidationService.Object, MockLogger.Object);
    }

    protected BoardState CreateBoardState(bool[,] grid, int generation = 0)
    {
        return new BoardState
        {
            Id = Guid.NewGuid(),
            Grid = grid,
            Rows = grid.GetLength(0),
            Columns = grid.GetLength(1),
            Generation = generation,
            CreatedAt = DateTime.UtcNow,
            LastModifiedAt = DateTime.UtcNow
        };
    }

    protected UploadBoardRequest CreateUploadRequest(bool[,] grid)
    {
        return new UploadBoardRequest
        {
            Grid = grid,
            Rows = grid.GetLength(0),
            Columns = grid.GetLength(1)
        };
    }

    protected void SetupBoardExists(Guid boardId, BoardState boardState)
    {
        MockRepository.Setup(r => r.GetByIdAsync(boardId))
            .ReturnsAsync(boardState);
    }

    protected void SetupBoardNotFound(Guid boardId)
    {
        MockRepository.Setup(r => r.GetByIdAsync(boardId))
            .ReturnsAsync((BoardState?)null);
    }

    protected void SetupValidationFailure(string error)
    {
        MockValidationService.Setup(v => v.ValidateUploadRequest(It.IsAny<UploadBoardRequest>()))
            .Returns(ValidationResult.Failure(error));
    }
}
