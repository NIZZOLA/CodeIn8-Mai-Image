using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using DotNetEnv;

// Load environment variables from .env file and validate required settings.
Env.Load();
var endpoint = Environment.GetEnvironmentVariable("FOUNDRY_ENDPOINT");
if (string.IsNullOrWhiteSpace(endpoint))
{
    Console.Error.WriteLine("Environment variable FOUNDRY_ENDPOINT is not set.");
    return 1;
}

var apiKey = Environment.GetEnvironmentVariable("AZURE_API_KEY");
if (string.IsNullOrWhiteSpace(apiKey))
{
    Console.Error.WriteLine("Environment variable AZURE_API_KEY is not set.");
    return 1;
}
// Create and configure the HTTP client.
using var http = new HttpClient { Timeout = TimeSpan.FromMinutes(5) };
http.DefaultRequestHeaders.Add("api-key", apiKey);
http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
// Prepare the payload for the image generation request.
var payload = new
{
    prompt = "Uma postagem para o instagram, sobre um artigo onde usando Microsoft Ai Foundry, C Sharp é feita uma demonstração sobre como gerar imagens utilizando Inteligência Artificial, nesta postagem podemos ter como imagem de fundo um robô como um pintor, desenhando sobre uma tela uma imagem de um planeta Terra com uma nave orbitando ao seu redor. Colocar como título 'Usando C# para criação de imagens de I.A. com MAI-Image' incluir no rodapé logos de Azure, Azure Ai Foundry, C Sharp, e colocar uma referencia na inferior direta '@nizzola.dev'",
    width = 1080,
    height = 1350,
    model = "MAI-Image-2.6"
};
// Serialize the payload to JSON and prepare the HTTP content.
var json = JsonSerializer.Serialize(payload);
var contentBytes = Encoding.UTF8.GetBytes(json);

using var content = new ByteArrayContent(contentBytes);
content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
{
    Content = content
};
// Send the HTTP request and get the response.
Console.WriteLine("Sending request to generate image...");
using var response = await http.SendAsync(request);
var body = await response.Content.ReadAsStringAsync();
// Save the raw response to a file for debugging purposes.
var stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
var responseFile = $"response_{stamp}.txt";
await File.WriteAllTextAsync(responseFile, body);
Console.WriteLine($"Response saved to {responseFile}");

// Check if the response indicates a successful request.
if (!response.IsSuccessStatusCode)
{
    Console.Error.WriteLine($"Request failed: {(int)response.StatusCode} {response.ReasonPhrase}");
    return 1;
}
// Parse the JSON response and extract the base64-encoded image data.
using var doc = JsonDocument.Parse(body);
var b64 = doc.RootElement.GetProperty("data")[0].GetProperty("b64_json").GetString();
if (string.IsNullOrEmpty(b64))
{
    Console.Error.WriteLine("b64_json is empty.");
    return 1;
}
// Convert the base64-encoded image data to a PNG file and save it.
var imageFile = $"generated_image_{stamp}.png";
await File.WriteAllBytesAsync(imageFile, Convert.FromBase64String(b64));
Console.WriteLine($"Image saved to {imageFile}");

// Read token counts from the response JSON format returned by the API.
var usage = doc.RootElement.GetProperty("usage");
var outputTokens = usage.TryGetProperty("num_output_tokens", out var outputTokensElement) ? outputTokensElement.GetInt32() : 0;
var inputTextTokens = usage.TryGetProperty("num_input_text_tokens", out var inputTextTokensElement) ? inputTextTokensElement.GetInt32() : 0;
var inputImageTokens = usage.TryGetProperty("num_input_image_tokens", out var inputImageTokensElement) ? inputImageTokensElement.GetInt32() : 0;

Console.WriteLine($"Tokens used: output={outputTokens}, input_text={inputTextTokens}, input_image={inputImageTokens}");
return 0;

