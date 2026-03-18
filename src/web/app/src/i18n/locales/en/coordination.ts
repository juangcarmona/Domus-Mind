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
    empty: "Nothing scheduled for this day.",
    household: "Household",
    noMembers: "No household members yet.",
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
  loading: "Loading…",
  error: "Failed to load data.",
} as const;
