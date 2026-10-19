using Microsoft.AspNetCore.Http;

namespace AysuOgel.Web.Helpers;

public static class ImageUploadHelper
{
    public const long MaxImageBytes = 5 * 1024 * 1024;

    public static async Task<string?> SaveImageAsync(IFormFile file, string webRootPath)
    {
        if (file.Length <= 0 || file.Length > MaxImageBytes)
            return null;

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension is not (".jpg" or ".jpeg" or ".png" or ".webp"))
            return null;

        await using var stream = file.OpenReadStream();
        var header = new byte[12];
        var read = await stream.ReadAsync(header.AsMemory(0, header.Length));
        if (!HasValidImageSignature(header, read, extension))
            return null;

        var uploadsPath = Path.Combine(webRootPath, "uploads");
        Directory.CreateDirectory(uploadsPath);

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(uploadsPath, fileName);
        stream.Position = 0;

        await using (var output = File.Create(fullPath))
        {
            await stream.CopyToAsync(output);
        }

        return $"/uploads/{fileName}";
    }

    private static bool HasValidImageSignature(byte[] bytes, int length, string extension)
    {
        if (extension is ".jpg" or ".jpeg")
            return length >= 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF;

        if (extension == ".png")
            return length >= 8 &&
                   bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47 &&
                   bytes[4] == 0x0D && bytes[5] == 0x0A && bytes[6] == 0x1A && bytes[7] == 0x0A;

        if (extension == ".webp")
            return length >= 12 &&
                   bytes[0] == (byte)'R' && bytes[1] == (byte)'I' &&
                   bytes[2] == (byte)'F' && bytes[3] == (byte)'F' &&
                   bytes[8] == (byte)'W' && bytes[9] == (byte)'E' &&
                   bytes[10] == (byte)'B' && bytes[11] == (byte)'P';

        return false;
    }
}