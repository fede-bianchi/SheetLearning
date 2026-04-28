using Microsoft.EntityFrameworkCore;
using MusicApp.Domain.Entities;

namespace MusicApp.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<ExerciseType> ExerciseTypes => Set<ExerciseType>();
    public DbSet<Clef> Clefs => Set<Clef>();
    public DbSet<Level> Levels => Set<Level>();
    public DbSet<UserLevelProgress> UserLevelProgress => Set<UserLevelProgress>();
    public DbSet<Attempt> Attempts => Set<Attempt>();
    public DbSet<AttemptError> AttemptErrors => Set<AttemptError>();
    public DbSet<BestScore> BestScores => Set<BestScore>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Vote> Votes => Set<Vote>();
    public DbSet<LessonBundle> LessonBundles => Set<LessonBundle>();
    public DbSet<LessonSlot> LessonSlots => Set<LessonSlot>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<LessonBundlePurchase> LessonBundlePurchases => Set<LessonBundlePurchase>();
    public DbSet<LessonBooking> LessonBookings => Set<LessonBooking>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<Chat> Chats => Set<Chat>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<ModerationLog> ModerationLogs => Set<ModerationLog>();
    public DbSet<TeacherProfile> TeacherProfiles => Set<TeacherProfile>();
    public DbSet<TeacherCategory> TeacherCategories => Set<TeacherCategory>();
    public DbSet<LessonRating> LessonRatings => Set<LessonRating>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
