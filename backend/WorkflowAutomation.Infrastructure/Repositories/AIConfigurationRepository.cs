using Microsoft.EntityFrameworkCore;
using WorkflowAutomation.Core.Entities;
using WorkflowAutomation.Core.Interfaces;
using WorkflowAutomation.Infrastructure.Data;

namespace WorkflowAutomation.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for AIConfiguration entity.
/// </summary>
public class AIConfigurationRepository : Repository<AIConfiguration>, IAIConfigurationRepository
{
    public AIConfigurationRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<AIConfiguration>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.IsDefault)
                .ThenBy(c => c.DisplayName)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error retrieving AI configurations for user {userId}", ex);
        }
    }

    public async Task<IEnumerable<AIConfiguration>> GetActiveByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet
                .Where(c => c.UserId == userId && c.IsActive)
                .OrderByDescending(c => c.IsDefault)
                .ThenBy(c => c.DisplayName)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error retrieving active AI configurations for user {userId}", ex);
        }
    }

    public async Task<AIConfiguration?> GetDefaultByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet
                .FirstOrDefaultAsync(c => c.UserId == userId && c.IsDefault && c.IsActive, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error retrieving default AI configuration for user {userId}", ex);
        }
    }

    public async Task<IEnumerable<AIConfiguration>> GetByProviderAsync(
        string provider,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet
                .Where(c => c.Provider == provider)
                .OrderBy(c => c.DisplayName)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error retrieving AI configurations for provider '{provider}'", ex);
        }
    }

    public async Task SetAsDefaultAsync(
        Guid configurationId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // First, unset any existing default for the user
            var existingDefaults = await _dbSet
                .Where(c => c.UserId == userId && c.IsDefault)
                .ToListAsync(cancellationToken);

            foreach (var config in existingDefaults)
            {
                config.IsDefault = false;
                config.UpdatedAt = DateTime.UtcNow;
            }

            // Set the new default
            var newDefault = await GetByIdAsync(configurationId, cancellationToken);
            if (newDefault == null)
            {
                throw new KeyNotFoundException($"AI configuration with ID {configurationId} not found");
            }

            if (newDefault.UserId != userId)
            {
                throw new InvalidOperationException("Cannot set AI configuration as default for a different user");
            }

            newDefault.IsDefault = true;
            newDefault.IsActive = true;
            newDefault.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex) when (ex is not KeyNotFoundException && ex is not InvalidOperationException)
        {
            throw new InvalidOperationException($"Error setting AI configuration {configurationId} as default", ex);
        }
    }

    public async Task ActivateAsync(Guid configurationId, CancellationToken cancellationToken = default)
    {
        try
        {
            var configuration = await GetByIdAsync(configurationId, cancellationToken);
            if (configuration == null)
            {
                throw new KeyNotFoundException($"AI configuration with ID {configurationId} not found");
            }

            configuration.IsActive = true;
            configuration.UpdatedAt = DateTime.UtcNow;
            await UpdateAsync(configuration, cancellationToken);
        }
        catch (Exception ex) when (ex is not KeyNotFoundException)
        {
            throw new InvalidOperationException($"Error activating AI configuration {configurationId}", ex);
        }
    }

    public async Task DeactivateAsync(Guid configurationId, CancellationToken cancellationToken = default)
    {
        try
        {
            var configuration = await GetByIdAsync(configurationId, cancellationToken);
            if (configuration == null)
            {
                throw new KeyNotFoundException($"AI configuration with ID {configurationId} not found");
            }

            configuration.IsActive = false;
            configuration.IsDefault = false; // If deactivated, it shouldn't be default
            configuration.UpdatedAt = DateTime.UtcNow;
            await UpdateAsync(configuration, cancellationToken);
        }
        catch (Exception ex) when (ex is not KeyNotFoundException)
        {
            throw new InvalidOperationException($"Error deactivating AI configuration {configurationId}", ex);
        }
    }

    public async Task<IEnumerable<string>> GetProvidersAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet
                .Select(c => c.Provider)
                .Distinct()
                .OrderBy(p => p)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error retrieving AI providers", ex);
        }
    }

    public async Task<bool> DisplayNameExistsAsync(
        string displayName,
        Guid userId,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _dbSet.Where(c =>
                c.UserId == userId &&
                c.DisplayName.ToLower() == displayName.ToLower());

            if (excludeId.HasValue)
            {
                query = query.Where(c => c.Id != excludeId.Value);
            }

            return await query.AnyAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error checking if display name '{displayName}' exists", ex);
        }
    }
}
