using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
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

    public async Task<string> GetVersionAsync()
    {
        var (exitCode, output, error) = await ExecuteTesseractProcessAsync("--version");
        return output;
    }

    public async Task<string> GetTextOfImageFileAsync(string inputFileName)
    {
        CheckInputFile(inputFileName);

        string outputFileNameWithoutExtension = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("D"));
        using DisposableFile outputFile = $"{outputFileNameWithoutExtension}.txt";

        await ExecuteTesseractProcessAsync($"\"{inputFileName}\" {outputFileNameWithoutExtension} -c tessedit_char_whitelist=0123456789 -l eng --oem 3 --psm 6");

        string returnValue = outputFile.ReadAllText();
        return returnValue;
    }
    
    // Optimized method for simple numeric captchas
    public async Task<string> RecognizeCaptchaAsync(string inputFileName)
    {
        CheckInputFile(inputFileName);

        string outputFileNameWithoutExtension = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("D"));
        using DisposableFile outputFile = $"{outputFileNameWithoutExtension}.txt";

        // Optimized parameters for numeric captcha:
        // --oem 1 = LSTM neural net mode (faster and more accurate)
        // --psm 7 = Treat the image as a single text line
        // -c tessedit_char_whitelist=0123456789 = Only recognize digits
        await ExecuteTesseractProcessAsync($"\"{inputFileName}\" {outputFileNameWithoutExtension} -c tessedit_char_whitelist=0123456789 -l eng --oem 1 --psm 7");

        string returnValue = outputFile.ReadAllText();
        
        logger.LogInformation("Captcha recognized: {Text}", returnValue?.Trim());
        
        return returnValue;
    }
    
    // PaddleOCR - AI-based OCR (usually more accurate)
    public async Task<string> RecognizeCaptchaWithPaddleOcrAsync(string inputFileName)
    {
        CheckInputFile(inputFileName);

        var scriptPath = Path.Combine(AppContext.BaseDirectory, "PythonOCR", "paddle_ocr.py");
        var (exitCode, output, error) = await ExecutePythonProcessAsync($"\"{scriptPath}\" \"{inputFileName}\"");
        
        logger.LogInformation("PaddleOCR recognized: {Text}", output?.Trim());
        
        return output?.Trim() ?? string.Empty;
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
            
            // Call the EasyOCR HTTP server (much faster - model stays loaded)
            var requestData = new { base64 = base64Image };
            var json = JsonSerializer.Serialize(requestData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await httpClient.PostAsync("http://localhost:5001/ocr", content);
            response.EnsureSuccessStatusCode();
            
            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<EasyOcrResponse>(responseJson);
            
            logger.LogInformation("EasyOCR (Fast) recognized: {Text}", result?.text);
            
            return result?.text?.Trim() ?? string.Empty;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "EasyOCR Fast failed, falling back to script");
            
            // Fallback to script-based approach
            var scriptPath = Path.Combine(AppContext.BaseDirectory, "PythonOCR", "easy_ocr.py");
            var (exitCode, output, error) = await ExecutePythonProcessAsync($"\"{scriptPath}\" \"{inputFileName}\"");
            
            return output?.Trim() ?? string.Empty;
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

    private async Task<(int exitCode, string output, string error)> ExecuteTesseractProcessAsync(string args)
    {
        var tesseractCreateInfo = new ProcessStartInfo("tesseract", args)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        var tesseractProcess = Process.Start(tesseractCreateInfo);

        string output = tesseractProcess.StandardOutput.ReadToEnd();

        string error = tesseractProcess.StandardError.ReadToEnd();

        await tesseractProcess.WaitForExitAsync();

        int exitCode = tesseractProcess.ExitCode;

        StringBuilder stringBuilder = new StringBuilder()
            .AppendLine(exitCode == 0 ? "Success" : "Error")
            .AppendLine(CultureInfo.InvariantCulture, $"ExitCode: {exitCode}")
            .AppendLine(CultureInfo.InvariantCulture, $"Executed Process: '{tesseractCreateInfo.FileName}'")
            .AppendLine(CultureInfo.InvariantCulture, $"Args: '{tesseractCreateInfo.Arguments}'")
            .AppendLine(CultureInfo.InvariantCulture, $"StdOut: '{output}'")
            .AppendLine(CultureInfo.InvariantCulture, $"StdErr: '{error}'");

        string logMessage = stringBuilder.ToString();

        if (exitCode != 0)
        {
            logger.LogError(logMessage);

            throw new InvalidOperationException($"Error on execute {tesseractCreateInfo.FileName} with args '{tesseractCreateInfo.Arguments}', exit code {tesseractProcess.ExitCode}. Output: '{output}' Error: '{error}'");
        }

        logger.LogInformation(logMessage);

        return (exitCode, output, error);
    }
    
    private async Task<(int exitCode, string output, string error)> ExecutePythonProcessAsync(string args)
    {
        var pythonCreateInfo = new ProcessStartInfo("python3", args)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        var pythonProcess = Process.Start(pythonCreateInfo);

        string output = pythonProcess.StandardOutput.ReadToEnd();
        string error = pythonProcess.StandardError.ReadToEnd();

        await pythonProcess.WaitForExitAsync();

        int exitCode = pythonProcess.ExitCode;

        if (exitCode != 0)
        {
            logger.LogError("Python OCR Error: {Error}", error);
            throw new InvalidOperationException($"Python OCR failed with exit code {exitCode}. Error: '{error}'");
        }

        return (exitCode, output, error);
    }
}
