using ModularAgenda.Core.Domain;

namespace ModularAgenda.Core.Repositories;

/// Silinmiş (IsDeleted = true) kayıtlar hiçbir metotta döndürülmez.
public interface IRepository<T> where T : AgendaItem
{
    Task AddAsync(T item);
    Task<T?> GetByIdAsync(Guid id);   // silinmişse null
    Task<IReadOnlyList<T>> GetAllAsync();
    Task UpdateAsync(T item);
}
