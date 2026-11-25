using Microsoft.EntityFrameworkCore;
using ModularAgenda.Core.Domain;
using ModularAgenda.Core.Repositories;

namespace ModularAgenda.Data.Repositories;

public class EfReminderRepository : EfRepository<Reminder>, IReminderRepository
{
    public EfReminderRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Reminder>> GetByTargetAsync(Guid targetId)
    {
        // Assuming Reminder has a TargetId or similar. Let's check Reminder.cs
        return await _context.Set<Reminder>()
            .Where(r => r.TargetId == targetId)
            .ToListAsync();
    }
}
