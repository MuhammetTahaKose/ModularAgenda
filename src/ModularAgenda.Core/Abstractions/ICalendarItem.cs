namespace ModularAgenda.Core.Abstractions;

public interface ICalendarItem
{
    DateOnly? CalendarDate { get; }   // null ise takvimde görünmez
    string Summary { get; }
}
