namespace FormBuilder.Application.Common;

/// <summary>Commits the changes tracked in the current request as one transaction.</summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
