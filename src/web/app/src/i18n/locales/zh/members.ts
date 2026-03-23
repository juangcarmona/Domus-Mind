export default {
  pageTitle: "成员",
  addMember: "添加成员",
  noMembers: "暂无成员。",

  groups: {
    adults: "成人和看护者",
    children: "儿童",
    pets: "宠物",
    others: "其他",
  },

  youBadge: "你",
  managerBadge: "管理员",

  access: {
    noAccess: "无访问权限",
    invited: "已邀请",
    passwordResetRequired: "需要重置密码",
    active: "活跃",
    disabled: "已禁用",
  },

  detail: {
    back: "成员",
    coreDetails: "基本信息",
    access: "访问权限",
    contacts: "联系方式",
    notes: "备注",

    name: "全名",
    preferredName: "常用名",
    role: "角色",
    birthDate: "出生日期",

    status: "状态",
    email: "账户邮箱",
    lastLogin: "最后登录",
    neverLoggedIn: "从未登录",

    primaryPhone: "主要电话",
    primaryEmail: "联系邮箱",
    householdNote: "家庭备注",
    householdNotePlaceholder: "管理员可见的简短备注",
  },

  actions: {
    edit: "编辑",
    editProfile: "编辑档案",
    grantAccess: "授予访问权限",
    resetPassword: "重置密码",
    disableAccess: "禁用访问",
    enableAccess: "启用访问",
    copy: "复制",
    done: "完成",
    saving: "保存中…",
    save: "保存",
    cancel: "取消",
  },

  roles: {
    Adult: "成人",
    Child: "儿童",
    Pet: "宠物",

  },

  form: {
    name: "全名",
    role: "角色",
    birthDate: "出生日期（可选）",
    isManager: "管理员",
    managerNote: "管理员角色仅适用于成人。",
    preferredName: "常用名 / 显示名",
    preferredNamePlaceholder: "应用中显示的名称（可选）",
    primaryPhone: "主要电话",
    primaryEmail: "联系邮箱",
    householdNote: "家庭备注",
    householdNotePlaceholder: "管理员可见的简短备注",
    editTitle: "编辑成员",
    editProfileTitle: "编辑档案",
    addTitle: "添加成员",
    grantAccessTitle: "授予访问权限",
    grantAccessSubtitle: "授予访问权限，使该成员可以登录。将生成临时密码。",
    email: "邮箱 / 用户名",
    displayName: "显示名称（可选）",
    displayNamePlaceholder: "应用中的名称",
    credentialsSaveWarning: "⚠ 此密码不会再次显示。请立即复制并安全地与成员共享。",
    newTemporaryPassword: "新临时密码",
    credentialsTitle: "分享这些凭据",
    temporaryPassword: "临时密码",
  },

  errors: {
    updateFailed: "更新失败。",
    addFailed: "添加失败。",
    provisionFailed: "授予访问权限失败。",
    regenFailed: "重置密码失败。",
    disableFailed: "禁用访问失败。",
    enableFailed: "启用访问失败。",
    notFound: "找不到该成员。",
  },
} as const;
