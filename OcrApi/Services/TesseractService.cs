using Tesseract;

namespace Ocr.Services;

public class TesseractService(ILogger<TesseractService> logger)
{
    public (int exitCode, string output, string error) ExecuteTesseractProcessAsync(string base64string)
    {
        try
        {
            string folderName = "tessdata"; // Name of the folder
            string folderPath = Path.Combine(AppContext.BaseDirectory, folderName);

            if (!Directory.Exists(folderPath))
            {
                return (1, null, "Tessdata folder not found");
            }

            byte[] imageData = Convert.FromBase64String(base64string);
            using TesseractEngine engine = new(folderName, "eng", EngineMode.TesseractOnly);
            engine.SetVariable("tessedit_char_whitelist", "0123456789");
            engine.SetVariable("tessedit_char_blacklist", "abcdefghijklmnopqrstuvwxyz!@#$%^&*()_+=-][}{|/?.,<> ");
            engine.DefaultPageSegMode = PageSegMode.SingleWord;
            using Page page = engine.Process(Pix.LoadFromMemory(imageData));
            string captchaValue = page.GetText();
            return (0, captchaValue.Trim(), null);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error in TesseractService");
            return (1, null, e.Message);
        }
    }
}