using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Configuration;

namespace EvaluatorApp.Services;

public interface ICloudinaryService
{
    Task<string> UploadImage(Stream imageStream, string fileName);
}

public class CloudinaryService : ICloudinaryService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryService(IConfiguration configuration)
    {
        // For prototype simplicity as requested, we are hardcoding the credentials provided by the user.
        // In a production app, these should ultimately come from secure storage or a backend proxy.
        
        var cloudName = "djwpi6z29";
        var apiKey = "181237231871119";
        var apiSecret = "J6ZWFk-oWb4bzyAaBwrDaCaN-3U";

        if (string.IsNullOrEmpty(cloudName) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
        {
            // Fallback or handle error. For now we can initialize with empty or throw if critical.
            // We'll allow it to be created but Upload will fail if not set.
            return;
        }

        var account = new Account(cloudName, apiKey, apiSecret);
        _cloudinary = new Cloudinary(account);
        _cloudinary.Api.Secure = true;
    }

    public async Task<string> UploadImage(Stream imageStream, string fileName)
    {
        if (_cloudinary == null) throw new Exception("Cloudinary configuration is missing.");

        var uploadParams = new ImageUploadParams()
        {
            File = new FileDescription(fileName, imageStream),
            Transformation = new Transformation().Crop("fill").Gravity("face").Width(500).Height(500) // Optional: optimize for profile pics
        };

        var uploadResult = await _cloudinary.UploadAsync(uploadParams);
        return uploadResult.SecureUrl.ToString();
    }
}
