using TesseractApi;
using TesseractApi.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Configure EasyOCR options
builder.Services.Configure<EasyOcrOptions>(builder.Configuration.GetSection("EasyOCR"));

builder.Services.AddSingleton<TesseractService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

WebApplication app = builder.Build();

app.MapTesseractEndpoints();

app.UseSwagger();
app.UseSwaggerUI();
app.Run();

