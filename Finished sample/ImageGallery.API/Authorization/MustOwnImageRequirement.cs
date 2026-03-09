using Microsoft.AspNetCore.Authorization;

namespace ImageGallery.API.Authorization;

public class MustOwnImageRequirement : IAuthorizationRequirement
{
    // store additional contextual information

    public MustOwnImageRequirement()
    {
    }
}
