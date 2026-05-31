using ImageGallery.Client.Services;
using ImageGallery.Client.ViewModels;
using ImageGallery.Model;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc; 
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;

namespace ImageGallery.Client.Controllers;

[Authorize]
public class GalleryController(IHttpClientFactory httpClientFactory, ITokenInformationLogger tokenInformationLogger) : Controller
{
    public async Task<IActionResult> Index()
    {
        // log token information - only for demonstration NOT for production
        await tokenInformationLogger.Log();

        var httpClient = httpClientFactory.CreateClient("APIClient");
        
        var request = new HttpRequestMessage(
            HttpMethod.Get,
            "/api/images/");

        var response = await httpClient.SendAsync(
            request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

        response.EnsureSuccessStatusCode();

        using (var responseStream = await response.Content.ReadAsStreamAsync())
        {
            var images = await JsonSerializer.DeserializeAsync<List<Image>>(responseStream);
            return View(new GalleryIndexViewModel(images ?? new List<Image>()));
        }
    }

    public async Task<IActionResult> EditImage(Guid id)
    {

        var httpClient = httpClientFactory.CreateClient("APIClient");

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/api/images/{id}");

        var response = await httpClient.SendAsync(
            request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

        response.EnsureSuccessStatusCode();

        using (var responseStream = await response.Content.ReadAsStreamAsync())
        {
            var deserializedImage = await JsonSerializer.DeserializeAsync<Image>(responseStream);

            if (deserializedImage == null)
            {
                throw new Exception("Deserialized image must not be null.");
            }

            var editImageViewModel = new EditImageViewModel()
            {
                Id = deserializedImage.Id,
                Title = deserializedImage.Title
            };

            return View(editImageViewModel);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditImage(EditImageViewModel editImageViewModel)
    {
        if (!ModelState.IsValid)
        {
            return View();
        }

        // create an ImageForUpdate instance
        var imageForUpdate = new ImageForUpdate(editImageViewModel.Title);

        // serialize it
        var serializedImageForUpdate = JsonSerializer.Serialize(imageForUpdate);

        var httpClient = httpClientFactory.CreateClient("APIClient");

        var request = new HttpRequestMessage(
            HttpMethod.Put,
            $"/api/images/{editImageViewModel.Id}")
        {
            Content = new StringContent(
                serializedImageForUpdate,
                System.Text.Encoding.Unicode,
                "application/json")
        };

        var response = await httpClient.SendAsync(
            request, HttpCompletionOption.ResponseHeadersRead);

        response.EnsureSuccessStatusCode();

        return RedirectToAction("Index");
    }

    public async Task<IActionResult> DeleteImage(Guid id)
    {
        var httpClient = httpClientFactory.CreateClient("APIClient");

        var request = new HttpRequestMessage(
            HttpMethod.Delete,
            $"/api/images/{id}");

        var response = await httpClient.SendAsync(
            request, HttpCompletionOption.ResponseHeadersRead);

        response.EnsureSuccessStatusCode();

        return RedirectToAction("Index");
    }

    [Authorize(Policy = "UserCanAddImage")]
    public IActionResult AddImage()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "UserCanAddImage")]
    public async Task<IActionResult> AddImage(AddImageViewModel addImageViewModel)
    {
        if (!ModelState.IsValid)
        {
            return View();
        }

        // create an ImageForCreation instance
        ImageForCreation? imageForCreation = null;

        // take the first (only) file in the Files list
        var imageFile = addImageViewModel.Files.First();

        if (imageFile.Length > 0)
        {
            using (var fileStream = imageFile.OpenReadStream())
            using (var ms = new MemoryStream())
            {
                fileStream.CopyTo(ms);
                imageForCreation = new ImageForCreation(
                    addImageViewModel.Title, ms.ToArray());
            }
        }

        // serialize it
        var serializedImageForCreation = JsonSerializer.Serialize(imageForCreation);

        var httpClient = httpClientFactory.CreateClient("APIClient");

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/api/images")
        {
            Content = new StringContent(
                serializedImageForCreation,
                System.Text.Encoding.Unicode,
                "application/json")
        };

        var response = await httpClient.SendAsync(
            request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

        response.EnsureSuccessStatusCode();

        return RedirectToAction("Index");
    }
}
