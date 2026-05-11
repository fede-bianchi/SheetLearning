using MusicApp.Application.Authorization;

namespace MusicApp.Web.Models;

public class OwnedResourceWrapper : IOwnedResource
{
    public int OwnerId { get; }

    public OwnedResourceWrapper(int ownerId)
    {
        OwnerId = ownerId;
    }
}
