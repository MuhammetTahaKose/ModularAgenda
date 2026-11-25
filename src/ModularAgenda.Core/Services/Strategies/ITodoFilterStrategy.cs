using ModularAgenda.Core.Domain;

namespace ModularAgenda.Core.Services.Strategies;

/// <summary>
/// Strategy deseni: görev listesini süzmenin bir yolu (tümü, bekleyenler, tamamlananlar,
/// öncelik, gecikmiş). Ekran hangisini kullanacağını seçer ve TodoService.GetTodosAsync'e
/// verir. Somut filtreler TodoItem dolunca eklenecek.
/// </summary>
public interface ITodoFilterStrategy
{
    /// <summary>Ekrandaki seçim kutusunda (ComboBox) görünecek ad.</summary>
    string DisplayName { get; }

    /// <summary>Görev listede gösterilsin mi?</summary>
    bool Matches(TodoItem todo);
}
