using Microsoft.AspNetCore.Http.Features;
using SfOnBaseBridge;

var builder = WebApplication.CreateBuilder(args);

// Bind Hyland config
builder.Services.Configure<HylandSettings>(
    builder.Configuration.GetSection("Hyland"));

// HTTP + controllers
builder.Services.AddHttpClient();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// File limits
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 1024L * 1024L * 1024L; // 1GB
});

// Cloud Foundry Kestrel port binding
builder.WebHost.ConfigureKestrel(options =>
{
    var port = Environment.GetEnvironmentVariable("PORT");
    options.ListenAnyIP(port is null ? 8080 : int.Parse(port));
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

app.UseRouting();
app.UseCors("AllowAll");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

app.Run();