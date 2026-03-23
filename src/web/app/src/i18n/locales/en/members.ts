export default {
  pageTitle: "Members",
  addMember: "Add member",
  noMembers: "No members yet.",

  groups: {
    adults: "Adults",
    children: "Children",
    pets: "Pets",
    others: "Others",
  },

  youBadge: "You",
  managerBadge: "Manager",

  access: {
    noAccess: "No access",
    invited: "Invited",
    passwordResetRequired: "Reset required",
    active: "Active",
    disabled: "Disabled",
  },

  detail: {
    back: "Members",
    coreDetails: "Core details",
    access: "Access",
    contacts: "Contacts",
    notes: "Notes",

    name: "Full name",
    preferredName: "Preferred name",
    role: "Role",
    birthDate: "Date of birth",

    status: "Status",
    email: "Account email",
    lastLogin: "Last login",
    neverLoggedIn: "Never signed in",

    primaryPhone: "Primary phone",
    primaryEmail: "Contact email",
    householdNote: "Household note",
    householdNotePlaceholder: "Short note visible to household managers",
  },

  actions: {
    edit: "Edit",
    editProfile: "Edit profile",
    grantAccess: "Grant access",
    resetPassword: "Reset password",
    disableAccess: "Disable access",
    enableAccess: "Enable access",
    copy: "Copy",
    done: "Done",
    saving: "Saving…",
    save: "Save",
    cancel: "Cancel",
  },

  roles: {
    Adult: "Adult",
    Child: "Child",
    Pet: "Pet",
  },

  form: {
    name: "Full name",
    role: "Role",
    birthDate: "Date of birth (optional)",
    isManager: "Manager",
    managerNote: "Manager role is only available for adults.",
    preferredName: "Preferred / display name",
    preferredNamePlaceholder: "Name shown in the app (optional)",
    primaryPhone: "Primary phone",
    primaryEmail: "Contact email",
    householdNote: "Household note",
    householdNotePlaceholder: "Short note visible to managers",
    editTitle: "Edit member",
    editProfileTitle: "Edit profile",
    addTitle: "Add member",
    grantAccessTitle: "Grant access",
    grantAccessSubtitle: "Grant access so this member can sign in. A temporary password will be generated.",
    email: "Email / Username",
    displayName: "Display name (optional)",
    displayNamePlaceholder: "Name shown in the app",
    credentialsSaveWarning: "⚠ This password will not be shown again. Copy it now and share it securely with the member.",
    newTemporaryPassword: "New temporary password",
    credentialsTitle: "Share these credentials",
    temporaryPassword: "Temporary password",
  },

  errors: {
    updateFailed: "Failed to update member.",
    addFailed: "Failed to add member.",
    provisionFailed: "Failed to grant access.",
    regenFailed: "Failed to reset password.",
    disableFailed: "Failed to disable access.",
    enableFailed: "Failed to enable access.",
    notFound: "Member not found.",
  },
} as const;
