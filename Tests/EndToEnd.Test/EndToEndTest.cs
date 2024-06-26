using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using FluentAssertions;
using TestUtils;

namespace EndToEnd.Test;

public class EndToEndTest
{
    protected readonly HttpClient _httpClient;

    public EndToEndTest()
    {
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri("http://localhost:5002");
    }
    
    [Fact]
    public async Task Should_Create_Account()
    {
        var newAccount = new JsonObject
        {
            ["Name"] = "Test Account"    
        };
        
        var accountId = await newAccount.Let(async requestBody =>
        {
            var response = await _httpClient.PostAsJsonAsync("/accounts", requestBody);
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadFromJsonAsync<JsonDocument>();
            responseBody.Should().NotBeNull();
        
            var name = responseBody!.RootElement.GetProperty("name").GetString();
            name.Should().Be("Test Account");
        
            var id = responseBody.RootElement.GetProperty("id").GetString();
            id.Should().NotBeNull();
            return id!;
        });

        accountId.Also(async id =>
        {
            var response = await _httpClient.GetAsync("/accounts/" + id);
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadFromJsonAsync<JsonDocument>();
            responseBody.Should().NotBeNull();
        
            var name = responseBody!.RootElement.GetProperty("name").GetString();
            name.Should().Be("Test Account");
        });
    }
}

public class TestApi
{
    
}