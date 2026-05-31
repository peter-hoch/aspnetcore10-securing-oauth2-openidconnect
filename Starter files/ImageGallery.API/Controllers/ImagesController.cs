using AutoMapper;
using ImageGallery.API.Services;
using ImageGallery.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ImageGallery.API.Controllers;

[Route("api/images")]
[ApiController]
[Authorize]
public class ImagesController(
    IGalleryRepository galleryRepository,
    IWebHostEnvironment hostingEnvironment,
    IMapper mapper) : ControllerBase
{ 

    [HttpGet()]
    public async Task<ActionResult<IEnumerable<Image>>> GetImages()
    {
        // get from repo
        var imagesFromRepo = await galleryRepository.GetImagesAsync();

        // map to model
        var imagesToReturn = mapper.Map<IEnumerable<Image>>(imagesFromRepo);

        // return
        return Ok(imagesToReturn);
    }

    [HttpGet("{id}", Name = "GetImage")]
    public async Task<ActionResult<Image>> GetImage(Guid id)
    {          
        var imageFromRepo = await galleryRepository.GetImageAsync(id);

        if (imageFromRepo == null)
        {
            return NotFound();
        }

        var imageToReturn = mapper.Map<Image>(imageFromRepo);
        return Ok(imageToReturn);
    }

    [HttpPost()]
    public async Task<ActionResult<Image>> CreateImage([FromBody] ImageForCreation imageForCreation)
    {
        // Automapper maps only the Title in our configuration
        var imageEntity = mapper.Map<Entities.Image>(imageForCreation);

        // Create an image from the passed-in bytes (Base64), and 
        // set the filename on the image

        // get this environment's web root path (the path
        // from which static content, like an image, is served)
        var webRootPath = hostingEnvironment.WebRootPath;

        // create the filename
        string fileName = Guid.NewGuid().ToString() + ".jpg";

        // the full file path
        var filePath = Path.Combine($"{webRootPath}/images/{fileName}");

        // write bytes and auto-close stream
        await System.IO.File.WriteAllBytesAsync(filePath, imageForCreation.Bytes);

        // fill out the filename
        imageEntity.FileName = fileName;

        // ownerId should be set - can't save image in starter solution, will
        // be fixed during the course
        //imageEntity.OwnerId = ...;

        // add and save.  
        galleryRepository.AddImage(imageEntity);

        await galleryRepository.SaveChangesAsync();

        var imageToReturn = mapper.Map<Image>(imageEntity);

        return CreatedAtRoute("GetImage",
            new { id = imageToReturn.Id },
            imageToReturn);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteImage(Guid id)
    {            
        var imageFromRepo = await galleryRepository.GetImageAsync(id);

        if (imageFromRepo == null)
        {
            return NotFound();
        }

        galleryRepository.DeleteImage(imageFromRepo);

        await galleryRepository.SaveChangesAsync();

        return NoContent();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateImage(Guid id, 
        [FromBody] ImageForUpdate imageForUpdate)
    {
        var imageFromRepo = await galleryRepository.GetImageAsync(id);
        if (imageFromRepo == null)
        {
            return NotFound();
        }

        mapper.Map(imageForUpdate, imageFromRepo);

        galleryRepository.UpdateImage(imageFromRepo);

        await galleryRepository.SaveChangesAsync();

        return NoContent();
    }
}