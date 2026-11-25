using Microsoft.EntityFrameworkCore;
using ModularAgenda.Core.Domain;

namespace ModularAgenda.Data;

public class AppDbContext : DbContext
{
    public AppDbContext() { }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Note> Notes => Set<Note>();
    public DbSet<TodoItem> TodoItems => Set<TodoItem>();
    public DbSet<CalendarEvent> CalendarEvents => Set<CalendarEvent>();
    public DbSet<Habit> Habits => Set<Habit>();
    public DbSet<Reminder> Reminders => Set<Reminder>();
    public DbSet<OneTimeReminder> OneTimeReminders => Set<OneTimeReminder>();
    public DbSet<DailyReminder> DailyReminders => Set<DailyReminder>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite("Data Source=agenda.db");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Global Query Filter: Soft Delete
        modelBuilder.Entity<Note>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<TodoItem>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<CalendarEvent>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Habit>().HasQueryFilter(e => !e.IsDeleted);
        // OneTimeReminder ve DailyReminder aynı tabloda (TPH); EF Core filtreyi sadece kök tipe kabul eder.
        modelBuilder.Entity<Reminder>().HasQueryFilter(e => !e.IsDeleted);
    }
}

