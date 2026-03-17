export default {
  title: "タイムライン",
  refresh: "更新",
  empty: "まだ何もありません。",
  unassigned: "未割り当て",
  filter: { all: "すべて", plans: "予定", chores: "家事", routines: "ルーティン" },
  groups: {
    Overdue: "期限超過",
    Today: "今日",
    Tomorrow: "明日",
    ThisWeek: "今週",
    Later: "後日",
    Undated: "ルーティン・進行中",
  },
  actions: { complete: "完了", cancel: "キャンセル" },
} as const;
