using Microsoft.AspNetCore.Mvc;
using Ocr.Services;

namespace Ocr
{
    public static partial class Extensions
    {


        public static void MapTesseractEndpoints(this WebApplication app)
        {
            RouteGroupBuilder tesseract = app.MapGroup("/ocr").DisableAntiforgery();

            tesseract.MapPost("/by-base64", ([FromBody]Base64String request, TesseractService tesseractService) =>
            {
                (int exitCode, string output, string error) = tesseractService.ExecuteTesseractProcessAsync(request.Base64);

                return exitCode switch
                {
                    0 => Results.Ok(output),
                    _ => Results.BadRequest(error)
                };
            });
        }
    }
    public class Base64String
    {
        public string Base64 { get; set; }
    }
}