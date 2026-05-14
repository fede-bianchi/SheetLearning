namespace MusicApp.Application.Caching;

public static class CacheKeys
{
    public const string ExerciseTypes    = "exercise_types";
    public const string ActiveBundles    = "active_bundles";
    public const string PlatformStats    = "admin_platform_stats";
    public static string UserLevels(int userId) => $"levels_user_{userId}";
}
