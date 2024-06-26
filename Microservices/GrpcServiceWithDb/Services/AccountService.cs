using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcService;
using GrpcServiceWithDb.Persistence;
using Microsoft.EntityFrameworkCore;
using Empty = GrpcService.Empty;

namespace GrpcServiceWithDb.Services;

public class AccountService(ILogger<AccountService> logger, AccountsDbContext dbContext) : AccountsService.AccountsServiceBase
{
    
    public override async Task<Account> Create(CreateAccount request, ServerCallContext context)
    {
        logger.LogInformation("Handling SayHello request for {Name}", request.Name);
        
        var entityEntry = dbContext.Accounts.Add(new MyEntity { Name = request.Name });
        await dbContext.SaveChangesAsync();
        
        logger.LogInformation("Completed async operation for {Name}", request.Name);

        return new Account
        {
            Id = entityEntry.Entity.Id,
            Name = entityEntry.Entity.Name,
        };
    }

    public override Task<NullableAccount> Get(AccountId request, ServerCallContext context)
    {
        var entity = dbContext.Accounts.SingleOrDefault(entity => entity.Id == request.Id);
        if (entity == null)
        {
            return Task.FromResult( new NullableAccount
            {
                Null = NullValue.NullValue
            });
        }

        var nullableAccount = new NullableAccount
        {
            Account = new Account
            {
                Id = entity.Id,
                Name = entity.Name,
            }
        };
        return Task.FromResult(nullableAccount);
    }

    public override async Task<Account> Update(Account request, ServerCallContext context)
    {
        var entity = dbContext.Accounts.SingleOrDefault(entity => entity.Id == request.Id);
        if (entity == null)
        {
            throw new ArgumentException("Account not found");
        }

        entity.Name = request.Name;
        await dbContext.SaveChangesAsync();
        return new Account
        {
            Id = entity.Id,
            Name = entity.Name,
        };
    }

    public override async Task<Account> Delete(AccountId request, ServerCallContext context)
    {
        var entity = dbContext.Accounts.SingleOrDefault(entity => entity.Id == request.Id);
        if (entity == null)
        {
            throw new ArgumentException("Account not found");
        }

        dbContext.Remove(entity);
        await dbContext.SaveChangesAsync();
        return new Account
        {
            Id = entity.Id,
            Name = entity.Name,
        };
    }

    public override async Task<Accounts> GetAll(Empty request, ServerCallContext context)
    {
        var entities = await dbContext.Accounts
            .Select(e => new Account { Id = e.Id, Name = e.Name })
            .ToListAsync();

        var reply = new Accounts();
        reply.Entities.AddRange(entities);
        return reply;
    }
}