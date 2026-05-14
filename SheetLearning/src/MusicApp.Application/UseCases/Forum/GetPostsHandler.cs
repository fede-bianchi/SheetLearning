using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;

namespace MusicApp.Application.UseCases.Forum;

public class GetPostsHandler
{
    private readonly IPostRepository _postRepository;
    private readonly IVoteRepository _voteRepository;
    private readonly ICommentRepository _commentRepo;

    public GetPostsHandler(
        IPostRepository postRepository,
        IVoteRepository voteRepository,
        ICommentRepository commentRepo)
    {
        _postRepository = postRepository;
        _voteRepository = voteRepository;
        _commentRepo = commentRepo;
    }

    public async Task<Result<PostListResponse>> HandleAsync(PostListQuery query, int? currentUserId)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize <= 0 ? 20 : Math.Min(query.PageSize, 50);
        var orderBy = string.IsNullOrWhiteSpace(query.OrderBy) ? "recenti" : query.OrderBy;

        var (posts, totalCount) = await _postRepository.GetPagedAsync(page, pageSize, orderBy);

        var postIds = posts.Select(post => post.Id).ToList();
        var voteCountsByPostId = postIds.Count > 0
            ? await _voteRepository.GetCountsBatchAsync("post", postIds)
            : new Dictionary<int, (int Upvotes, int Downvotes)>();

        var userVotesByPostId = currentUserId.HasValue && postIds.Count > 0
            ? await _voteRepository.GetUserVotesBatchAsync(currentUserId.Value, "post", postIds)
            : new Dictionary<int, string>();

        var commentCounts = postIds.Count > 0
            ? await _commentRepo.GetCountBatchAsync(postIds)
            : new Dictionary<int, int>();

        var postDtos = posts
            .Select(post =>
            {
                var counts = voteCountsByPostId.TryGetValue(post.Id, out var item)
                    ? item
                    : (Upvotes: 0, Downvotes: 0);

                userVotesByPostId.TryGetValue(post.Id, out var userVote);

                return post.ToPostSummaryDto(
                    counts.Upvotes,
                    counts.Downvotes,
                    userVote,
                    commentCount: commentCounts.TryGetValue(post.Id, out var cc) ? cc : 0);
            })
            .ToList();

        return Result<PostListResponse>.Ok(new PostListResponse(
            postDtos,
            totalCount,
            page,
            pageSize));
    }
}
