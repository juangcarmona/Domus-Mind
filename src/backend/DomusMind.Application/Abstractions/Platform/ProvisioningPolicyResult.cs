namespace DomusMind.Application.Abstractions.Platform;

public sealed record ProvisioningPolicyResult(
    bool Allowed,
    string ReasonCode,
    string Message)
{
    public static ProvisioningPolicyResult Permit() =>
        new(true, "allowed", "Household creation is allowed.");

    public static ProvisioningPolicyResult DenySingleInstanceBound() =>
        new(false, "single_instance_already_bound",
            "This installation already has a household. SingleInstance mode allows exactly one.");

    public static ProvisioningPolicyResult DenyCreationDisabled() =>
        new(false, "household_creation_disabled",
            "Household creation is disabled in this deployment.");

    public static ProvisioningPolicyResult DenyInvitationRequired() =>
        new(false, "invitation_required",
            "An invitation is required to create a household in this deployment.");

    public static ProvisioningPolicyResult DenyMaxHouseholdsReached() =>
        new(false, "max_households_reached",
            "The maximum number of households for this deployment has been reached.");
}
