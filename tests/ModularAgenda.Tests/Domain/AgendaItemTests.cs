namespace ModularAgenda.Tests.Domain;

public class AgendaItemTests
{
    // DateTime.UtcNow'un iki çağrı arasında kesin ilerlemesi için kısa bekleme.
    private static void WaitForClockToAdvance() => Thread.Sleep(15);

    [Fact]
    public void Constructor_WhenCreated_IdIsNotEmpty()
    {
        // Arrange & Act
        var item = new TestAgendaItem();

        // Assert
        Assert.NotEqual(Guid.Empty, item.Id);
    }

    [Fact]
    public void Constructor_WhenCreated_CreatedAndUpdatedAreEqualAndUtc()
    {
        // Arrange & Act
        var item = new TestAgendaItem();

        // Assert
        Assert.Equal(item.CreatedAtUtc, item.UpdatedAtUtc);
        Assert.Equal(DateTimeKind.Utc, item.CreatedAtUtc.Kind);
        Assert.Equal(DateTimeKind.Utc, item.UpdatedAtUtc.Kind);
    }

    [Fact]
    public void Delete_WhenCalled_SetsIsDeletedAndAdvancesUpdatedAt()
    {
        // Arrange
        var item = new TestAgendaItem();
        var updatedBefore = item.UpdatedAtUtc;
        WaitForClockToAdvance();

        // Act
        item.Delete();

        // Assert
        Assert.True(item.IsDeleted);
        Assert.True(item.UpdatedAtUtc > updatedBefore);
    }

    [Fact]
    public void Delete_WhenCalledTwice_DoesNotChangeUpdatedAt()
    {
        // Arrange
        var item = new TestAgendaItem();
        item.Delete();
        var updatedAfterFirstDelete = item.UpdatedAtUtc;
        WaitForClockToAdvance();

        // Act
        item.Delete();

        // Assert
        Assert.True(item.IsDeleted);
        Assert.Equal(updatedAfterFirstDelete, item.UpdatedAtUtc);
    }
}
