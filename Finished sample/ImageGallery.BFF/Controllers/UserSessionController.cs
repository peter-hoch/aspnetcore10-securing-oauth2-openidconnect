using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ImageGallery.BFF.Controllers;

[Authorize]
public class UserSessionController : Controller
{
    public IActionResult GivenName()
    {
        var objectToReturn = new
        {
            given_name = User.Claims
                .FirstOrDefault(c => c.Type == "given_name")?.Value
        };

        return Json(objectToReturn);
    }

    public async Task Logout()
    {
        await HttpContext.SignOutAsync("BFFCookieScheme");
        await HttpContext.SignOutAsync("BFFChallengeScheme");
    }
}
