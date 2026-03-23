export default {
  pageTitle: "Membres",
  addMember: "Ajouter un membre",
  noMembers: "Aucun membre pour l'instant.",

  groups: {
    adults: "Adultes & aidants",
    children: "Enfants",
    pets: "Animaux",
    others: "Autres",
  },

  youBadge: "Vous",
  managerBadge: "Administrateur",

  access: {
    noAccess: "Aucun accès",
    invited: "Invité",
    passwordResetRequired: "Réinitialisation requise",
    active: "Actif",
    disabled: "Désactivé",
  },

  detail: {
    back: "Membres",
    coreDetails: "Informations principales",
    access: "Accès",
    contacts: "Contacts",
    notes: "Notes",

    name: "Nom complet",
    preferredName: "Nom préféré",
    role: "Rôle",
    birthDate: "Date de naissance",

    status: "Statut",
    email: "Email du compte",
    lastLogin: "Dernière connexion",
    neverLoggedIn: "Jamais connecté",

    primaryPhone: "Téléphone principal",
    primaryEmail: "Email de contact",
    householdNote: "Note du foyer",
    householdNotePlaceholder: "Note courte visible par les administrateurs",
  },

  actions: {
    edit: "Modifier",
    editProfile: "Modifier le profil",
    grantAccess: "Accorder l'accès",
    resetPassword: "Réinitialiser le mot de passe",
    disableAccess: "Désactiver l'accès",
    enableAccess: "Activer l'accès",
    copy: "Copier",
    done: "Terminé",
    saving: "Enregistrement…",
    save: "Enregistrer",
    cancel: "Annuler",
  },

  roles: {
    Adult: "Adulte",
    Child: "Enfant",
    Pet: "Animal",

  },

  form: {
    name: "Nom complet",
    role: "Rôle",
    birthDate: "Date de naissance (optionnel)",
    isManager: "Administrateur",
    managerNote: "Le rôle administrateur est réservé aux adultes.",
    preferredName: "Nom préféré / affiché",
    preferredNamePlaceholder: "Nom affiché dans l'app (optionnel)",
    primaryPhone: "Téléphone principal",
    primaryEmail: "Email de contact",
    householdNote: "Note du foyer",
    householdNotePlaceholder: "Note courte pour les administrateurs",
    editTitle: "Modifier le membre",
    editProfileTitle: "Modifier le profil",
    addTitle: "Ajouter un membre",
    grantAccessTitle: "Accorder l'accès",
    grantAccessSubtitle: "Accordez l'accès pour que ce membre puisse se connecter. Un mot de passe temporaire sera généré.",
    email: "Email / Nom d'utilisateur",
    displayName: "Nom d'affichage (optionnel)",
    displayNamePlaceholder: "Nom dans l'app",
    credentialsSaveWarning: "⚠ Ce mot de passe ne sera plus affiché. Copiez-le et partagez-le de façon sécurisée.",
    newTemporaryPassword: "Nouveau mot de passe temporaire",
    credentialsTitle: "Partagez ces identifiants",
    temporaryPassword: "Mot de passe temporaire",
  },

  errors: {
    updateFailed: "Échec de la mise à jour.",
    addFailed: "Échec de l'ajout du membre.",
    provisionFailed: "Échec de l'attribution de l'accès.",
    regenFailed: "Échec de la réinitialisation du mot de passe.",
    disableFailed: "Échec de la désactivation de l'accès.",
    enableFailed: "Échec de l'activation de l'accès.",
    notFound: "Membre introuvable.",
  },
} as const;
