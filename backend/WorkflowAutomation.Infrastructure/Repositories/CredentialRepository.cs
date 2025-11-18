using Microsoft.EntityFrameworkCore;
using WorkflowAutomation.Core.Entities;
using WorkflowAutomation.Core.Interfaces;
using WorkflowAutomation.Infrastructure.Data;

namespace WorkflowAutomation.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Credential entity.
/// Note: Encryption/decryption logic should be handled in a separate service layer.
/// </summary>
public class CredentialRepository : Repository<Credential>, ICredentialRepository
{
    public CredentialRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Credential>> GetByTypeAsync(
        string type,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet
                .Where(c => c.Type == type)
                .OrderBy(c => c.Name)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error retrieving credentials of type '{type}'", ex);
        }
    }

    public async Task<IEnumerable<Credential>> GetByCreatorAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet
                .Where(c => c.CreatedBy == userId)
                .OrderBy(c => c.Name)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error retrieving credentials for user {userId}", ex);
        }
    }

    public async Task<Credential?> GetByNameAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet
                .FirstOrDefaultAsync(c => c.Name == name, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error retrieving credential with name '{name}'", ex);
        }
    }

    public async Task<bool> NameExistsAsync(
        string name,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _dbSet.Where(c => c.Name.ToLower() == name.ToLower());

            if (excludeId.HasValue)
            {
                query = query.Where(c => c.Id != excludeId.Value);
            }

            return await query.AnyAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error checking if credential name '{name}' exists", ex);
        }
    }

    public async Task<(IEnumerable<Credential> Items, int TotalCount)> GetFilteredPagedAsync(
        string? type = null,
        Guid? userId = null,
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
            if (!string.IsNullOrWhiteSpace(type))
            {
                query = query.Where(c => c.Type == type);
            }

            if (userId.HasValue)
            {
                query = query.Where(c => c.CreatedBy == userId.Value);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.ToLower();
                query = query.Where(c => c.Name.ToLower().Contains(term));
            }

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderBy(c => c.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error retrieving filtered paged credentials", ex);
        }
    }

    public async Task<IEnumerable<string>> GetCredentialTypesAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet
                .Select(c => c.Type)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error retrieving credential types", ex);
        }
    }
}
