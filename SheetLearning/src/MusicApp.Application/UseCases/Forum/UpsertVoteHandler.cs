using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;
using MusicApp.Domain.Entities;

namespace MusicApp.Application.UseCases.Forum;

public class UpsertVoteHandler
{
    private readonly IVoteRepository _voteRepository;
    private readonly IPostRepository _postRepository;
    private readonly ICommentRepository _commentRepository;

    public UpsertVoteHandler(
        IVoteRepository voteRepository,
        IPostRepository postRepository,
        ICommentRepository commentRepository)
    {
        _voteRepository = voteRepository;
        _postRepository = postRepository;
        _commentRepository = commentRepository;
    }

    public async Task<Result<bool>> HandleAsync(int userId, VoteRequest request)
    {
        if (request.TargetType is not ("post" or "comment"))
        {
            return Result<bool>.Fail(ErrorCodes.InvalidTargetType, "TargetType non valido.");
        }

        if (request.TargetType == "post")
        {
            var post = await _postRepository.GetByIdAsync(request.TargetId);
            if (post is null || post.IsDeleted)
            {
                return Result<bool>.Fail(ErrorCodes.VoteTargetNotFound, "Target del voto non trovato.");
            }
        }
        else
        {
            var comment = await _commentRepository.GetByIdAsync(request.TargetId);
            if (comment is null || comment.IsDeleted)
            {
                return Result<bool>.Fail(ErrorCodes.VoteTargetNotFound, "Target del voto non trovato.");
            }
        }

        var existingVote = await _voteRepository.GetByUserAndTargetAsync(
            userId,
            request.TargetType,
            request.TargetId);

        if (existingVote is null)
        {
            var vote = new Vote
            {
                UserId = userId,
                TargetType = request.TargetType,
                TargetId = request.TargetId,
                Voto = request.Voto,
                CreatedAt = DateTime.UtcNow
            };

            await _voteRepository.CreateAsync(vote);
            return Result<bool>.Ok(true);
        }

        if (existingVote.Voto == request.Voto)
        {
            return Result<bool>.Ok(true);
        }

        existingVote.Voto = request.Voto;
        await _voteRepository.UpdateAsync(existingVote);
        return Result<bool>.Ok(true);
    }
}
