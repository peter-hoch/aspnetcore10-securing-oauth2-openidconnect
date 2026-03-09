using ImageGallery.API.DbContexts;
using ImageGallery.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace ImageGallery.API.Services;

public class GalleryRepository(GalleryContext galleryContext) : IGalleryRepository 
{
    public async Task<bool> ImageExistsAsync(Guid id)
    {
        return await galleryContext.Images.AnyAsync(i => i.Id == id);
    }       

    public async Task<Image?> GetImageAsync(Guid id)
    {
        return await galleryContext.Images.FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<IEnumerable<Image>> GetImagesAsync(string ownerId)
    {
        return await galleryContext.Images
            .Where(i => i.OwnerId == ownerId)
            .OrderBy(i => i.Title).ToListAsync();
    }

    public async Task<bool> IsImageOwnerAsync(Guid id, string ownerId)
    {
        return await galleryContext.Images
            .AnyAsync(i => i.Id == id && i.OwnerId == ownerId);
    }

    public void AddImage(Image image)
    {
        galleryContext.Images.Add(image);
    }

    public void UpdateImage(Image image)
    {
        // no code in this implementation
    }

    public void DeleteImage(Image image)
    {
        galleryContext.Images.Remove(image);

        // Note: in a real-life scenario, the image itself potentially should 
        // be removed from disk.  We don't do this in this demo
        // scenario to allow for easier testing / re-running the code
    }

    public async Task<bool> SaveChangesAsync()
    {
        return (await galleryContext.SaveChangesAsync() >= 0);
    } 
}
