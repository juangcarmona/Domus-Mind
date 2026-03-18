export default {
  title: "コーディネーション",
  tabs: {
    timeline: "タイムライン",
    day: "日",
    week: "週",
    month: "月",
  },
  nav: {
    prevDay: "← 前日",
    nextDay: "翌日 →",
    prevWeek: "← 前週",
    nextWeek: "翌週 →",
    prevMonth: "← 前月",
    nextMonth: "翌月 →",
    today: "今日",
  },
  day: {
    title: "日",
    empty: "この日には予定がありません。",
    household: "家族全体",
    noMembers: "まだメンバーがいません。",
  },
  week: {
    empty: "今週には予定がありません。",
  },
  month: {
    empty: "予定なし。",
    weekdays: {
      sun: "日",
      mon: "月",
      tue: "火",
      wed: "水",
      thu: "木",
      fri: "金",
      sat: "土",
    },
  },
  timeline: {
    empty: "表示するものがありません。",
    scrollHint: "スクロールして時間を探索 →",
  },
  loading: "読み込み中…",
  error: "データの読み込みに失敗しました。",
} as const;
