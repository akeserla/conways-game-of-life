using GameOfLife.API.Data;
using GameOfLife.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GameOfLife.API.Repositories;

public class BoardStateRepository : IBoardStateRepository
{
    private readonly GameOfLifeDbContext _context;
    private readonly ILogger<BoardStateRepository> _logger;

    public BoardStateRepository(GameOfLifeDbContext context, ILogger<BoardStateRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<BoardState?> GetByIdAsync(Guid id)
    {
        try
        {
            var board = await _context.BoardStates
                .Include(b => b.History)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (board != null && board.History != null)
            {
                board.History = board.History
                    .OrderByDescending(h => h.Generation)
                    .ToList();
            }

            return board;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving board state with ID {BoardId}", id);
            throw;
        }
    }

    public async Task<BoardState> CreateAsync(BoardState boardState)
    {
        try
        {
            boardState.CreatedAt = DateTime.UtcNow;
            boardState.LastModifiedAt = DateTime.UtcNow;
            
            _context.BoardStates.Add(boardState);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Created board state with ID {BoardId}", boardState.Id);
            return boardState;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating board state");
            throw;
        }
    }

public async Task<BoardState> UpdateAsync(BoardState boardState)
{
    try
    {
        boardState.LastModifiedAt = DateTime.UtcNow;
        
        // Check if entity is already being tracked
        var existingEntity = _context.ChangeTracker.Entries<BoardState>()
            .FirstOrDefault(e => e.Entity.Id == boardState.Id);
            
        if (existingEntity != null)
        {
            // Update the existing tracked entity
            existingEntity.CurrentValues.SetValues(boardState);
        }
        else
        {
            // Use Update for new entities
            _context.BoardStates.Update(boardState);
        }
        
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Updated board state with ID {BoardId}", boardState.Id);
        return boardState;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error updating board state with ID {BoardId}", boardState.Id);
        throw;
    }
}

    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            var boardState = await _context.BoardStates.FindAsync(id);
            if (boardState == null)
                return false;

            _context.BoardStates.Remove(boardState);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Deleted board state with ID {BoardId}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting board state with ID {BoardId}", id);
            throw;
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        try
        {
            return await _context.BoardStates.AnyAsync(b => b.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking existence of board state with ID {BoardId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<BoardState>> GetAllAsync(int skip = 0, int take = 100)
    {
        try
        {
            return await _context.BoardStates
                .OrderByDescending(b => b.LastModifiedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving board states");
            throw;
        }
    }

    public async Task<int> GetCountAsync()
    {
        try
        {
            return await _context.BoardStates.CountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error counting board states");
            throw;
        }
    }

    public async Task<BoardStateHistory> AddHistoryEntryAsync(BoardStateHistory historyEntry)
    {
        try
        {
            historyEntry.CreatedAt = DateTime.UtcNow;
            
            _context.BoardStateHistory.Add(historyEntry);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Added history entry for board {BoardId} at generation {Generation}", 
                historyEntry.BoardStateId, historyEntry.Generation);
            return historyEntry;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding history entry for board {BoardId}", historyEntry.BoardStateId);
            throw;
        }
    }

    public async Task<IEnumerable<BoardStateHistory>> GetHistoryAsync(Guid boardId, int skip = 0, int take = 100)
    {
        try
        {
            return await _context.BoardStateHistory
                .Where(h => h.BoardStateId == boardId)
                .OrderByDescending(h => h.Generation)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving history for board {BoardId}", boardId);
            throw;
        }
    }
}
