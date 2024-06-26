using FluentAssertions;
using Grpc.Core;
using Grpc.Net.Client;
using GrpcService;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;
using TestUtils;
using Xunit.Abstractions;

namespace GrpcServiceWithDb.Tests;

public class AccountServiceTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly AccountsService.AccountsServiceClient _accountClient;

    public AccountServiceTests(WebApplicationFactory<Program> factory, ITestOutputHelper output)
    {
        var withWebHostBuilder = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureLogging(logging =>
            {
                logging.AddProvider(new XUnitLoggerProvider(output));
            });
        });

        var client = withWebHostBuilder.CreateDefaultClient();
        var channel = GrpcChannel.ForAddress(client.BaseAddress!, new GrpcChannelOptions { HttpClient = client });
        _accountClient = new AccountsService.AccountsServiceClient(channel);
    }
    
    [Fact]
    public async Task Should_Return_Null_Getting_With_Unknown_Id()
    {
        var nullableAccount = await _accountClient.GetAsync(new AccountId { Id = "unknown" });
        nullableAccount.Account.Should().BeNull();
    }
    
    [Fact]
    public async Task Should_Throw_InvalidArgument_When_Updating_Unknown()
    {
        var e = await Assert.ThrowsAsync<RpcException>(async () => await _accountClient.UpdateAsync(new Account { Id = "unknown", Name = "New Name"}));
        e.StatusCode.Should().Be(StatusCode.InvalidArgument);
        e.Status.Detail.Should().Be("Account not found");
    }
    
    [Fact]
    public async Task Should_Throw_InvalidArgument_When_Deleting_Unknown()
    {
        var e = await Assert.ThrowsAsync<RpcException>(async () => await _accountClient.DeleteAsync(new AccountId { Id = "unknown"}));
        e.StatusCode.Should().Be(StatusCode.InvalidArgument);
        e.Status.Detail.Should().Be("Account not found");
    }
    
    public class WithAccount: IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly string _name = $"Account-{Guid.NewGuid().ToString()}";
        private readonly AccountsService.AccountsServiceClient _accountClient;
        private readonly string _accountId;

        public WithAccount(WebApplicationFactory<Program> factory, ITestOutputHelper output)
        {
            var accountServiceTests = new AccountServiceTests(factory, output);
            _accountClient = accountServiceTests._accountClient;
            _accountId = SetUp().GetAwaiter().GetResult();
        }

        private async Task<string> SetUp()
        {
            var response = await _accountClient.CreateAsync(new CreateAccount { Name = _name });
            return response.Id;
        }

        [Fact]
        public async Task Should_Be_Able_To_Get_Account()
        {
            var nullableAccount = await _accountClient.GetAsync(new AccountId { Id = _accountId });
            nullableAccount.Account.Should().NotBeNull();
            nullableAccount.Account.Id.Should().Be(_accountId);
            nullableAccount.Account.Name.Should().Be(_name);
        }

        [Fact]
        public async Task Should_Be_Able_To_Get_All_Accounts()
        {
            var accounts = await _accountClient.GetAllAsync(new Empty());
            accounts.Should().NotBeNull();
            accounts.Entities.Should().ContainSingle(account => account.Id == _accountId);
        }

        [Fact]
        public async Task Should_Be_Able_To_Delete_Account()
        {
            await _accountClient.DeleteAsync(new AccountId { Id = _accountId });
            var nullableAccount = await _accountClient.GetAsync(new AccountId { Id = _accountId });
            nullableAccount.Account.Should().BeNull();
        }

        [Fact]
        public async Task Should_Be_Able_To_Update_Account_Name()
        {
            await _accountClient.UpdateAsync(new Account {Id = _accountId, Name = "New Name"});
            var nullableAccount = await _accountClient.GetAsync(new AccountId { Id = _accountId });
            nullableAccount.Account.Should().NotBeNull();
            nullableAccount.Account.Id.Should().Be(_accountId);
            nullableAccount.Account.Name.Should().Be("New Name");
        }
    }
}