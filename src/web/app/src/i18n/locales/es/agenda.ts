export default {
  nav: {
    back: "Volver",
    prevDay: "Día anterior",
    nextDay: "Día siguiente",
    prevWeek: "Semana anterior",
    nextWeek: "Semana siguiente",
    prevMonth: "Mes anterior",
    nextMonth: "Mes siguiente",
    today: "Hoy",
  },
  views: {
    day: "Día",
    week: "Semana",
    month: "Mes",
  },
  day: {
    backlog: "Pendientes",
    timeline: "Línea de tiempo",
    nothingScheduled: "--- Nada programado ---",
    noBacklogItems: "No hay elementos sin programar",
    completedSection: "Completado",
    allDay: "Todo el día",
    overdue: "Vencido",
    unscheduled: "Sin programar",
  },
  week: {
    title: "Semana",
    empty: "Nada esta semana",
  },
  month: {
    title: "Mes",
    empty: "Nada este mes",
  },
  loading: "Cargando…",
  error: "Error al cargar la agenda.",
  shared: {
    label: "Compartido",
  },
  memberNotFound: "Miembro no encontrado.",
  addEntry: "Añadir entrada",
  dateCard: {
    today: "Hoy",
  },
} as const;
