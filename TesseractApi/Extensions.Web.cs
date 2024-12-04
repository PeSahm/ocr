using Microsoft.AspNetCore.Mvc;
using TesseractApi.Services;

namespace TesseractApi;

public static partial class Extensions
{


    public static void MapTesseractEndpoints(this WebApplication app)
    {
        var tesseract = app.MapGroup("/ocr").DisableAntiforgery();
        //Desabilitado Antiforgery intencionalmente. 
        //Use um API Gateway para proteger a API.

        tesseract.MapGet("/", async (TesseractService tesseractService) =>
        {
            return await tesseractService.GetVersionAsync();
        });

        tesseract.MapPost("/by-upload", async (IFormFile file, TesseractService tesseractService) =>
        {
            using DisposableFile disposableFile = await file.SaveFileOnTempDirectory().ConfigureAwait(true);

            string returnValue = await tesseractService.GetTextOfImageFileAsync(disposableFile.File.FullName);

            return returnValue;
        });

        tesseract.MapPost("/by-filepath", async ([FromForm]string fileName, TesseractService tesseractService) =>
        {
            string returnValue = await tesseractService.GetTextOfImageFileAsync(fileName);

            return returnValue;
        });
        tesseract.MapPost("/by-base64", async ([FromBody]Base64File request, TesseractService tesseractService) =>
        {
            using DisposableFile disposableFile = await request.Base64.SaveFileOnTempDirectory().ConfigureAwait(true);

            string returnValue = await tesseractService.GetTextOfImageFileAsync(disposableFile.File.FullName);

            return returnValue;
        });
    }

}
public class Base64File
{
    public string Base64 { get; set; }
}
