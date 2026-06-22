using Microsoft.AspNetCore.Http;

namespace LinkUpPro.Application.Interfaces;

public interface IFileService
{
    string GetAbsolutePath(string relativePath);

    Task<string> UploadFileAsync(IFormFile file, string folderName);

    Task<string> UploadTempFileAsync(IFormFile file);

    bool IsImageValid(IFormFile file);

    void DeleteFile(string filePath);
}
