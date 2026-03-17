export default {
  title: "Calendrier",
  refresh: "Actualiser",
  empty: "Rien ici pour l'instant.",
  unassigned: "non attribué",
  filter: { all: "Tout", plans: "Plannings", chores: "Tâches", routines: "Routines" },
  groups: {
    Overdue: "En retard",
    Today: "Aujourd'hui",
    Tomorrow: "Demain",
    ThisWeek: "Cette semaine",
    Later: "Plus tard",
    Undated: "Routines et en cours",
  },
  actions: { complete: "Terminer", cancel: "Annuler" },
} as const;
