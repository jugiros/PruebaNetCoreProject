using AuthService.Application.Ports;
using AuthService.Domain.Entities;
using AuthService.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AuthService.Infrastructure.Persistence.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly AuthDbContext _context;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(AuthDbContext context, ILogger<UserRepository> logger)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(logger);
        _context = context;
        _logger  = logger;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error al obtener usuario por Id");
            throw;
        }
    }

    public async Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(email);
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email.Value == email.Value, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(email);
        return await _context.Users
            .AnyAsync(u => u.Email.Value == email.Value, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);
        await _context.Users.AddAsync(user, cancellationToken).ConfigureAwait(false);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);
        _context.Users.Update(user);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
