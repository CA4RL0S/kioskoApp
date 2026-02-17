using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Configuration;

namespace KioskoAPI.Services;

public class CloudinaryService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryService(IConfiguration configuration)
    {
        // Hardcoded credentials as per user prototype (copied from EvaluatorApp)
        var cloudName = "djwpi6z29";
        var apiKey = "181237231871119";
        var apiSecret = "J6ZWFk-oWb4bzyAaBwrDaCaN-3U";

        var account = new Account(cloudName, apiKey, apiSecret);
        _cloudinary = new Cloudinary(account);
        _cloudinary.Api.Secure = true;
    }

    public async Task<string> UploadFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is empty");

        using var stream = file.OpenReadStream();
        
        // Determine if it's an image or video/raw based on content type or extension
        // For simplicity, we'll try to detect.
        var fileType = file.ContentType.ToLower();
        
        if (fileType.StartsWith("image"))
        {
            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(file.FileName, stream),
                Transformation = new Transformation().Crop("limit").Width(1000)
            };
            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            return uploadResult.SecureUrl.ToString();
        }
        else if (fileType.StartsWith("video"))
        {
            var uploadParams = new VideoUploadParams()
            {
                File = new FileDescription(file.FileName, stream),
            };
            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            return uploadResult.SecureUrl.ToString();
        }
        else 
        {
            // Default to raw for other types (docs)
             var uploadParams = new RawUploadParams()
            {
                File = new FileDescription(file.FileName, stream),
            };
            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            return uploadResult.SecureUrl.ToString();
        }
    }
}
