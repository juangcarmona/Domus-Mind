export default {
  title: "Timeline",
  refresh: "Refresh",
  empty: "Nothing here yet.",
  unassigned: "unassigned",
  filter: { all: "All", plans: "Plans", chores: "Chores", routines: "Routines" },
  groups: {
    Overdue: "Overdue",
    Today: "Today",
    Tomorrow: "Tomorrow",
    ThisWeek: "This week",
    Later: "Later",
    Undated: "Routines & ongoing",
  },
  actions: { complete: "Complete", cancel: "Cancel" },
} as const;
