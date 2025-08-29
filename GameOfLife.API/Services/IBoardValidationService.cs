using GameOfLife.API.Models;

namespace GameOfLife.API.Services;

public interface IBoardValidationService
{
    ValidationResult ValidateUploadRequest(UploadBoardRequest request);
    ValidationResult ValidateGenerations(int generations);
    ValidationResult ValidateBoardId(Guid boardId);
}

public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    
    public static ValidationResult Success() => new() { IsValid = true };
    
    public static ValidationResult Failure(params string[] errors) => new()
    {
        IsValid = false,
        Errors = errors.ToList()
    };
}
