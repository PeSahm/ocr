using Ocr;
using Ocr.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<TesseractService>();
builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();

WebApplication app = builder.Build();

app.MapTesseractEndpoints();
app.UseSwagger();
app.UseSwaggerUI();
app.Run();

