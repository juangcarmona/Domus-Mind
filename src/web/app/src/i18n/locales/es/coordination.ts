export default {
  title: "Coordinación",
  tabs: {
    timeline: "Línea de tiempo",
    day: "Día",
    week: "Semana",
    month: "Mes",
  },
  nav: {
    prevDay: "← Día anterior",
    nextDay: "Día siguiente →",
    prevWeek: "← Semana anterior",
    nextWeek: "Semana siguiente →",
    prevMonth: "← Mes anterior",
    nextMonth: "Mes siguiente →",
    today: "Hoy",
  },
  day: {
    title: "Día",
    empty: "Nada programado para este día.",
    household: "Hogar",
    noMembers: "Aún no hay miembros del hogar.",
  },
  week: {
    empty: "Nada programado para esta semana.",
  },
  month: {
    empty: "Nada programado.",
    weekdays: {
      sun: "Dom",
      mon: "Lun",
      tue: "Mar",
      wed: "Mié",
      thu: "Jue",
      fri: "Vie",
      sat: "Sáb",
    },
  },
  timeline: {
    empty: "Nada que mostrar.",
    scrollHint: "Desplázate para explorar el tiempo →",
  },
  loading: "Cargando…",
  error: "Error al cargar los datos.",
} as const;
