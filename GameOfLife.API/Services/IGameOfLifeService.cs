using GameOfLife.API.Models;

namespace GameOfLife.API.Services;

public interface IGameOfLifeService
{
    Task<UploadBoardResponse> UploadBoardStateAsync(UploadBoardRequest request);
    Task<BoardStateResponse> GetNextStateAsync(Guid boardId);
    Task<BoardStateResponse> GetNStatesAheadAsync(Guid boardId, int generations);
    Task<BoardStateResponse> GetFinalStateAsync(Guid boardId);
}