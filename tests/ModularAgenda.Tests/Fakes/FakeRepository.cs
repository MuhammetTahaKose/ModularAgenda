using ModularAgenda.Core.Domain;
using ModularAgenda.Core.Repositories;

namespace ModularAgenda.Tests.Fakes;

/// <summary>
/// Testler için bellekte çalışan repository. Gerçeği gibi silinmiş kayıtları hiç döndürmez
/// (Karar K6.1). Ayrıca hangi kayıtların UpdateAsync ile kaydedildiğini not eder: bellekte
/// nesnenin kendisi tutulduğu için servis kaydetmeyi unutsa bile değişiklik görünürdü,
/// gerçek veritabanında ise kaybolurdu. UpdatedIds bu hatayı yakalar.
/// </summary>
public class FakeRepository<T> : IRepository<T> where T : AgendaItem
{
    protected Dictionary<Guid, T> Items { get; } = new();

    /// <summary>UpdateAsync ile kaydedilen kayıtların Id'leri, çağrı sırasıyla.</summary>
    public List<Guid> UpdatedIds { get; } = new();

    public Task AddAsync(T item)
    {
        Items.Add(item.Id, item);
        return Task.CompletedTask;
    }

    public Task<T?> GetByIdAsync(Guid id) =>
        Task.FromResult<T?>(Items.TryGetValue(id, out var item) && !item.IsDeleted ? item : null);

    public Task<IReadOnlyList<T>> GetAllAsync() =>
        Task.FromResult<IReadOnlyList<T>>(Active().ToList());

    public Task UpdateAsync(T item)
    {
        if (!Items.ContainsKey(item.Id))
            throw new InvalidOperationException("Güncellenecek kayıt hiç eklenmemiş.");
        UpdatedIds.Add(item.Id);
        return Task.CompletedTask;
    }

    /// <summary>Silinmemiş kayıtlar.</summary>
    protected IEnumerable<T> Active() => Items.Values.Where(item => !item.IsDeleted);
}
