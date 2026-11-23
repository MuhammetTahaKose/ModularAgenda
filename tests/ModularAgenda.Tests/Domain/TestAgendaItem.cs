using ModularAgenda.Core.Domain;

namespace ModularAgenda.Tests.Domain;

// AgendaItem abstract olduğu için testlerde kullanılan en küçük alt sınıf.
internal sealed class TestAgendaItem : AgendaItem
{
    public override string Summary => "Test kaydı";
}
