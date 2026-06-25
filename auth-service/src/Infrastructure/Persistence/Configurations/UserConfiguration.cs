using AuthService.Domain.Entities;
using AuthService.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthService.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.ToTable("users");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).HasColumnName("id").ValueGeneratedNever();

        builder.Property(u => u.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(u => u.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(u => u.UpdatedAt).HasColumnName("updated_at");
        builder.Property(u => u.LastLoginAt).HasColumnName("last_login_at");

        builder.OwnsOne(u => u.Email, email =>
        {
            email.Property(e => e.Value)
                 .HasColumnName("email")
                 .HasMaxLength(254)
                 .IsRequired();
            email.HasIndex(e => e.Value).IsUnique();
        });

        builder.OwnsOne(u => u.FullName, name =>
        {
            name.Property(n => n.FirstName).HasColumnName("first_name").HasMaxLength(100).IsRequired();
            name.Property(n => n.LastName).HasColumnName("last_name").HasMaxLength(100).IsRequired();
        });

        builder.OwnsOne(u => u.PasswordHash, ph =>
        {
            ph.Property(p => p.Value).HasColumnName("password_hash").HasMaxLength(256).IsRequired();
        });
    }
}
