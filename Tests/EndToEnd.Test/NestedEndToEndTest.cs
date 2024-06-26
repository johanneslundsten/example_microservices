using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using FluentAssertions;

namespace EndToEnd.Test;

public class NestedEndToEndTest
{
    private readonly HttpClient _httpClient;

    public NestedEndToEndTest()
    {
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri("http://localhost:5002");
    }
    
    [Fact]
    public async Task Should_Get_404_On_Invalid_Account_Id()
    {
        var response = await _httpClient.GetAsync("/accounts/invalid_account_id");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    public class WithAccount
    {
        private readonly string _accountId;
        private readonly HttpClient _httpClient;

        public WithAccount()
        {
            var nestedEndToEndTest = new NestedEndToEndTest();
            _httpClient = nestedEndToEndTest._httpClient;
            _accountId = SetUp(nestedEndToEndTest._httpClient).GetAwaiter().GetResult();
        }

        private async Task<string> SetUp(HttpClient httpClient)
        {
            var newAccount = new JsonObject
            {
                ["name"] = "Test Account"    
            };

            var response = await httpClient.PostAsJsonAsync("/accounts", newAccount);
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadFromJsonAsync<JsonDocument>();
            responseBody.Should().NotBeNull();
        
            var name = responseBody!.RootElement.GetProperty("name").GetString();
            name.Should().Be("Test Account");
        
            var id = responseBody.RootElement.GetProperty("id").GetString();
            id.Should().NotBeNull();
            return id!;
        }

        [Fact]
        public async Task Should_Be_Able_To_Get_Account()
        {
            var response = await _httpClient.GetAsync($"/accounts/{_accountId}");
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadFromJsonAsync<JsonDocument>();
            responseBody.Should().NotBeNull();
        
            var name = responseBody!.RootElement.GetProperty("name").GetString();
            name.Should().Be("Test Account");
        }

        [Fact]
        public async Task Should_Be_Able_To_Delete_Account()
        {
            var deleteResponse = await _httpClient.DeleteAsync($"/accounts/{_accountId}");
            deleteResponse.EnsureSuccessStatusCode();

            var getResponse = await _httpClient.GetAsync($"/accounts/{_accountId}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}