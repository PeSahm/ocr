using Microsoft.AspNetCore.Mvc;
using TesseractApi.Services;

namespace TesseractApi;

public static partial class Extensions
{
    private static readonly string[] AllowedImageContentTypes = 
    { 
        "image/jpeg", "image/jpg", "image/png", "image/gif", 
        "image/bmp", "image/tiff", "image/webp" 
    };


    public static void MapTesseractEndpoints(this WebApplication app)
    {
        var ocr = app.MapGroup("/ocr").DisableAntiforgery();
        //Desabilitado Antiforgery intencionalmente. 
        //Use um API Gateway para proteger a API.
        
        // EasyOCR endpoint (Deep learning based) - File upload
        ocr.MapPost("/captcha-easy", async (IFormFile file, TesseractService tesseractService) =>
        {
            // Validate file input
            if (file == null)
            {
                return Results.BadRequest(new { error = "No file provided" });
            }

            if (file.Length == 0)
            {
                return Results.BadRequest(new { error = "File is empty" });
            }

            // Check file size (10MB limit)
            const long maxFileSize = 10 * 1024 * 1024; // 10MB
            if (file.Length > maxFileSize)
            {
                return Results.BadRequest(new { error = $"File size exceeds maximum allowed size of {maxFileSize / (1024 * 1024)}MB" });
            }

            // Check content type
            if (!AllowedImageContentTypes.Contains(file.ContentType?.ToLowerInvariant()))
            {
                return Results.BadRequest(new { error = $"Invalid file type. Allowed types: {string.Join(", ", AllowedImageContentTypes)}" });
            }

            using DisposableFile disposableFile = await file.SaveFileOnTempDirectory().ConfigureAwait(true);

            string returnValue = await tesseractService.RecognizeCaptchaWithEasyOcrAsync(disposableFile.File.FullName);

            return Results.Ok(returnValue?.Trim());
        }).WithName("RecognizeCaptchaEasy");
        
        // EasyOCR endpoint (Deep learning based) - Base64 input
        ocr.MapPost("/captcha-easy-base64", async ([FromBody]Base64File request, TesseractService tesseractService) =>
        {
            // Validate base64 input
            if (request == null || string.IsNullOrWhiteSpace(request.Base64))
            {
                return Results.BadRequest(new { error = "Base64 image data is required and cannot be empty" });
            }

            try
            {
                using DisposableFile disposableFile = await request.Base64.SaveFileOnTempDirectory().ConfigureAwait(true);

                string returnValue = await tesseractService.RecognizeCaptchaWithEasyOcrAsync(disposableFile.File.FullName);

                return Results.Ok(returnValue?.Trim());
            }
            catch (FormatException)
            {
                return Results.BadRequest(new { error = "Invalid base64 format. Please provide a valid base64 encoded image" });
            }
            catch (ArgumentException ex) when (ex.Message.Contains("base64", StringComparison.OrdinalIgnoreCase))
            {
                return Results.BadRequest(new { error = "Invalid base64 format. Please provide a valid base64 encoded image" });
            }
        }).WithName("RecognizeCaptchaEasyBase64");
    }

}
public class Base64File
{
    public string Base64 { get; set; }
}
