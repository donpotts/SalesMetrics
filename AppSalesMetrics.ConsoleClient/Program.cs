using CsvHelper;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AppSalesMetrics.Shared.Models;

const string baseUrl = "https://localhost:5026";
const string defaultEmail = "normaluser@example.com";
const string defaultPassword = "testUser123!";

Console.WriteLine("AppSalesMetrics.ConsoleClient");

using HttpClient client = new()
{
    BaseAddress = new Uri(baseUrl)
};

string? apiResponse;

apiResponse = await GetTokenAsync(defaultEmail, defaultPassword);
Console.WriteLine(apiResponse);

if (apiResponse == null)
{
    Console.WriteLine("[ERROR] Did not receive token from server");
    return;
}

client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiResponse}");

if (args.Length > 0)
{
    string csvFilePath = args[0]; // assuming the CSV file path is the first argument
    string fileName = Path.GetFileName(csvFilePath).ToLowerInvariant();
    string entityType = fileName.Split('.')[0];
    string apiEndpoint = entityType switch
    {
        "product" => "/api/product",
        "category" => "/api/category",
        "customer" => "/api/customer",
        "order" => "/api/order",
        "orderitem" => "/api/orderitem",
        _ => throw new InvalidOperationException($"Unknown entity type for file: {fileName}")
    };

    using var reader = new StreamReader(csvFilePath);
    using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

    csv.Read();
    csv.ReadHeader();

    while (csv.Read())
    {
        var record = csv.GetRecord<dynamic>();
        var jsonRecord = JsonSerializer.Serialize(record);
        var normalizedJson = NormalizeDates(jsonRecord);
        Console.WriteLine($"Posted record to {apiEndpoint}: " + normalizedJson);
        await PostRecordAsync(normalizedJson, apiEndpoint);
    }
}
else
{
    apiResponse = await GetEntityAllAsync();
    Console.WriteLine(apiResponse);
}

// Create and post a dummy customer record for testing
var dummyCustomer = new
{
    Id = "9999",
    Name = "Test User",
    Email = "testuser@example.com",
    Phone = "555-123-4567",
    Address = "123 Test Lane, Testville, TX 75001",
    Notes = "This is a dummy record for testing.",
    CreatedDate = DateTime.UtcNow
};

//var dummyJson = JsonSerializer.Serialize(dummyCustomer);
//Console.WriteLine("Posting dummy customer: " + dummyJson);
//await PostRecordAsync(dummyJson, "/api/customer");

async Task<string?> GetTokenAsync(
    string email,
    string password)
{
    var response = await client.PostAsJsonAsync("/identity/login", new
    {
        email,
        password
    });

    response.EnsureSuccessStatusCode();

    var content = await response.Content.ReadFromJsonAsync<AccessTokenResponse>();

    return content?.AccessToken;
}

async Task<string> GetEntityAllAsync()
{
    var response = await client.GetStringAsync("/api/product");
    return response;
}

async Task PostRecordAsync(string record, string apiEndpoint)
{
    bool succeeded = false;
    string? key = null;

    // Try to extract the key (Id) from the record for update
    try
    {
        using var doc = JsonDocument.Parse(record);
        if (doc.RootElement.TryGetProperty("Id", out var idProp))
        {
            key = idProp.ToString();
        }
    }
    catch { }

    while (!succeeded)
    {
        var response = await client.PostAsync(apiEndpoint, new StringContent(record, new MediaTypeHeaderValue("application/json")));

        if (response.StatusCode == HttpStatusCode.TooManyRequests && response.Headers.RetryAfter?.Delta != null)
        {
            Console.WriteLine($"[INFO] Rate limited. Retrying after {response.Headers.RetryAfter.Delta.Value}...");
            await Task.Delay(response.Headers.RetryAfter.Delta.Value);
        }
        else if (response.StatusCode == HttpStatusCode.Conflict && key != null)
        {
            // Try update (PUT) if record exists
            var putEndpoint = apiEndpoint.TrimEnd('/') + "/" + key;
            var putResponse = await client.PutAsync(putEndpoint, new StringContent(record, new MediaTypeHeaderValue("application/json")));
            if (putResponse.IsSuccessStatusCode)
            {
                Console.WriteLine($"[INFO] Updated record with Id={key} at {putEndpoint}");
                succeeded = true;
            }
            else
            {
                Console.WriteLine($"[ERROR] Failed to update record with Id={key}: {putResponse.StatusCode}");
                succeeded = true; // Avoid infinite loop
            }
        }
        else
        {
            response.EnsureSuccessStatusCode();
            succeeded = true;
        }
    }
}

string NormalizeDates(string json)
{
    using var doc = JsonDocument.Parse(json);
    var dict = new Dictionary<string, object?>();

    foreach (var prop in doc.RootElement.EnumerateObject())
    {
        if ((prop.Name.EndsWith("Date") || prop.Name.EndsWith("DateTime")) && prop.Value.ValueKind == JsonValueKind.String)
        {
            var str = prop.Value.GetString();
            if (string.IsNullOrWhiteSpace(str))
            {
                dict[prop.Name] = null;
            }
            else if (DateTime.TryParse(str, out var dt))
            {
                dict[prop.Name] = dt.ToString("o"); // ISO 8601
            }
            else
            {
                dict[prop.Name] = str;
            }
        }
        else if (prop.Value.ValueKind == JsonValueKind.Number)
        {
            // Try to keep numbers as int if possible
            if (prop.Value.TryGetInt64(out var l))
                dict[prop.Name] = l;
            else
                dict[prop.Name] = prop.Value.GetDouble();
        }
        else if (prop.Value.ValueKind == JsonValueKind.True || prop.Value.ValueKind == JsonValueKind.False)
        {
            dict[prop.Name] = prop.Value.GetBoolean();
        }
        else if (prop.Value.ValueKind == JsonValueKind.Null)
        {
            dict[prop.Name] = null;
        }
        else
        {
            dict[prop.Name] = prop.Value.GetString();
        }
    }

    return JsonSerializer.Serialize(dict);
}
