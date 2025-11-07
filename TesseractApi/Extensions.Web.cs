using Microsoft.AspNetCore.Mvc;
using TesseractApi.Services;

namespace TesseractApi;

public static partial class Extensions
{


    public static void MapTesseractEndpoints(this WebApplication app)
    {
        var ocr = app.MapGroup("/ocr").DisableAntiforgery();
        //Desabilitado Antiforgery intencionalmente. 
        //Use um API Gateway para proteger a API.
        
        // EasyOCR endpoint (Deep learning based) - File upload
        ocr.MapPost("/captcha-easy", async (IFormFile file, TesseractService tesseractService) =>
        {
            using DisposableFile disposableFile = await file.SaveFileOnTempDirectory().ConfigureAwait(true);

            string returnValue = await tesseractService.RecognizeCaptchaWithEasyOcrAsync(disposableFile.File.FullName);

            return returnValue?.Trim();
        }).WithName("RecognizeCaptchaEasy");
        
        // EasyOCR endpoint (Deep learning based) - Base64 input
        ocr.MapPost("/captcha-easy-base64", async ([FromBody]Base64File request, TesseractService tesseractService) =>
        {
            using DisposableFile disposableFile = await request.Base64.SaveFileOnTempDirectory().ConfigureAwait(true);

            string returnValue = await tesseractService.RecognizeCaptchaWithEasyOcrAsync(disposableFile.File.FullName);

            return returnValue?.Trim();
        }).WithName("RecognizeCaptchaEasyBase64");
    }

}
public class Base64File
{
    public string Base64 { get; set; }
}
