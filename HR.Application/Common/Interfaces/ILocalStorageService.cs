using Microsoft.AspNetCore.Http;

namespace HR.Application.Common.Interfaces;

public interface ILocalStorageService
{
    Task<string> SaveFileAsync(IFormFile file, string subDirectory = "uploads");
    void DeleteFile(string fileUrl);
}
