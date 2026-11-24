using ModularAgenda.Core.Domain;

namespace ModularAgenda.Core.Services.Reminders;

/// <summary>Zamanı gelen bir hatırlatıcı için ekrana giden bilgi.</summary>
/// <param name="Reminder">Çalan hatırlatıcı.</param>
/// <param name="Target">Bağlı olduğu kayıt: görev, etkinlik ya da alışkanlık.</param>
/// <param name="Message">Ekranda gösterilecek birleşik metin, örneğin "21:00, Kitap oku".</param>
/// <param name="TriggeredAtUtc">Hatırlatıcının çaldığı an (UTC).</param>
public record ReminderAlert(Reminder Reminder, AgendaItem Target, string Message, DateTime TriggeredAtUtc);
