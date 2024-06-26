using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using GrpcService;
using NSubstitute;
using Microsoft.Extensions.DependencyInjection;
using TestUtils;
using Account = GrpcService.Account;
using CreateAccount = ExternalApi.Models.CreateAccount;

namespace ExternalApi.Tests
{
    public class AccountsApiTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private AccountsService.AccountsServiceClient _grpcClientMock;
        private readonly HttpClient _client;

        public AccountsApiTests(WebApplicationFactory<Program> factory)
        {
            _grpcClientMock = Substitute.For<AccountsService.AccountsServiceClient>();
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddSingleton(_grpcClientMock);
                });
            });
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task GetAccounts_ReturnsAccounts()
        {
            _grpcClientMock.GetAll(Arg.Any<Empty>()).Returns(new Accounts
            {
                Entities = { new Account { Id = "test-id", Name = "Test Account" } }
            });

            var response = await _client.GetAsync("/accounts");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var accounts = await response.Content.ReadFromJsonAsync<List<Account>>();
            accounts.Should().NotBeNull();
            accounts.Should().BeOfType<List<Account>>();
            accounts.First().Id.Should().Be("test-id");
            accounts.First().Name.Should().Be("Test Account");
        }

        [Fact]
        public async Task GetAccountById_ReturnsAccount()
        {
            _grpcClientMock.Get(Arg.Any<AccountId>()).Returns(new NullableAccount()
            {
                Account = new Account
                {
                    Id = "test-id",
                    Name = "Test Account"
                }
            });
            var accountId = "test-id";

            var response = await _client.GetAsync($"/accounts/{accountId}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var account = await response.Content.ReadFromJsonAsync<Account>();
            account.Should().NotBeNull();
            account.Id.Should().Be(accountId);
            account.Name.Should().Be("Test Account");
        }

        [Fact]
        public async Task CreateAccount_ReturnsCreatedAccount()
        {
            var newAccount = new CreateAccount { Name = "New Account" };
            var returnThis = new Account
            {
                Id = "test-id",
                Name = "New Account"
            };
            
            var asyncUnaryCall = GrpcTestHelper.CreateAsyncUnaryCall(returnThis);
            _grpcClientMock.CreateAsync(Arg.Any<GrpcService.CreateAccount>()).Returns(asyncUnaryCall);
            

            var response = await _client.PostAsJsonAsync("/accounts", newAccount);

            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var createdAccount = await response.Content.ReadFromJsonAsync<Account>();
            createdAccount.Should().NotBeNull();
            createdAccount.Id.Should().Be("test-id");
            createdAccount.Name.Should().Be(newAccount.Name);
        }

        [Fact]
        public async Task UpdateAccount_ReturnsNoContent()
        {
            var accountId = "test-id";
            var updatedAccount = new Account { Id = accountId, Name = "Updated Account" };
            
            var asyncUnaryCall = GrpcTestHelper.CreateAsyncUnaryCall(updatedAccount);
            _grpcClientMock.UpdateAsync(Arg.Any<GrpcService.Account>()).Returns(asyncUnaryCall);
            
            var response = await _client.PutAsJsonAsync($"/accounts/{accountId}", updatedAccount);

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task DeleteAccount_ReturnsOk()
        {
            var returnThis = new Account
            {
                Id = "test-id",
                Name = "New Account"
            };
            
            var asyncUnaryCall = GrpcTestHelper.CreateAsyncUnaryCall(returnThis);
            _grpcClientMock.DeleteAsync(Arg.Any<GrpcService.AccountId>()).Returns(asyncUnaryCall);
            var accountId = "test-id";

            var response = await _client.DeleteAsync($"/accounts/{accountId}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var account = await response.Content.ReadFromJsonAsync<Account>();
            account.Should().NotBeNull();
            account.Id.Should().Be(accountId);
            account.Name.Should().Be("New Account");
        }
    }
}
