using ModularAgenda.Core.Domain;

namespace ModularAgenda.Core.Services.Strategies;

/// <summary>
/// Strategy deseni: görev listesini sıralamanın bir yolu. Her sıralama ayrı bir sınıftır
/// (önceliğe, son tarihe, duruma göre); ekran hangisini kullanacağını seçer ve
/// TodoService.GetTodosAsync'e verir. Somut sıralamalar TodoItem dolunca eklenecek.
/// </summary>
public interface ITodoSortStrategy
{
    /// <summary>Ekrandaki seçim kutusunda (ComboBox) görünecek ad.</summary>
    string DisplayName { get; }

    IEnumerable<TodoItem> Sort(IEnumerable<TodoItem> todos);
}
