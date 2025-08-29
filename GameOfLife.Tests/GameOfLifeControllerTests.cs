using GameOfLife.API.Controllers;
using GameOfLife.API.Models;
using GameOfLife.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace GameOfLife.Tests;

public class GameOfLifeControllerTests
{
    private readonly Mock<IGameOfLifeService> _mockService;
    private readonly Mock<ILogger<GameOfLifeController>> _mockLogger;
    private readonly GameOfLifeController _controller;

    public GameOfLifeControllerTests()
    {
        _mockService = new Mock<IGameOfLifeService>();
        _mockLogger = new Mock<ILogger<GameOfLifeController>>();
        _controller = new GameOfLifeController(_mockService.Object, _mockLogger.Object);
        
        // Set up HttpContext for testing
        var httpContext = new DefaultHttpContext();
        httpContext.TraceIdentifier = "test-trace-id";
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    [Fact]
    public async Task UploadBoardState_ValidRequest_ReturnsOkResult()
    {
        // Arrange
        var request = new UploadBoardRequest
        {
            Grid = new bool[,] { { true, false }, { false, true } },
            Rows = 2,
            Columns = 2
        };
        var expectedResponse = new UploadBoardResponse
        {
            BoardId = Guid.NewGuid(),
            Message = "Board uploaded successfully. Size: 2x2"
        };
        _mockService.Setup(s => s.UploadBoardStateAsync(request))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.UploadBoardState(request);

        // Assert
        var actionResult = Assert.IsType<ActionResult<UploadBoardResponse>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var response = Assert.IsType<UploadBoardResponse>(okResult.Value);
        Assert.Equal(expectedResponse.BoardId, response.BoardId);
        Assert.Equal(expectedResponse.Message, response.Message);
    }

    [Fact]
    public async Task UploadBoardState_ServiceThrowsException_ReturnsBadRequest()
    {
        // Arrange
        var request = new UploadBoardRequest
        {
            Grid = new bool[0, 0], // Invalid grid
            Rows = 0,
            Columns = 0
        };
        _mockService.Setup(s => s.UploadBoardStateAsync(request))
            .ThrowsAsync(new ArgumentException("Grid cannot be null or empty"));

        // Act
        var result = await _controller.UploadBoardState(request);

        // Assert
        var actionResult = Assert.IsType<ActionResult<UploadBoardResponse>>(result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        var errorResponse = Assert.IsType<ErrorResponse>(badRequestResult.Value);
        Assert.Equal("Invalid Request", errorResponse.Error);
        Assert.Contains("Grid cannot be null or empty", errorResponse.Message);
    }

    [Fact]
    public async Task GetNextState_ValidBoardId_ReturnsOkResult()
    {
        // Arrange
        var boardId = Guid.NewGuid();
        var expectedResponse = new BoardStateResponse
        {
            BoardId = boardId,
            Grid = new bool[,] { { false, true }, { true, false } },
            Rows = 2,
            Columns = 2,
            Generation = 1,
            LastModifiedAt = DateTime.UtcNow
        };
        _mockService.Setup(s => s.GetNextStateAsync(boardId))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetNextState(boardId);

        // Assert
        var actionResult = Assert.IsType<ActionResult<BoardStateResponse>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var response = Assert.IsType<BoardStateResponse>(okResult.Value);
        Assert.Equal(expectedResponse.BoardId, response.BoardId);
        Assert.Equal(expectedResponse.Generation, response.Generation);
    }

    [Fact]
    public async Task GetNextState_ServiceThrowsException_ReturnsNotFound()
    {
        // Arrange
        var boardId = Guid.NewGuid();
        _mockService.Setup(s => s.GetNextStateAsync(boardId))
            .ThrowsAsync(new KeyNotFoundException($"Board with ID {boardId} not found"));

        // Act
        var result = await _controller.GetNextState(boardId);

        // Assert
        var actionResult = Assert.IsType<ActionResult<BoardStateResponse>>(result);
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
        var errorResponse = Assert.IsType<ErrorResponse>(notFoundResult.Value);
        Assert.Equal("Not Found", errorResponse.Error);
        Assert.Contains(boardId.ToString(), errorResponse.Message);
    }

    [Fact]
    public async Task GetNStatesAhead_ValidRequest_ReturnsOkResult()
    {
        // Arrange
        var boardId = Guid.NewGuid();
        var generations = 3;
        var expectedResponse = new BoardStateResponse
        {
            BoardId = boardId,
            Grid = new bool[,] { { false, false }, { false, false } },
            Rows = 2,
            Columns = 2,
            Generation = 3,
            LastModifiedAt = DateTime.UtcNow
        };
        _mockService.Setup(s => s.GetNStatesAheadAsync(boardId, generations))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetNStatesAhead(boardId, generations);

        // Assert
        var actionResult = Assert.IsType<ActionResult<BoardStateResponse>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var response = Assert.IsType<BoardStateResponse>(okResult.Value);
        Assert.Equal(expectedResponse.BoardId, response.BoardId);
        Assert.Equal(expectedResponse.Generation, response.Generation);
    }

    [Fact]
    public async Task GetNStatesAhead_ServiceThrowsArgumentException_ReturnsBadRequest()
    {
        // Arrange
        var boardId = Guid.NewGuid();
        var generations = 0; // Invalid generations
        _mockService.Setup(s => s.GetNStatesAheadAsync(boardId, generations))
            .ThrowsAsync(new ArgumentException("Generations must be greater than 0"));

        // Act
        var result = await _controller.GetNStatesAhead(boardId, generations);

        // Assert
        var actionResult = Assert.IsType<ActionResult<BoardStateResponse>>(result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        var errorResponse = Assert.IsType<ErrorResponse>(badRequestResult.Value);
        Assert.Equal("Invalid Request", errorResponse.Error);
        Assert.Contains("Generations must be greater than 0", errorResponse.Message);
    }

    [Fact]
    public async Task GetNStatesAhead_ServiceThrowsKeyNotFoundException_ReturnsNotFound()
    {
        // Arrange
        var boardId = Guid.NewGuid();
        var generations = 1;
        _mockService.Setup(s => s.GetNStatesAheadAsync(boardId, generations))
            .ThrowsAsync(new KeyNotFoundException($"Board with ID {boardId} not found"));

        // Act
        var result = await _controller.GetNStatesAhead(boardId, generations);

        // Assert
        var actionResult = Assert.IsType<ActionResult<BoardStateResponse>>(result);
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
        var errorResponse = Assert.IsType<ErrorResponse>(notFoundResult.Value);
        Assert.Equal("Not Found", errorResponse.Error);
        Assert.Contains(boardId.ToString(), errorResponse.Message);
    }

    [Fact]
    public async Task GetFinalState_ValidBoardId_ReturnsOkResult()
    {
        // Arrange
        var boardId = Guid.NewGuid();
        var expectedResponse = new BoardStateResponse
        {
            BoardId = boardId,
            Grid = new bool[,] { { false, false }, { false, false } },
            Rows = 2,
            Columns = 2,
            Generation = 5,
            LastModifiedAt = DateTime.UtcNow
        };
        _mockService.Setup(s => s.GetFinalStateAsync(boardId))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetFinalState(boardId);

        // Assert
        var actionResult = Assert.IsType<ActionResult<BoardStateResponse>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var response = Assert.IsType<BoardStateResponse>(okResult.Value);
        Assert.Equal(expectedResponse.BoardId, response.BoardId);
        Assert.Equal(expectedResponse.Generation, response.Generation);
    }

    [Fact]
    public async Task GetFinalState_ServiceThrowsException_ReturnsNotFound()
    {
        // Arrange
        var boardId = Guid.NewGuid();
        _mockService.Setup(s => s.GetFinalStateAsync(boardId))
            .ThrowsAsync(new KeyNotFoundException($"Board with ID {boardId} not found"));

        // Act
        var result = await _controller.GetFinalState(boardId);

        // Assert
        var actionResult = Assert.IsType<ActionResult<BoardStateResponse>>(result);
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
        var errorResponse = Assert.IsType<ErrorResponse>(notFoundResult.Value);
        Assert.Equal("Not Found", errorResponse.Error);
        Assert.Contains(boardId.ToString(), errorResponse.Message);
    }

    [Fact]
    public async Task GetFinalState_ServiceThrowsOtherException_ReturnsInternalServerError()
    {
        // Arrange
        var boardId = Guid.NewGuid();
        _mockService.Setup(s => s.GetFinalStateAsync(boardId))
            .ThrowsAsync(new InvalidOperationException("Unexpected error"));

        // Act
        var result = await _controller.GetFinalState(boardId);

        // Assert
        var actionResult = Assert.IsType<ActionResult<BoardStateResponse>>(result);
        var statusCodeResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(500, statusCodeResult.StatusCode);
        var errorResponse = Assert.IsType<ErrorResponse>(statusCodeResult.Value);
        Assert.Equal("Internal Server Error", errorResponse.Error);
        Assert.Contains("An error occurred while getting the final state", errorResponse.Message);
    }
}
