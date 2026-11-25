using ModularAgenda.Core.Abstractions;
using ModularAgenda.Core.Exceptions;

namespace ModularAgenda.Core.Domain;

public class Note : AgendaItem, ICalendarItem
{
<<<<<<< HEAD
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public Guid? TargetId { get; set; }
    public DateOnly? Date { get; set; }

    public DateOnly? CalendarDate => Date;

    public override string Summary => Title;
=======
    private const int SummaryMaxLength = 30;

    public DateOnly Date { get; private set; }
    public string Text { get; private set; }

    public Note(DateOnly date, string text)
    {
        EnsureNotEmpty(text);
        Date = date;
        Text = text;
    }

    public DateOnly? CalendarDate => Date;

    public override string Summary =>
        Text.Length <= SummaryMaxLength
            ? Text
            : Text[..SummaryMaxLength] + "…";

    public void UpdateText(string text)
    {
        EnsureNotEmpty(text);
        Text = text;
        MarkUpdated();
    }

    private static void EnsureNotEmpty(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new EmptyNoteTextException();
    }
>>>>>>> origin/main
}

