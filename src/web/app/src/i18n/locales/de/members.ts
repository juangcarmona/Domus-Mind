export default {
  pageTitle: "Mitglieder",
  addMember: "Mitglied hinzufügen",
  noMembers: "Noch keine Mitglieder.",

  groups: {
    adults: "Erwachsene",
    children: "Kinder",
    pets: "Haustiere",
    others: "Weitere",
  },

  youBadge: "Du",
  managerBadge: "Administrator",

  access: {
    noAccess: "Kein Zugang",
    invited: "Eingeladen",
    passwordResetRequired: "Reset erforderlich",
    active: "Aktiv",
    disabled: "Deaktiviert",
  },

  detail: {
    back: "Mitglieder",
    coreDetails: "Grunddaten",
    access: "Zugang",
    contacts: "Kontakte",
    notes: "Notizen",

    name: "Vollständiger Name",
    preferredName: "Bevorzugter Name",
    role: "Rolle",
    birthDate: "Geburtsdatum",

    status: "Status",
    email: "Konto-E-Mail",
    lastLogin: "Letzte Anmeldung",
    neverLoggedIn: "Noch nie angemeldet",

    primaryPhone: "Haupttelefon",
    primaryEmail: "Kontakt-E-Mail",
    householdNote: "Haushaltsnotiz",
    householdNotePlaceholder: "Kurze Notiz für Haushaltsverwalter",
  },

  actions: {
    edit: "Bearbeiten",
    editProfile: "Profil bearbeiten",
    grantAccess: "Zugang gewähren",
    resetPassword: "Passwort zurücksetzen",
    disableAccess: "Zugang deaktivieren",
    enableAccess: "Zugang aktivieren",
    copy: "Kopieren",
    done: "Fertig",
    saving: "Speichern…",
    save: "Speichern",
    cancel: "Abbrechen",
  },

  roles: {
    Adult: "Erwachsener",
    Child: "Kind",
    Pet: "Haustier",

  },

  form: {
    name: "Vollständiger Name",
    role: "Rolle",
    birthDate: "Geburtsdatum (optional)",
    isManager: "Administrator",
    managerNote: "Die Administrator-Rolle ist nur für Erwachsene verfügbar.",
    preferredName: "Bevorzugter / Anzeigename",
    preferredNamePlaceholder: "Name in der App (optional)",
    primaryPhone: "Haupttelefon",
    primaryEmail: "Kontakt-E-Mail",
    householdNote: "Haushaltsnotiz",
    householdNotePlaceholder: "Kurze Notiz für Verwalter",
    editTitle: "Mitglied bearbeiten",
    editProfileTitle: "Profil bearbeiten",
    addTitle: "Mitglied hinzufügen",
    grantAccessTitle: "Zugang gewähren",
    grantAccessSubtitle: "Zugang gewähren, damit dieses Mitglied sich anmelden kann. Ein temporäres Passwort wird generiert.",
    email: "E-Mail / Benutzername",
    displayName: "Anzeigename (optional)",
    displayNamePlaceholder: "Name in der App",
    credentialsSaveWarning: "⚠ Dieses Passwort wird nicht erneut angezeigt. Kopieren und sicher mit dem Mitglied teilen.",
    newTemporaryPassword: "Neues temporäres Passwort",
    credentialsTitle: "Diese Zugangsdaten teilen",
    temporaryPassword: "Temporäres Passwort",
  },

  errors: {
    updateFailed: "Aktualisierung fehlgeschlagen.",
    addFailed: "Hinzufügen fehlgeschlagen.",
    provisionFailed: "Zugangsvergabe fehlgeschlagen.",
    regenFailed: "Passwortreset fehlgeschlagen.",
    disableFailed: "Deaktivierung fehlgeschlagen.",
    enableFailed: "Aktivierung fehlgeschlagen.",
    notFound: "Mitglied nicht gefunden.",
  },
} as const;
