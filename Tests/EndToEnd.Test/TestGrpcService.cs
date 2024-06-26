using Grpc.Net.Client;
using GrpcService;

namespace EndToEnd.Test;

public class GrpcServiceWithDbTest
{
    [Fact]
    public void TestGettingEntities()
    {
        using var channel = GrpcChannel.ForAddress("http://localhost:5001");
        var client = new AccountsService.AccountsServiceClient(channel);
        var reply = client.Create(new CreateAccount { Name = "Iggy" });
        Assert.NotNull(reply.Id);
        
        var entitiesReply = client.GetAll(new Empty());
        var entities = entitiesReply.Entities.Where(entity => entity.Id == reply.Id).ToList();
        Assert.Single(entities);
        Assert.Equal("Iggy", entities.Single().Name);
    }
}