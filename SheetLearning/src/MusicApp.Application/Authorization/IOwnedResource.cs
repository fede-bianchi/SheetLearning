namespace MusicApp.Application.Authorization;

public interface IOwnedResource
{
    int OwnerId { get; }
}
