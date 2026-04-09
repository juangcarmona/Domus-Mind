export default {
  nav: {
    back: "Back",
    prevDay: "Previous day",
    nextDay: "Next day",
    prevWeek: "Previous week",
    nextWeek: "Next week",
    prevMonth: "Previous month",
    nextMonth: "Next month",
    today: "Today",
  },
  views: {
    day: "Day",
    week: "Week",
    month: "Month",
  },
  day: {
    backlog: "Backlog",
    timeline: "Timeline",
    nothingScheduled: "--- Nothing scheduled ---",
    noBacklogItems: "No unscheduled items",
    completedSection: "Completed",
    allDay: "All day",
    overdue: "Overdue",
    unscheduled: "Unscheduled",
  },
  week: {
    title: "Week",
    empty: "Nothing this week",
  },
  month: {
    title: "Month",
    empty: "Nothing this month",
  },
  loading: "Loading…",
  error: "Failed to load agenda.",
  item: {
    edit: "Edit",
    openInProvider: "Open in Outlook",
    externalCalendar: "External Calendar",
    importedReadOnly: "Imported · read only",
  },
  shared: {
    label: "Shared",
  },
  memberNotFound: "Member not found.",
  addEntry: "Add entry",
  dateCard: {
    today: "Today",
  },
} as const;
