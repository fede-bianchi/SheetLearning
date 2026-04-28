namespace MusicApp.Domain.Entities;

public class TeacherCategory
{
    public int Id { get; set; }
    public int TeacherId { get; set; }
    public string Categoria { get; set; } = string.Empty;

    public TeacherProfile TeacherProfile { get; set; } = null!;
}
