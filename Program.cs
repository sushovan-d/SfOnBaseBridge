using Microsoft.AspNetCore.Http.Features;
using SfOnBaseBridge;

var builder = WebApplication.CreateBuilder(args);

// Load Hyland settings
builder.Services.Configure<HylandSettings>(
    builder.Configuration.GetSection("Hyland"));

builder.Services.AddHttpClient();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers();

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 1024L * 1024L * 1024L; // 1 GB
});

var app = builder.Build();

app.UseHttpsRedirection();

app.UseRouting();

// FIX: CORS should be ATTACHED BEFORE controllers
app.UseCors("AllowAll");

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

// Correct endpoint mapping (after CORS + Auth)
app.MapControllers();

app.Run();
