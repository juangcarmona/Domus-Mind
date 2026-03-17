export default {
  title: "Zeitstrahl",
  refresh: "Aktualisieren",
  empty: "Noch nichts hier.",
  unassigned: "nicht zugewiesen",
  filter: { all: "Alle", plans: "Pläne", chores: "Aufgaben", routines: "Routinen" },
  groups: {
    Overdue: "Überfällig",
    Today: "Heute",
    Tomorrow: "Morgen",
    ThisWeek: "Diese Woche",
    Later: "Später",
    Undated: "Routinen & laufend",
  },
  actions: { complete: "Erledigt", cancel: "Abbrechen" },
} as const;
