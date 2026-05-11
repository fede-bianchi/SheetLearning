namespace MusicApp.Application.DTOs;

public record UpdateTeacherProfileRequest(
    string?  Bio,
    string?  Specializzazioni,
    string   VisibileA
);
