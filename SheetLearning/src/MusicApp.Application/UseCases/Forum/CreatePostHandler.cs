using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;
using MusicApp.Domain.Entities;

namespace MusicApp.Application.UseCases.Forum;

public class CreatePostHandler
{
    private readonly IPostRepository _postRepository;

    public CreatePostHandler(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<Result<PostSummaryDto>> HandleAsync(int userId, CreatePostRequest request)
    {
        var now = DateTime.UtcNow;

        var post = new Post
        {
            UserId = userId,
            Titolo = request.Titolo,
            Contenuto = request.Contenuto,
            IsDeleted = false,
            DeletedAt = null,
            CreatedAt = now,
            UpdatedAt = now
        };

        var created = await _postRepository.CreateAsync(post);
        var createdWithAuthor = await _postRepository.GetByIdWithAuthorAsync(created.Id);
        if (createdWithAuthor is null)
        {
            return Result<PostSummaryDto>.Fail(ErrorCodes.PostNotFound, "Post non trovato.");
        }

        var dto = createdWithAuthor.ToPostSummaryDto(
            upvotes: 0,
            downvotes: 0,
            userVote: null,
            commentCount: 0);

        return Result<PostSummaryDto>.Ok(dto);
    }
}
