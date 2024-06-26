using Microsoft.EntityFrameworkCore;

namespace GrpcServiceWithDb.Persistence;

public class AccountsDbContext : DbContext
{
    public DbSet<MyEntity> Accounts { get; set; }

    public AccountsDbContext(DbContextOptions<AccountsDbContext> options)
        : base(options)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MyEntity>()
            .Property(e => e.Id)
            .HasDefaultValueSql("gen_random_uuid()");
    }
}

public class MyEntity
{
    public string Id { get; set; }
    public string Name { get; set; }
}