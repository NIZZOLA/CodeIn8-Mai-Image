using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using DotNetEnv;

const string endpoint = "https://nizzola-mkt-resource.services.ai.azure.com/mai/v1/images/generations";

Env.Load();

var apiKey = Environment.GetEnvironmentVariable("AZURE_API_KEY");
if (string.IsNullOrWhiteSpace(apiKey))
{
    Console.Error.WriteLine("Environment variable AZURE_API_KEY is not set.");
    return 1;
}

using var http = new HttpClient { Timeout = TimeSpan.FromMinutes(5) };
http.DefaultRequestHeaders.Add("api-key", apiKey);
http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

var payload = new
{
    prompt = "A photograph of earth from space with an big starship as the same model of a Star Wars Cruiser",
    width = 1024,
    height = 1024,
    model = "MAI-Image-2.6"
};

var json = JsonSerializer.Serialize(payload);
var contentBytes = Encoding.UTF8.GetBytes(json);

using var content = new ByteArrayContent(contentBytes);
content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
{
    Content = content
};

using var response = await http.SendAsync(request);
var body = await response.Content.ReadAsStringAsync();

var stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
var responseFile = $"response_{stamp}.txt";
await File.WriteAllTextAsync(responseFile, body);
Console.WriteLine($"Response saved to {responseFile}");

if (!response.IsSuccessStatusCode)
{
    Console.Error.WriteLine($"Request failed: {(int)response.StatusCode} {response.ReasonPhrase}");
    return 1;
}

using var doc = JsonDocument.Parse(body);
var b64 = doc.RootElement.GetProperty("data")[0].GetProperty("b64_json").GetString();
if (string.IsNullOrEmpty(b64))
{
    Console.Error.WriteLine("b64_json is empty.");
    return 1;
}

var imageFile = $"generated_image_{stamp}.png";
await File.WriteAllBytesAsync(imageFile, Convert.FromBase64String(b64));
Console.WriteLine($"Image saved to {imageFile}");
return 0;

