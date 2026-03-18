export default {
  title: "Coordinamento",
  tabs: {
    timeline: "Agenda",
    day: "Giorno",
    week: "Settimana",
    month: "Mese",
  },
  nav: {
    prevDay: "← Giorno precedente",
    nextDay: "Giorno successivo →",
    prevWeek: "← Settimana precedente",
    nextWeek: "Settimana successiva →",
    prevMonth: "← Mese precedente",
    nextMonth: "Mese successivo →",
    today: "Oggi",
  },
  day: {
    title: "Giorno",
    empty: "Niente in programma per questo giorno.",
    household: "Famiglia",
    noMembers: "Nessun membro della famiglia ancora.",
  },
  week: {
    empty: "Niente in programma per questa settimana.",
  },
  month: {
    empty: "Niente in programma.",
    weekdays: {
      sun: "Dom",
      mon: "Lun",
      tue: "Mar",
      wed: "Mer",
      thu: "Gio",
      fri: "Ven",
      sat: "Sab",
    },
  },
  timeline: {
    empty: "Niente da mostrare.",
    scrollHint: "Scorri per esplorare il tempo →",
  },
  loading: "Caricamento…",
  error: "Impossibile caricare i dati.",
} as const;
