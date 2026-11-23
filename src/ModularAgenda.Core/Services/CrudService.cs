using ModularAgenda.Core.Domain;
using ModularAgenda.Core.Repositories;

namespace ModularAgenda.Core.Services;

/// <summary>
/// Her kayıt türü için ortak ekle / güncelle / sil / listele işlemleri. Görev, etkinlik,
/// not, alışkanlık ve hatırlatıcı servisleri bu sınıftan türer ve yalnızca kendilerine
/// özgü işlemleri ekler. Soyut olduğu için doğrudan nesnesi oluşturulamaz.
/// </summary>
/// <typeparam name="T">AgendaItem'dan türeyen kayıt türü.</typeparam>
public abstract class CrudService<T> where T : AgendaItem
{
    /// <summary>Alt sınıflar veriye bu repository üzerinden ulaşır.</summary>
    protected IRepository<T> Repository { get; }

    protected CrudService(IRepository<T> repository)
    {
        ArgumentNullException.ThrowIfNull(repository);
        Repository = repository;
    }

    public async Task AddAsync(T item)
    {
        ArgumentNullException.ThrowIfNull(item);
        await Repository.AddAsync(item);
    }

    public async Task UpdateAsync(T item)
    {
        ArgumentNullException.ThrowIfNull(item);
        await Repository.UpdateAsync(item);
    }

    /// <summary>Kayıt yoksa ya da silinmişse null döner.</summary>
    public async Task<T?> GetByIdAsync(Guid id) => await Repository.GetByIdAsync(id);

    /// <summary>Silinmemiş tüm kayıtlar.</summary>
    public async Task<IReadOnlyList<T>> GetAllAsync() => await Repository.GetAllAsync();

    /// <summary>
    /// Kaydı siler. Gerçek silme yoktur (Karar K4.3): kayda "silindi" işareti konur ve
    /// kaydedilir. Bağlı notları ya da hatırlatıcıları olan türlerin servisleri bu metodu
    /// ezer (override) ve onları da siler.
    /// </summary>
    public virtual async Task DeleteAsync(T item)
    {
        ArgumentNullException.ThrowIfNull(item);
        item.Delete();
        await Repository.UpdateAsync(item); // İşaret ancak kaydedilince kalıcı olur.
    }
}
