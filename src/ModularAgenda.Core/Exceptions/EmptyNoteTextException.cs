namespace ModularAgenda.Core.Exceptions;

public class EmptyNoteTextException : DomainException
{
    public EmptyNoteTextException()
        : base("Not metni boş olamaz.")
    {
    }
}
