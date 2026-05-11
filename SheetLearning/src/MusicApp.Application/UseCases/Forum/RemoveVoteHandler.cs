using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;

namespace MusicApp.Application.UseCases.Forum;

public class RemoveVoteHandler
{
    private readonly IVoteRepository _voteRepository;

    public RemoveVoteHandler(IVoteRepository voteRepository)
    {
        _voteRepository = voteRepository;
    }

    public async Task<Result<bool>> HandleAsync(int userId, VoteRequest request)
    {
        var existingVote = await _voteRepository.GetByUserAndTargetAsync(
            userId,
            request.TargetType,
            request.TargetId);

        if (existingVote is null)
        {
            return Result<bool>.Ok(true);
        }

        await _voteRepository.DeleteAsync(existingVote);
        return Result<bool>.Ok(true);
    }
}
