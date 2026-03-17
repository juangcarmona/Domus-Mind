using DomusMind.Domain.Abstractions;
using DomusMind.Domain.Family;

namespace DomusMind.Domain.Tasks;

public sealed class RoutineTargetMember : Entity<MemberId>
{
    public MemberId MemberId => Id;

    private RoutineTargetMember(MemberId memberId)
        : base(memberId)
    {
    }

    internal static RoutineTargetMember Create(MemberId memberId)
        => new(memberId);

#pragma warning disable CS8618
    private RoutineTargetMember() : base(default) { }
#pragma warning restore CS8618
}