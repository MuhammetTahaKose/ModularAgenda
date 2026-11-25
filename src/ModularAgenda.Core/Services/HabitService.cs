using ModularAgenda.Core.Domain;
using ModularAgenda.Core.Repositories;

namespace ModularAgenda.Core.Services;

/// <summary>Alışkanlık (Zincir) sayfasının servisi.</summary>
public class HabitService : CrudService<Habit>
{
    private readonly LinkedItemsCleaner _linkedItems;

    public HabitService(IRepository<Habit> habits, LinkedItemsCleaner linkedItems) : base(habits)
    {
        ArgumentNullException.ThrowIfNull(linkedItems);
        _linkedItems = linkedItems;
    }

    /// <summary>
    /// Alışkanlığı siler; ona bağlı hatırlatıcıları da siler. Alışkanlığa not bağlanamadığı
    /// için (Habit INoteable değil) not silme çağrısı burada derlenmez bile.
    /// </summary>
    public override async Task DeleteAsync(Habit habit)
    {
        await base.DeleteAsync(habit);
        await _linkedItems.DeleteRemindersOfAsync(habit);
    }

    // Habit dolunca eklenecek: günü işaretleme, işareti kaldırma ve seri bilgisi (GetChainsAsync).
}
