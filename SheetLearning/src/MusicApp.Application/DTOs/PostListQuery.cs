namespace MusicApp.Application.DTOs;

public record PostListQuery(
    int Page = 1,
    int PageSize = 20,
    string OrderBy = "recenti"
);
