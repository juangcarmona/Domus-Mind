export default {
  title: "时间线",
  refresh: "刷新",
  empty: "暂无内容。",
  unassigned: "未分配",
  filter: { all: "全部", plans: "计划", chores: "家务", routines: "例行" },
  groups: {
    Overdue: "已逾期",
    Today: "今天",
    Tomorrow: "明天",
    ThisWeek: "本周",
    Later: "以后",
    Undated: "例行及进行中",
  },
  actions: { complete: "完成", cancel: "取消" },
} as const;
