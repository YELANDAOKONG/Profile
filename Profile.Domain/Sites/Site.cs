using Profile.Domain.Sites.Value;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Sites;

public sealed class Site
{
    public Site(
        SiteMode mode,
        RegistrationPolicy registrationPolicy,
        UserIdentity ownerId,
        UserIdentity? rootAccountSiteOwnerId)
    {
        ArgumentNullException.ThrowIfNull(ownerId);

        ValidateMode(mode, nameof(mode));
        ValidateRegistrationPolicy(registrationPolicy, nameof(registrationPolicy));

        Mode = mode;
        RegistrationPolicy = registrationPolicy;
        OwnerId = ownerId;
        RootAccountSiteOwnerId = rootAccountSiteOwnerId;
    }

    public SiteMode Mode { get; private set; }

    public RegistrationPolicy RegistrationPolicy { get; private set; }

    // Identifies the account that owns this deployment. Ownership controls
    // site-level authority and is independent of which account site is shown
    // at the deployment root.
    public UserIdentity OwnerId { get; }

    // Selects the account site exposed at the deployment root. This mapping
    // grants no ownership or administrative authority and may be left unset.
    public UserIdentity? RootAccountSiteOwnerId { get; private set; }

    public void ChangeMode(SiteMode mode)
    {
        ValidateMode(mode, nameof(mode));
        Mode = mode;
    }

    public void ChangeRegistrationPolicy(RegistrationPolicy registrationPolicy)
    {
        ValidateRegistrationPolicy(registrationPolicy, nameof(registrationPolicy));
        RegistrationPolicy = registrationPolicy;
    }

    public void SetRootAccountSiteOwner(UserIdentity? ownerId)
    {
        RootAccountSiteOwnerId = ownerId;
    }

    private static void ValidateMode(SiteMode mode, string parameterName)
    {
        if (!Enum.IsDefined(mode))
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                mode,
                "Site mode is not supported.");
        }
    }

    private static void ValidateRegistrationPolicy(
        RegistrationPolicy registrationPolicy,
        string parameterName)
    {
        if (!Enum.IsDefined(registrationPolicy))
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                registrationPolicy,
                "Registration policy is not supported.");
        }
    }
}
