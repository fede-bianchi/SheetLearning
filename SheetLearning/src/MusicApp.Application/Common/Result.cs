namespace MusicApp.Application.Common;

public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? ErrorCode { get; }
    public string? ErrorMessage { get; }

    private Result(T value)
    {
        IsSuccess = true;
        Value = value;
    }

    private Result(string code, string message)
    {
        IsSuccess = false;
        ErrorCode = code;
        ErrorMessage = message;
    }

    public static Result<T> Ok(T value) => new(value);

    public static Result<T> Fail(string code, string message) => new(code, message);
}

public static class ErrorCodes
{
    public const string EmailAlreadyExists = "EMAIL_ALREADY_EXISTS";
    public const string NicknameAlreadyExists = "NICKNAME_ALREADY_EXISTS";
    public const string InvalidCredentials = "INVALID_CREDENTIALS";
    public const string AccountInactive = "ACCOUNT_INACTIVE";
    public const string InvalidRefreshToken = "INVALID_REFRESH_TOKEN";
    public const string ExpiredRefreshToken = "EXPIRED_REFRESH_TOKEN";

    public const string UserNotFound = "USER_NOT_FOUND";
    public const string WrongPassword = "WRONG_PASSWORD";
    public const string NicknameConflict = "NICKNAME_CONFLICT";
    public const string UnderAge = "UNDER_AGE";

    public const string ExerciseTypeNotFound = "EXERCISE_TYPE_NOT_FOUND";
    public const string LevelNotFound = "LEVEL_NOT_FOUND";
    public const string LevelExerciseMismatch = "LEVEL_EXERCISE_MISMATCH";
    public const string AttemptNotFound = "ATTEMPT_NOT_FOUND";
    public const string InvalidDifficolta = "INVALID_DIFFICOLTA";
    public const string InvalidPunteggio = "INVALID_PUNTEGGIO";
}
