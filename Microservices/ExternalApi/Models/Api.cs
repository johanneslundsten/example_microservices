namespace ExternalApi.Models;

public static class Api
{
    public static Account ToApi(this GrpcService.Account account)
    {
        return new Account
        {
            Id = account.Id,
            Name = account.Name,
        };
    }
}
public class Account
{
    public string Id { get; set; }
    public string Name { get; set; }

    public GrpcService.Account ToGrpc()
    {
        return new GrpcService.Account
        {
            Id = Id,
            Name = Name
        };
    }
}

public class CreateAccount
{
    public string Name { get; set; }
    public GrpcService.CreateAccount ToGrpc()
    {
        return new GrpcService.CreateAccount
        {
            Name = Name
        };
    }
}


