using DomusMind.Domain.Family;

namespace DomusMind.Domain.Areas;

public sealed class Area
{
    public Guid Id { get; private set; }
    public FamilyId FamilyId { get; private set; }
    public string Name { get; private set; }
    public string? Color { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    public Area(
        Guid id,
        FamilyId familyId,
        string name,
        string? color,
        DateTime createdAtUtc)
    {
        Id = id;
        FamilyId = familyId;
        Name = name;
        Color = string.IsNullOrWhiteSpace(color) ? null : color.Trim();
        CreatedAtUtc = createdAtUtc;
    }

#pragma warning disable CS8618
    private Area()
    {
    }
#pragma warning restore CS8618
}
