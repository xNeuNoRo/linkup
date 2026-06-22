using LinkUpPro.Application.Interfaces;
using LinkUpPro.Domain.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LinkUpPro.Infrastructure.Shared.Storage;

public class FileService : IFileService
{
    private readonly FileSettings _settings;
    private readonly ILogger<FileService> _logger;

    public FileService(IOptions<FileSettings> options, ILogger<FileService> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    public string GetAbsolutePath(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return string.Empty;

        string cleanRelativePath = relativePath.TrimStart('/', '\\');
        string baseAbsolutePath = Path.GetFullPath(_settings.BasePath);
        string combinedPath = Path.GetFullPath(
            Path.Combine(baseAbsolutePath, cleanRelativePath)
        );

        if (!combinedPath.StartsWith(baseAbsolutePath, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogCritical(
                "Intento de acceso ilegal fuera del directorio base (Path Traversal): {Path}",
                combinedPath
            );
            return string.Empty;
        }

        return combinedPath;
    }

    public async Task<string> UploadFileAsync(IFormFile file, string folderName)
    {
        try
        {
            string sanitizedFolder = string.Join(
                    "_",
                    folderName.Split(Path.GetInvalidFileNameChars())
                )
                .Replace("..", "")
                .Trim('/', '\\');

            string fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

            string uploadRoot = Path.Combine(
                _settings.UrlPrefix.TrimStart('/', '\\'),
                sanitizedFolder
            );
            string absoluteFolderPath = GetAbsolutePath(uploadRoot);

            if (string.IsNullOrEmpty(absoluteFolderPath))
                throw new InvalidOperationException("La ruta de carga no es valida o es insegura.");

            string relativePath =
                $"/{_settings.UrlPrefix.Trim('/')}/{sanitizedFolder}/{fileName}";

            if (!Directory.Exists(absoluteFolderPath))
                Directory.CreateDirectory(absoluteFolderPath);

            string absoluteFilePath = Path.Combine(absoluteFolderPath, fileName);
            using (var stream = new FileStream(absoluteFilePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            _logger.LogInformation("Archivo guardado exitosamente: {Path}", relativePath);
            return relativePath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error critico al intentar guardar archivo.");
            throw;
        }
    }

    public async Task<string> UploadTempFileAsync(IFormFile file)
    {
        try
        {
            string fileName = $"temp_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            string absoluteTempPath = GetAbsolutePath("temp");

            if (string.IsNullOrEmpty(absoluteTempPath))
                throw new InvalidOperationException("El directorio temporal no es seguro.");

            if (!Directory.Exists(absoluteTempPath))
                Directory.CreateDirectory(absoluteTempPath);

            string fullPath = Path.Combine(absoluteTempPath, fileName);
            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return "temp/" + fileName;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar archivo temporal.");
            throw;
        }
    }

    public bool IsImageValid(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return false;

        if (file.Length > _settings.MaxImageFileSizeBytes)
        {
            _logger.LogWarning("Intento de carga de imagen excediendo tamano: {Size} bytes", file.Length);
            return false;
        }

        string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_settings.AllowedImageExtensions.Contains(extension))
        {
            _logger.LogWarning("Extension de archivo no permitida: {Extension}", extension);
            return false;
        }

        string mimeType = file.ContentType.ToLowerInvariant();
        if (!_settings.AllowedMimeTypes.Contains(mimeType))
        {
            _logger.LogWarning("MIME type no permitido: {MimeType}", mimeType);
            return false;
        }

        return true;
    }

    public void DeleteFile(string filePath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return;

            string absolutePath = GetAbsolutePath(filePath);

            if (File.Exists(absolutePath))
            {
                File.Delete(absolutePath);
                _logger.LogInformation("Archivo eliminado del servidor: {Path}", absolutePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo eliminar el archivo fisico: {Path}.", filePath);
        }
    }
}
