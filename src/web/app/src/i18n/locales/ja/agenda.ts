export default {
  nav: {
    back: "戻る",
    prevDay: "前の日",
    nextDay: "次の日",
    prevWeek: "前の週",
    nextWeek: "次の週",
    prevMonth: "前の月",
    nextMonth: "次の月",
    today: "今日",
  },
  views: {
    day: "日",
    week: "週",
    month: "月",
  },
  day: {
    backlog: "バックログ",
    timeline: "タイムライン",
    nothingScheduled: "--- 予定なし ---",
    noBacklogItems: "未スケジュールのアイテムなし",
    completedSection: "完了",
    allDay: "終日",
    overdue: "期限超過",
    unscheduled: "未スケジュール",
  },
  week: {
    title: "週",
    empty: "今週は何もありません",
  },
  month: {
    title: "月",
    empty: "今月は何もありません",
  },
  loading: "読み込み中…",
  error: "アジェンダを読み込めませんでした。",
  shared: {
    label: "共有",
  },
  memberNotFound: "メンバーが見つかりません。",
  addEntry: "エントリを追加",
  dateCard: {
    today: "今日",
  },
} as const;
