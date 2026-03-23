export default {
  pageTitle: "Miembros",
  addMember: "Añadir miembro",
  noMembers: "Aún no hay miembros.",

  groups: {
    adults: "Adultos y cuidadores",
    children: "Niños",
    pets: "Mascotas",
    others: "Otros",
  },

  youBadge: "Tú",
  managerBadge: "Administrador",

  access: {
    noAccess: "Sin acceso",
    invited: "Invitado",
    passwordResetRequired: "Restablecimiento requerido",
    active: "Activo",
    disabled: "Desactivado",
  },

  detail: {
    back: "Miembros",
    coreDetails: "Datos principales",
    access: "Acceso",
    contacts: "Contactos",
    notes: "Notas",

    name: "Nombre completo",
    preferredName: "Nombre preferido",
    role: "Rol",
    birthDate: "Fecha de nacimiento",

    status: "Estado",
    email: "Correo de cuenta",
    lastLogin: "Último acceso",
    neverLoggedIn: "Nunca ha iniciado sesión",

    primaryPhone: "Teléfono principal",
    primaryEmail: "Correo de contacto",
    householdNote: "Nota del hogar",
    householdNotePlaceholder: "Nota breve visible para los administradores",
  },

  actions: {
    edit: "Editar",
    editProfile: "Editar perfil",
    grantAccess: "Conceder acceso",
    resetPassword: "Restablecer contraseña",
    disableAccess: "Desactivar acceso",
    enableAccess: "Activar acceso",
    copy: "Copiar",
    done: "Listo",
    saving: "Guardando…",
    save: "Guardar",
    cancel: "Cancelar",
  },

  roles: {
    Adult: "Adulto",
    Child: "Niño",
    Pet: "Mascota",

  },

  form: {
    name: "Nombre completo",
    role: "Rol",
    birthDate: "Fecha de nacimiento (opcional)",
    isManager: "Administrador",
    managerNote: "El rol de administrador solo está disponible para adultos.",
    preferredName: "Nombre preferido / de visualización",
    preferredNamePlaceholder: "Nombre que se muestra en la app (opcional)",
    primaryPhone: "Teléfono principal",
    primaryEmail: "Correo de contacto",
    householdNote: "Nota del hogar",
    householdNotePlaceholder: "Nota breve para administradores",
    editTitle: "Editar miembro",
    editProfileTitle: "Editar perfil",
    addTitle: "Añadir miembro",
    grantAccessTitle: "Conceder acceso",
    grantAccessSubtitle: "Concede acceso para que este miembro pueda iniciar sesión. Se generará una contraseña temporal.",
    email: "Correo / Nombre de usuario",
    displayName: "Nombre de visualización (opcional)",
    displayNamePlaceholder: "Nombre en la app",
    credentialsSaveWarning: "⚠ Esta contraseña no se mostrará de nuevo. Cópiala ahora y compártela de forma segura.",
    newTemporaryPassword: "Nueva contraseña temporal",
    credentialsTitle: "Comparte estas credenciales",
    temporaryPassword: "Contraseña temporal",
  },

  errors: {
    updateFailed: "Error al actualizar el miembro.",
    addFailed: "Error al añadir el miembro.",
    provisionFailed: "Error al conceder acceso.",
    regenFailed: "Error al restablecer la contraseña.",
    disableFailed: "Error al desactivar el acceso.",
    enableFailed: "Error al activar el acceso.",
    notFound: "Miembro no encontrado.",
  },
} as const;
