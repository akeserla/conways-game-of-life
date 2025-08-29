using GameOfLife.API.Models;

namespace GameOfLife.API.Repositories;

public interface IBoardStateRepository
{
    Task<BoardState?> GetByIdAsync(Guid id);
    Task<BoardState> CreateAsync(BoardState boardState);
    Task<BoardState> UpdateAsync(BoardState boardState);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<IEnumerable<BoardState>> GetAllAsync(int skip = 0, int take = 100);
    Task<int> GetCountAsync();
    Task<BoardStateHistory> AddHistoryEntryAsync(BoardStateHistory historyEntry);
    Task<IEnumerable<BoardStateHistory>> GetHistoryAsync(Guid boardId, int skip = 0, int take = 100);
}
