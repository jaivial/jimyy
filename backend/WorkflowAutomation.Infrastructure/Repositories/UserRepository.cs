using Microsoft.EntityFrameworkCore;
using WorkflowAutomation.Core.Entities;
using WorkflowAutomation.Core.Enums;
using WorkflowAutomation.Core.Interfaces;
using WorkflowAutomation.Infrastructure.Data;

namespace WorkflowAutomation.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for User entity.
/// </summary>
public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower(), cancellationToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error retrieving user with email '{email}'", ex);
        }
    }

    public async Task<IEnumerable<User>> GetByRoleAsync(
        UserRole role,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet
                .Where(u => u.Role == role)
                .OrderBy(u => u.Email)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error retrieving users with role {role}", ex);
        }
    }

    public async Task<IEnumerable<User>> GetActiveUsersAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet
                .Where(u => u.IsActive)
                .OrderBy(u => u.Email)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error retrieving active users", ex);
        }
    }

    public async Task<User?> GetWithDetailsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet
                .Include(u => u.Workflows)
                .Include(u => u.Credentials)
                .Include(u => u.AIConfigurations)
                .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error retrieving user details for ID {id}", ex);
        }
    }

    public async Task<bool> EmailExistsAsync(
        string email,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _dbSet.Where(u => u.Email.ToLower() == email.ToLower());

            if (excludeId.HasValue)
            {
                query = query.Where(u => u.Id != excludeId.Value);
            }

            return await query.AnyAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error checking if email '{email}' exists", ex);
        }
    }

    public async Task<(IEnumerable<User> Items, int TotalCount)> GetFilteredPagedAsync(
        UserRole? role = null,
        bool? activeOnly = null,
        string? searchTerm = null,
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var query = _dbSet.AsQueryable();

            // Apply filters
            if (role.HasValue)
            {
                query = query.Where(u => u.Role == role.Value);
            }

            if (activeOnly.HasValue)
            {
                query = query.Where(u => u.IsActive == activeOnly.Value);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.ToLower();
                query = query.Where(u =>
                    u.Email.ToLower().Contains(term) ||
                    u.FirstName.ToLower().Contains(term) ||
                    u.LastName.ToLower().Contains(term));
            }

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderBy(u => u.Email)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error retrieving filtered paged users", ex);
        }
    }

    public async Task ActivateAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found");
            }

            user.IsActive = true;
            user.UpdatedAt = DateTime.UtcNow;
            await UpdateAsync(user, cancellationToken);
        }
        catch (Exception ex) when (ex is not KeyNotFoundException)
        {
            throw new InvalidOperationException($"Error activating user {userId}", ex);
        }
    }

    public async Task DeactivateAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found");
            }

            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;
            await UpdateAsync(user, cancellationToken);
        }
        catch (Exception ex) when (ex is not KeyNotFoundException)
        {
            throw new InvalidOperationException($"Error deactivating user {userId}", ex);
        }
    }

    public async Task UpdatePasswordAsync(
        Guid userId,
        string newPasswordHash,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found");
            }

            user.PasswordHash = newPasswordHash;
            user.UpdatedAt = DateTime.UtcNow;
            await UpdateAsync(user, cancellationToken);
        }
        catch (Exception ex) when (ex is not KeyNotFoundException)
        {
            throw new InvalidOperationException($"Error updating password for user {userId}", ex);
        }
    }
}
