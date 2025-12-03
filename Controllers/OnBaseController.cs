using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;
using SfOnBaseBridge;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json;

[ApiController]
[Route("api/onbase")]
public class OnBaseController : ControllerBase
{
    private readonly IHttpClientFactory _factory;
    private readonly HylandSettings _settings;
    private readonly HttpClient _httpClient;

    public OnBaseController(IHttpClientFactory factory, IOptions<HylandSettings> settings)
    {
        _settings = settings.Value;
        _factory = factory;
        _httpClient = _factory.CreateClient("Upload File");
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload()
    {
        var token = await GetTokenAsync();

        if (string.IsNullOrEmpty(token))
            return Unauthorized("Missing token");

        var client = _factory.CreateClient();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        // OPTIONAL: mimic browser user agent (Hyland dev WAF sometimes requires it)
        client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
        client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent",
            "AppleWebKit/537.36 (KHTML, like Gecko)");
        client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent",
            "Chrome/142.0.0.0 Safari/537.36");

        var url = "https://appintel-dev-test.content.dev.experience.hyland.com/api/upload";

        // Body must be empty JSON, not multipart
        var content = new StringContent("", Encoding.UTF8, "application/json");

        var response = await client.PostAsync(url, content);

        var result = await response.Content.ReadAsStringAsync();

        var doc = JsonDocument.Parse(result);
        var uploadId = doc.RootElement.GetProperty("id").GetString();

        if (uploadId != null)
        {
            return Ok(new { uploadId });
        }
        return StatusCode((int)response.StatusCode, result);
    }

    private async Task<string> GetTokenAsync()
    {
        var body = new Dictionary<string, string>
        {
            { "client_id", _settings.ClientId },
            { "client_secret", _settings.ClientSecret },
            { "grant_type", "client_credentials" }
        };

        var client = _factory.CreateClient();
        var response = await client.PostAsync(
            _settings.TokenUrl,
            new FormUrlEncodedContent(body));

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var token = JsonDocument.Parse(json)
                                .RootElement
                                .GetProperty("access_token")
                                .GetString();

        return token!;
    }
}