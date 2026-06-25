using PruebaNetCoreProject.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace PruebaNetCoreProject.Infrastructure.Persistence.Contexts;

public sealed class ApplicationDbContext : DbContext, IUnitOfWork
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        base.OnModelCreating(modelBuilder);
    }
}
