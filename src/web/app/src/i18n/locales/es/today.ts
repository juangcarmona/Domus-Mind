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
    todayEmpty: "Nada programado",
    todayNothingScheduled: "Nada hoy.",
    nothingToday: "Nada hoy",
    household: "Hogar",
    noMembers: "Aún no hay miembros del hogar.",
    completedSection: "Completado",
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
  item: {
    edit: "Editar",
    select: "Selecciona un elemento para ver los detalles",
  },
  loading: "Cargando…",
  error: "Error al cargar los datos.",
} as const;
