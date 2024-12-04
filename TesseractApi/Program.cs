using TesseractApi;
using TesseractApi.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<TesseractService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

WebApplication app = builder.Build();

app.MapTesseractEndpoints();

app.UseSwagger();
app.UseSwaggerUI();
app.Run();

