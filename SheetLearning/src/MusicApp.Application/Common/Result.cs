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

    // Phase 3
    public const string PostNotFound = "POST_NOT_FOUND";
    public const string CommentNotFound = "COMMENT_NOT_FOUND";
    public const string PostDeleted = "POST_DELETED";
    public const string CommentDeleted = "COMMENT_DELETED";
    public const string Forbidden = "FORBIDDEN";
    public const string VoteTargetNotFound = "VOTE_TARGET_NOT_FOUND";
    public const string InvalidTargetType = "INVALID_TARGET_TYPE";

    // Phase 4
    public const string TeacherProfileNotFound    = "TEACHER_PROFILE_NOT_FOUND";
    public const string TeacherNotVisible         = "TEACHER_NOT_VISIBLE";
    public const string SlotNotFound              = "SLOT_NOT_FOUND";
    public const string SlotNotAvailable          = "SLOT_NOT_AVAILABLE";
    public const string SlotAlreadyBooked         = "SLOT_ALREADY_BOOKED";
    public const string SlotTeacherMismatch       = "SLOT_TEACHER_MISMATCH";
    public const string BookingNotFound           = "BOOKING_NOT_FOUND";
    public const string BookingInvalidTransition  = "BOOKING_INVALID_TRANSITION";
    public const string RatingWindowExpired       = "RATING_WINDOW_EXPIRED";
    public const string RatingAlreadyExists       = "RATING_ALREADY_EXISTS";
    public const string BundleNotFound            = "BUNDLE_NOT_FOUND";
    public const string BundleInactive            = "BUNDLE_INACTIVE";
    public const string BundleExhausted           = "BUNDLE_EXHAUSTED";
    public const string BundleExpired             = "BUNDLE_EXPIRED";
    public const string BundlePurchaseNotFound    = "BUNDLE_PURCHASE_NOT_FOUND";
    public const string StudentNotFound           = "STUDENT_NOT_FOUND";
    public const string TeacherStudentUnrelated   = "TEACHER_STUDENT_UNRELATED";
    public const string CannotBookOwnSlot         = "CANNOT_BOOK_OWN_SLOT";
    public const string InvalidCategoryCount      = "INVALID_CATEGORY_COUNT";

    // Phase 5
    public const string BundleNotFoundForCheckout    = "BUNDLE_NOT_FOUND_FOR_CHECKOUT";
    public const string ActiveSubscriptionExists     = "ACTIVE_SUBSCRIPTION_EXISTS";
    public const string NoActiveSubscription         = "NO_ACTIVE_SUBSCRIPTION";
    public const string WebhookSignatureInvalid      = "WEBHOOK_SIGNATURE_INVALID";
    public const string WebhookEventUnhandled        = "WEBHOOK_EVENT_UNHANDLED";
    public const string PaymentAlreadyProcessed      = "PAYMENT_ALREADY_PROCESSED";

    // Phase 6
    public const string ChatNotFound          = "CHAT_NOT_FOUND";
    public const string TeacherNotFound       = "TEACHER_NOT_FOUND";
    public const string CannotMessageSelf     = "CANNOT_MESSAGE_SELF";
    public const string MessageContentEmpty   = "MESSAGE_CONTENT_EMPTY";
    public const string TargetNotATeacher     = "TARGET_NOT_A_TEACHER";

    // Phase 7
    public const string NotificationNotFound = "NOTIFICATION_NOT_FOUND";
    public const string NotificationArchived = "NOTIFICATION_ARCHIVED";

    // Phase 8
    public const string AdminCannotTargetSelf   = "ADMIN_CANNOT_TARGET_SELF";
    public const string UserAlreadyBanned       = "USER_ALREADY_BANNED";
    public const string UserNotBanned           = "USER_NOT_BANNED";
    public const string InvalidRole             = "INVALID_ROLE";
    public const string RoleNotFound            = "ROLE_NOT_FOUND";
    public const string AdminCannotChangeOwnRole = "ADMIN_CANNOT_CHANGE_OWN_ROLE";
    public const string InvalidDateRange        = "INVALID_DATE_RANGE";
}
