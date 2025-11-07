using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace TesseractApi.Services;

public class TesseractService
{
    private readonly ILogger<TesseractService> logger;
    private static readonly HttpClient httpClient = new HttpClient();

    public TesseractService(ILogger<TesseractService> logger)
    {
        this.logger = logger;
    }
    
    // EasyOCR - Deep learning based OCR (Fast version using HTTP server)
    public async Task<string> RecognizeCaptchaWithEasyOcrAsync(string inputFileName)
    {
        CheckInputFile(inputFileName);

        try
        {
            // Read file and convert to base64
            byte[] imageBytes = await File.ReadAllBytesAsync(inputFileName);
            string base64Image = $"data:image/jpeg;base64,{Convert.ToBase64String(imageBytes)}";
            
            // Call the EasyOCR HTTP server
            var requestData = new { base64 = base64Image };
            var json = JsonSerializer.Serialize(requestData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await httpClient.PostAsync("http://localhost:5001/ocr", content);
            response.EnsureSuccessStatusCode();
            
            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<EasyOcrResponse>(responseJson);
            
            logger.LogInformation("EasyOCR recognized: {Text}", result?.text);
            
            return result?.text?.Trim() ?? string.Empty;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "EasyOCR failed");
            throw;
        }
    }
    
    private class EasyOcrResponse
    {
        public string text { get; set; }
    }

    private static void CheckInputFile(string inputFileName)
    {
        if (string.IsNullOrWhiteSpace(inputFileName)) 
            throw new ArgumentException($"'{nameof(inputFileName)}' cannot be null or whitespace.", nameof(inputFileName));


        string[] permittedDirectories = new string[] { Path.GetTempPath(), "/data/" };

        FileInfo fileInfo = new FileInfo(inputFileName);
        if (!permittedDirectories.Any(permittedDirectory => $"{fileInfo.Directory.FullName}/".StartsWith(permittedDirectory, StringComparison.InvariantCultureIgnoreCase)))
            throw new UnauthorizedAccessException("Input file must be in a permitted directory");

        if (!File.Exists(inputFileName))
            throw new FileNotFoundException("Input file does not exists", inputFileName);

    }
}
