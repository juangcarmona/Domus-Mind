export default {
  title: "Coordination",
  tabs: {
    timeline: "Timeline",
    day: "Day",
    week: "Week",
    month: "Month",
  },
  nav: {
    prevDay: "← Previous day",
    nextDay: "Next day →",
    prevWeek: "← Previous week",
    nextWeek: "Next week →",
    prevMonth: "← Previous month",
    nextMonth: "Next month →",
    today: "Today",
  },
  day: {
    title: "Day",
    empty: "Nothing here today.",
    todayEmpty: "Nothing scheduled",
    todayNothingScheduled: "Nothing here today.",
    nothingToday: "Nothing today",
    household: "Household",
    noMembers: "No household members yet.",
    completedSection: "Completed",
  },
  week: {
    empty: "Nothing scheduled for this week.",
  },
  month: {
    empty: "Nothing scheduled.",
    weekdays: {
      sun: "Sun",
      mon: "Mon",
      tue: "Tue",
      wed: "Wed",
      thu: "Thu",
      fri: "Fri",
      sat: "Sat",
    },
  },
  timeline: {
    empty: "Nothing to show.",
    scrollHint: "Scroll to explore time →",
  },
  item: {
    edit: "Edit",
    select: "Select an item to see details",
  },
  loading: "Loading…",
  error: "Failed to load data.",
} as const;
