using Microsoft.Win32.SafeHandles;
using System.Diagnostics;
using TesseractApi.Services;

namespace TesseractApi;

public static partial class Extensions
{
    public static async Task<DisposableFile> SaveFileOnTempDirectory(this IFormFile formFile)
    {
        ArgumentNullException.ThrowIfNull(formFile);

        //não pode usar o using aqui, pois o arquivo será deletado antes de ser lido
        DisposableFile disposableFile = formFile.GetTempFileName();

        await formFile.WriteOnFile(disposableFile).ConfigureAwait(false);

        return disposableFile;
    }

    public static async Task<DisposableFile> SaveFileOnTempDirectory(this string base64String)
    {
        // Decode the Base64 string into a byte array
        byte[] fileBytes = Convert.FromBase64String(base64String);

        // Create a memory stream from the byte array
        MemoryStream stream = new(fileBytes);

        // Create a FormFile instance
        IFormFile formFile = new FormFile(stream, 0, fileBytes.Length, "file", "captcha")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/png"
        };

        return await formFile.SaveFileOnTempDirectory();
    }

    private static async Task WriteOnFile(this IFormFile formFile, FileInfo fileInfo)
    {
        using FileStream stream = fileInfo.OpenWrite();

        await formFile.CopyToAsync(stream).ConfigureAwait(false);
    }

    private static string GetTempFileName(this IFormFile file)
    {
        return Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():D}-{file.FileName}");
    }
}