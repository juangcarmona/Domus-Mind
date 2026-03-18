export default {
  title: "协调",
  tabs: {
    timeline: "时间线",
    day: "日",
    week: "周",
    month: "月",
  },
  nav: {
    prevDay: "← 上一天",
    nextDay: "下一天 →",
    prevWeek: "← 上一周",
    nextWeek: "下一周 →",
    prevMonth: "← 上一月",
    nextMonth: "下一月 →",
    today: "今天",
  },
  day: {
    title: "日",
    empty: "今天没有安排。",
    household: "家庭",
    noMembers: "还没有家庭成员。",
  },
  week: {
    empty: "本周没有安排。",
  },
  month: {
    empty: "没有安排。",
    weekdays: {
      sun: "日",
      mon: "一",
      tue: "二",
      wed: "三",
      thu: "四",
      fri: "五",
      sat: "六",
    },
  },
  timeline: {
    empty: "没有内容显示。",
    scrollHint: "滚动探索时间 →",
  },
  loading: "加载中…",
  error: "加载数据失败。",
} as const;
