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
        return await _context.Set<T>().FindAsync(id);
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

    public async Task DeleteAsync(Guid id)
    {
        var item = await _context.Set<T>().IgnoreQueryFilters().FirstOrDefaultAsync(e => e.Id == id);
        if (item != null)
        {
            item.Delete();
            _context.Set<T>().Update(item);
            await _context.SaveChangesAsync();
        }
    }
}
