using ModularAgenda.Core.Abstractions;

namespace ModularAgenda.Core.Services.Calendar;

/// <summary>Aylık takvim ızgarasındaki tek bir gün kutusu.</summary>
/// <param name="Date">Kutunun günü.</param>
/// <param name="IsInCurrentMonth">Gösterilen aya mı ait, yoksa önceki/sonraki aydan dolgu mu?</param>
/// <param name="IsToday">Bugün mü? (Ekranda vurgulamak için.)</param>
/// <param name="Items">
/// O güne düşen etkinlikler, son tarihli görevler ve notlar. Kutu bunların türünü bilmeden
/// her birinin Summary'sini çizer (çok biçimlilik).
/// </param>
public record CalendarDay(DateOnly Date, bool IsInCurrentMonth, bool IsToday, IReadOnlyList<ICalendarItem> Items)
{
    public bool HasItems => Items.Count > 0;
}
