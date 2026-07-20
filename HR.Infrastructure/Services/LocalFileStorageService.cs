using HR.Application.Common.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace HR.Infrastructure.Services;

public class LocalFileStorageService : ILocalStorageService
{
    private readonly IWebHostEnvironment _env;

    public LocalFileStorageService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<string> SaveFileAsync(IFormFile file, string subDirectory = "uploads")
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is empty.");

        // We store files in wwwroot/uploads/...
        var uploadsFolder = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), subDirectory);

        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        // Generate a unique filename
        var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName.Replace(" ", "_")}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }

        // Return the relative URL so it can be served over HTTP
        return $"/{subDirectory}/{uniqueFileName}";
    }

    public void DeleteFile(string fileUrl)
    {
        if (string.IsNullOrEmpty(fileUrl)) return;

        // Convert the URL back to a physical path
        var relativePath = fileUrl.TrimStart('/');
        var filePath = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), relativePath);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }
}
