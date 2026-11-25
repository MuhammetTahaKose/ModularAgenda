using ModularAgenda.Core.Domain;
using ModularAgenda.Core.Repositories;
using ModularAgenda.Core.Services.Strategies;

namespace ModularAgenda.Core.Services;

/// <summary>To-do sayfasının kullandığı servis.</summary>
public class TodoService : CrudService<TodoItem>
{
    private readonly LinkedItemsCleaner _linkedItems;

    public TodoService(ITodoRepository todos, LinkedItemsCleaner linkedItems) : base(todos)
    {
        ArgumentNullException.ThrowIfNull(linkedItems);
        _linkedItems = linkedItems;
    }

    /// <summary>Görevi siler; ona bağlı notları ve hatırlatıcıları da siler (K2.6).</summary>
    public override async Task DeleteAsync(TodoItem todo)
    {
        await base.DeleteAsync(todo);
        await _linkedItems.DeleteNotesOfAsync(todo);
        await _linkedItems.DeleteRemindersOfAsync(todo);
    }

    /// <summary>
    /// Strategy deseninin kullanıldığı yer. Hangi filtre ve sıralamanın kullanılacağına
    /// ekran karar verir; servis yalnızca verilen stratejileri uygular. Yeni bir sıralama
    /// eklemek için bu metodu değiştirmek gerekmez, yeni bir strateji sınıfı yazmak yeter.
    /// </summary>
    public async Task<IReadOnlyList<TodoItem>> GetTodosAsync(ITodoFilterStrategy filter, ITodoSortStrategy sort)
    {
        ArgumentNullException.ThrowIfNull(filter);
        ArgumentNullException.ThrowIfNull(sort);

        IReadOnlyList<TodoItem> all = await Repository.GetAllAsync();
        IEnumerable<TodoItem> filtered = all.Where(todo => filter.Matches(todo));
        return sort.Sort(filtered).ToList();
    }

    // TodoItem dolunca eklenecek: CompleteAsync(todo). "Tamamlanmış görev tekrar
    // tamamlanamaz" kuralı TodoItem'ın içinde olacak; servis sadece çağırıp kaydedecek.
}
