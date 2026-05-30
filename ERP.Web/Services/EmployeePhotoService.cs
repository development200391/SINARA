using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ERP.Web.Services;

public sealed class EmployeePhotoService(IWebHostEnvironment environment) : IEmployeePhotoService
{
    private const long MaxFileSizeBytes = 5 * 1024 * 1024;
    private const int OutputSize = 300;
    private const int WebpQuality = 82;

    private static readonly HashSet<string> AllowedExtensions = [".jpg", ".jpeg", ".png"];
    private static readonly HashSet<string> AllowedMimeTypes = ["image/jpeg", "image/jpg", "image/pjpeg", "image/png"];

    public async Task<string> SavePhotoAsync(IFormFile file, EmployeePhotoCropData cropData, CancellationToken ct = default)
    {
        ValidateFile(file);

        await using var inputStream = file.OpenReadStream();
        var format = await Image.DetectFormatAsync(inputStream, ct);
        if (format is null || !IsSupportedFormat(format))
        {
            throw new InvalidOperationException("Only JPG, JPEG, or PNG images are allowed.");
        }

        inputStream.Position = 0;
        using var image = await Image.LoadAsync<Rgba32>(inputStream, ct);

        var cropRectangle = BuildCropRectangle(image.Width, image.Height, cropData);
        image.Mutate(context =>
        {
            context.Crop(cropRectangle);
            context.Resize(OutputSize, OutputSize);
        });

        var uploadDirectory = GetUploadDirectory();
        Directory.CreateDirectory(uploadDirectory);

        var fileName = $"{Guid.NewGuid():N}.webp";
        var filePath = Path.Combine(uploadDirectory, fileName);

        var encoder = new WebpEncoder
        {
            Quality = WebpQuality,
            Method = WebpEncodingMethod.Default,
            FileFormat = WebpFileFormatType.Lossy
        };

        await image.SaveAsWebpAsync(filePath, encoder, ct);

        return $"/uploads/employees/{fileName}";
    }

    public Task DeletePhotoAsync(string? photoPath, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(photoPath))
        {
            return Task.CompletedTask;
        }

        var normalizedPath = photoPath.Trim();
        if (!normalizedPath.StartsWith("/uploads/employees/", StringComparison.OrdinalIgnoreCase))
        {
            return Task.CompletedTask;
        }

        var webRootPath = ResolveWebRootPath();
        var uploadRootPath = Path.GetFullPath(Path.Combine(webRootPath, "uploads", "employees"));

        var relativePath = normalizedPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.GetFullPath(Path.Combine(webRootPath, relativePath));

        if (!fullPath.StartsWith(uploadRootPath, StringComparison.OrdinalIgnoreCase))
        {
            return Task.CompletedTask;
        }

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    private static bool IsSupportedFormat(IImageFormat format)
    {
        return string.Equals(format.Name, JpegFormat.Instance.Name, StringComparison.OrdinalIgnoreCase)
            || string.Equals(format.Name, PngFormat.Instance.Name, StringComparison.OrdinalIgnoreCase);
    }

    private static Rectangle BuildCropRectangle(int imageWidth, int imageHeight, EmployeePhotoCropData cropData)
    {
        if (imageWidth <= 0 || imageHeight <= 0)
        {
            throw new InvalidOperationException("Image has invalid dimensions.");
        }

        if (cropData.Width.GetValueOrDefault() <= 0 || cropData.Height.GetValueOrDefault() <= 0)
        {
            var side = Math.Min(imageWidth, imageHeight);
            return new Rectangle((imageWidth - side) / 2, (imageHeight - side) / 2, side, side);
        }

        var x = (int)Math.Floor(cropData.X.GetValueOrDefault());
        var y = (int)Math.Floor(cropData.Y.GetValueOrDefault());
        var width = (int)Math.Round(cropData.Width.GetValueOrDefault());
        var height = (int)Math.Round(cropData.Height.GetValueOrDefault());

        x = Math.Clamp(x, 0, Math.Max(0, imageWidth - 1));
        y = Math.Clamp(y, 0, Math.Max(0, imageHeight - 1));

        width = Math.Clamp(width, 1, imageWidth - x);
        height = Math.Clamp(height, 1, imageHeight - y);

        var sideLength = Math.Min(width, height);
        x += (width - sideLength) / 2;
        y += (height - sideLength) / 2;

        x = Math.Clamp(x, 0, Math.Max(0, imageWidth - sideLength));
        y = Math.Clamp(y, 0, Math.Max(0, imageHeight - sideLength));

        return new Rectangle(x, y, sideLength, sideLength);
    }

    private static void ValidateFile(IFormFile file)
    {
        if (file.Length <= 0)
        {
            throw new InvalidOperationException("Photo file is empty.");
        }

        if (file.Length > MaxFileSizeBytes)
        {
            throw new InvalidOperationException("Photo file must be 5 MB or smaller.");
        }

        var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException("Only JPG, JPEG, or PNG files are allowed.");
        }

        var contentType = file.ContentType?.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(contentType) || !AllowedMimeTypes.Contains(contentType))
        {
            throw new InvalidOperationException("Invalid image MIME type. Allowed: image/jpeg, image/png.");
        }
    }

    private string GetUploadDirectory()
    {
        var webRootPath = ResolveWebRootPath();
        return Path.Combine(webRootPath, "uploads", "employees");
    }

    private string ResolveWebRootPath()
    {
        if (!string.IsNullOrWhiteSpace(environment.WebRootPath))
        {
            return environment.WebRootPath;
        }

        return Path.Combine(environment.ContentRootPath, "wwwroot");
    }
}