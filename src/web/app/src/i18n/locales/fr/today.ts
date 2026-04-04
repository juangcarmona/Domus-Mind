export default {
  title: "Coordination",
  tabs: {
    timeline: "Calendrier",
    day: "Jour",
    week: "Semaine",
    month: "Mois",
  },
  nav: {
    prevDay: "← Jour précédent",
    nextDay: "Jour suivant →",
    prevWeek: "← Semaine précédente",
    nextWeek: "Semaine suivante →",
    prevMonth: "← Mois précédent",
    nextMonth: "Mois suivant →",
    today: "Aujourd'hui",
  },
  day: {
    title: "Jour",
    empty: "Rien de prévu pour ce jour.",
    todayEmpty: "Rien de prévu",
    todayNothingScheduled: "Rien aujourd'hui.",
    nothingToday: "Rien aujourd'hui",
    household: "Foyer",
    noMembers: "Aucun membre du foyer pour l'instant.",
    completedSection: "Terminé",
  },
  week: {
    empty: "Rien de prévu pour cette semaine.",
  },
  month: {
    empty: "Rien de prévu.",
    weekdays: {
      sun: "Dim",
      mon: "Lun",
      tue: "Mar",
      wed: "Mer",
      thu: "Jeu",
      fri: "Ven",
      sat: "Sam",
    },
  },
  timeline: {
    empty: "Rien à afficher.",
    scrollHint: "Faites défiler pour explorer le temps →",
  },
  item: {
    edit: "Modifier",
    select: "Sélectionner un élément pour voir les détails",
  },
  loading: "Chargement…",
  error: "Impossible de charger les données.",
} as const;
