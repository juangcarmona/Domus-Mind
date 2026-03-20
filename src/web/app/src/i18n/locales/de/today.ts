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
    household: "Haushalt",
    noMembers: "Noch keine Haushaltsmitglieder.",
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
  loading: "Wird geladen…",
  error: "Daten konnten nicht geladen werden.",
} as const;
