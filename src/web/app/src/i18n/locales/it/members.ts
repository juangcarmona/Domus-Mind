export default {
  pageTitle: "Membri",
  addMember: "Aggiungi membro",
  noMembers: "Nessun membro ancora.",

  groups: {
    adults: "Adulti e assistenti",
    children: "Bambini",
    pets: "Animali",
    others: "Altri",
  },

  youBadge: "Tu",
  managerBadge: "Amministratore",

  access: {
    noAccess: "Nessun accesso",
    invited: "Invitato",
    passwordResetRequired: "Reset richiesto",
    active: "Attivo",
    disabled: "Disabilitato",
  },

  detail: {
    back: "Membri",
    coreDetails: "Dati principali",
    access: "Accesso",
    contacts: "Contatti",
    notes: "Note",

    name: "Nome completo",
    preferredName: "Nome preferito",
    role: "Ruolo",
    birthDate: "Data di nascita",

    status: "Stato",
    email: "Email account",
    lastLogin: "Ultimo accesso",
    neverLoggedIn: "Mai connesso",

    primaryPhone: "Telefono principale",
    primaryEmail: "Email di contatto",
    householdNote: "Nota familiare",
    householdNotePlaceholder: "Nota breve visibile agli amministratori",
  },

  actions: {
    edit: "Modifica",
    editProfile: "Modifica profilo",
    grantAccess: "Concedi accesso",
    resetPassword: "Reimposta password",
    disableAccess: "Disabilita accesso",
    enableAccess: "Abilita accesso",
    copy: "Copia",
    done: "Fatto",
    saving: "Salvataggio…",
    save: "Salva",
    cancel: "Annulla",
  },

  roles: {
    Adult: "Adulto",
    Child: "Bambino",
    Pet: "Animale",

  },

  form: {
    name: "Nome completo",
    role: "Ruolo",
    birthDate: "Data di nascita (opzionale)",
    isManager: "Amministratore",
    managerNote: "Il ruolo amministratore è disponibile solo per gli adulti.",
    preferredName: "Nome preferito / visualizzato",
    preferredNamePlaceholder: "Nome mostrato nell'app (opzionale)",
    primaryPhone: "Telefono principale",
    primaryEmail: "Email di contatto",
    householdNote: "Nota familiare",
    householdNotePlaceholder: "Nota breve per gli amministratori",
    editTitle: "Modifica membro",
    editProfileTitle: "Modifica profilo",
    addTitle: "Aggiungi membro",
    grantAccessTitle: "Concedi accesso",
    grantAccessSubtitle: "Concedi l'accesso in modo che questo membro possa accedere. Verrà generata una password temporanea.",
    email: "Email / Nome utente",
    displayName: "Nome visualizzato (opzionale)",
    displayNamePlaceholder: "Nome nell'app",
    credentialsSaveWarning: "⚠ Questa password non verrà più mostrata. Copiala ora e condividila in modo sicuro.",
    newTemporaryPassword: "Nuova password temporanea",
    credentialsTitle: "Condividi queste credenziali",
    temporaryPassword: "Password temporanea",
  },

  errors: {
    updateFailed: "Aggiornamento non riuscito.",
    addFailed: "Aggiunta non riuscita.",
    provisionFailed: "Concessione accesso non riuscita.",
    regenFailed: "Reset password non riuscito.",
    disableFailed: "Disabilitazione non riuscita.",
    enableFailed: "Abilitazione non riuscita.",
    notFound: "Membro non trovato.",
  },
} as const;
