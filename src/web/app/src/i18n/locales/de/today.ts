export default {
  title: "Koordination",
  tabs: {
    timeline: "Zeitstrahl",
    day: "Tag",
    week: "Woche",
    month: "Monat",
  },
  nav: {
    prevDay: "← Vorheriger Tag",
    nextDay: "Nächster Tag →",
    prevWeek: "← Vorherige Woche",
    nextWeek: "Nächste Woche →",
    prevMonth: "← Vorheriger Monat",
    nextMonth: "Nächster Monat →",
    today: "Heute",
  },
  day: {
    title: "Tag",
    empty: "Für diesen Tag nichts geplant.",
    todayEmpty: "Nichts geplant",
    todayNothingScheduled: "Heute nichts.",
    nothingToday: "Heute nichts",
    household: "Haushalt",
    noMembers: "Noch keine Haushaltsmitglieder.",
    completedSection: "Erledigt",
  },
  week: {
    empty: "Für diese Woche nichts geplant.",
  },
  month: {
    empty: "Nichts geplant.",
    weekdays: {
      sun: "So",
      mon: "Mo",
      tue: "Di",
      wed: "Mi",
      thu: "Do",
      fri: "Fr",
      sat: "Sa",
    },
  },
  timeline: {
    empty: "Nichts zu zeigen.",
    scrollHint: "Scrollen, um die Zeit zu erkunden →",
  },
  item: {
    edit: "Bearbeiten",
    select: "Element auswählen, um Details anzuzeigen",
  },
  loading: "Wird geladen…",
  error: "Daten konnten nicht geladen werden.",
} as const;
