using Microsoft.EntityFrameworkCore;
using ModularAgenda.Core.Domain;
using ModularAgenda.Core.Repositories;

namespace ModularAgenda.Data.Repositories;

public class EfRepository<T> : IRepository<T> where T : AgendaItem
{
    protected readonly AppDbContext _context;

    public EfRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(T item)
    {
        await _context.Set<T>().AddAsync(item);
        await _context.SaveChangesAsync();
    }

    public async Task<T?> GetByIdAsync(Guid id)
    {
        // FindAsync context'te takip edilen silinmiş kaydı da döndürür; sorgu ise silinmiş kayıt filtresini her zaman uygular.
        return await _context.Set<T>().FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<IReadOnlyList<T>> GetAllAsync()
    {
        return await _context.Set<T>().ToListAsync();
    }

    public async Task UpdateAsync(T item)
    {
        // Update updated_at logic here if AgendaItem supports it
        _context.Set<T>().Update(item);
        await _context.SaveChangesAsync();
    }
}
