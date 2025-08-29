using GameOfLife.API.Models;
using GameOfLife.API.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace GameOfLife.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GameOfLifeController : ControllerBase
{
    private readonly IGameOfLifeService _gameOfLifeService;
    private readonly ILogger<GameOfLifeController> _logger;

    public GameOfLifeController(IGameOfLifeService gameOfLifeService, ILogger<GameOfLifeController> logger)
    {
        _gameOfLifeService = gameOfLifeService ?? throw new ArgumentNullException(nameof(gameOfLifeService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Upload a new board state (2D grid of cells)
    /// </summary>
    /// <param name="request">The board state to upload</param>
    /// <returns>A unique identifier for the stored board</returns>
    [HttpPost("upload")]
    [ProducesResponseType(typeof(UploadBoardResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UploadBoardResponse>> UploadBoardState([FromBody] UploadBoardRequest request)
    {
        var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        
        try
        {
            var response = await _gameOfLifeService.UploadBoardStateAsync(request);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error uploading board state. RequestId: {RequestId}", requestId);
            return BadRequest(new ErrorResponse 
            { 
                Error = "Invalid Request", 
                Message = ex.Message,
                RequestId = requestId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading board state. RequestId: {RequestId}", requestId);
            return StatusCode(500, new ErrorResponse 
            { 
                Error = "Internal Server Error", 
                Message = "An error occurred while uploading the board state",
                RequestId = requestId
            });
        }
    }

    /// <summary>
    /// Get the next generation state of the board
    /// </summary>
    /// <param name="boardId">The ID of the board</param>
    /// <returns>The next generation state</returns>
    [HttpGet("{boardId}/next")]
    [ProducesResponseType(typeof(BoardStateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<BoardStateResponse>> GetNextState(Guid boardId)
    {
        var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        
        try
        {
            var response = await _gameOfLifeService.GetNextStateAsync(boardId);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error getting next state for board {BoardId}. RequestId: {RequestId}", boardId, requestId);
            return BadRequest(new ErrorResponse 
            { 
                Error = "Invalid Request", 
                Message = ex.Message,
                RequestId = requestId
            });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Board not found: {BoardId}. RequestId: {RequestId}", boardId, requestId);
            return NotFound(new ErrorResponse 
            { 
                Error = "Not Found", 
                Message = ex.Message,
                RequestId = requestId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting next state for board {BoardId}. RequestId: {RequestId}", boardId, requestId);
            return StatusCode(500, new ErrorResponse 
            { 
                Error = "Internal Server Error", 
                Message = "An error occurred while getting the next state",
                RequestId = requestId
            });
        }
    }

    /// <summary>
    /// Get the board state after N generations
    /// </summary>
    /// <param name="boardId">The ID of the board</param>
    /// <param name="generations">The number of generations to advance</param>
    /// <returns>The board state after N generations</returns>
    [HttpGet("{boardId}/advance/{generations}")]
    [ProducesResponseType(typeof(BoardStateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<BoardStateResponse>> GetNStatesAhead(Guid boardId, int generations)
    {
        var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        
        try
        {
            var response = await _gameOfLifeService.GetNStatesAheadAsync(boardId, generations);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error getting {Generations} states ahead for board {BoardId}. RequestId: {RequestId}", 
                generations, boardId, requestId);
            return BadRequest(new ErrorResponse 
            { 
                Error = "Invalid Request", 
                Message = ex.Message,
                RequestId = requestId
            });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Board not found: {BoardId}. RequestId: {RequestId}", boardId, requestId);
            return NotFound(new ErrorResponse 
            { 
                Error = "Not Found", 
                Message = ex.Message,
                RequestId = requestId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting {Generations} states ahead for board {BoardId}. RequestId: {RequestId}", 
                generations, boardId, requestId);
            return StatusCode(500, new ErrorResponse 
            { 
                Error = "Internal Server Error", 
                Message = "An error occurred while advancing the board state",
                RequestId = requestId
            });
        }
    }

    /// <summary>
    /// Get the final stable state of the board
    /// </summary>
    /// <param name="boardId">The ID of the board</param>
    /// <returns>The final stable state or current state if stability not reached within reasonable iterations</returns>
    [HttpGet("{boardId}/final")]
    [ProducesResponseType(typeof(BoardStateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<BoardStateResponse>> GetFinalState(Guid boardId)
    {
        var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        
        try
        {
            var response = await _gameOfLifeService.GetFinalStateAsync(boardId);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error getting final state for board {BoardId}. RequestId: {RequestId}", boardId, requestId);
            return BadRequest(new ErrorResponse 
            { 
                Error = "Invalid Request", 
                Message = ex.Message,
                RequestId = requestId
            });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Board not found: {BoardId}. RequestId: {RequestId}", boardId, requestId);
            return NotFound(new ErrorResponse 
            { 
                Error = "Not Found", 
                Message = ex.Message,
                RequestId = requestId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting final state for board {BoardId}. RequestId: {RequestId}", boardId, requestId);
            return StatusCode(500, new ErrorResponse 
            { 
                Error = "Internal Server Error", 
                Message = "An error occurred while getting the final state",
                RequestId = requestId
            });
        }
    }
}
