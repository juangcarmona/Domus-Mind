export default {
  pageTitle: "メンバー",
  addMember: "メンバーを追加",
  noMembers: "メンバーはまだいません。",

  groups: {
    adults: "大人・介護者",
    children: "子供",
    pets: "ペット",
    others: "その他",
  },

  youBadge: "あなた",
  managerBadge: "管理者",

  access: {
    noAccess: "アクセスなし",
    invited: "招待済み",
    passwordResetRequired: "パスワードリセット必要",
    active: "アクティブ",
    disabled: "無効",
  },

  detail: {
    back: "メンバー",
    coreDetails: "基本情報",
    access: "アクセス",
    contacts: "連絡先",
    notes: "メモ",

    name: "氏名",
    preferredName: "呼び名",
    role: "役割",
    birthDate: "生年月日",

    status: "ステータス",
    email: "アカウントメール",
    lastLogin: "最終ログイン",
    neverLoggedIn: "未ログイン",

    primaryPhone: "電話番号",
    primaryEmail: "連絡先メール",
    householdNote: "世帯メモ",
    householdNotePlaceholder: "管理者向けの短いメモ",
  },

  actions: {
    edit: "編集",
    editProfile: "プロフィール編集",
    grantAccess: "アクセス付与",
    resetPassword: "パスワードリセット",
    disableAccess: "アクセス無効化",
    enableAccess: "アクセス有効化",
    copy: "コピー",
    done: "完了",
    saving: "保存中…",
    save: "保存",
    cancel: "キャンセル",
  },

  roles: {
    Adult: "大人",
    Child: "子供",
    Pet: "ペット",

  },

  form: {
    name: "氏名",
    role: "役割",
    birthDate: "生年月日（任意）",
    isManager: "管理者",
    managerNote: "管理者の役割は大人のみ利用可能です。",
    preferredName: "呼び名 / 表示名",
    preferredNamePlaceholder: "アプリでの表示名（任意）",
    primaryPhone: "電話番号",
    primaryEmail: "連絡先メール",
    householdNote: "世帯メモ",
    householdNotePlaceholder: "管理者向けの短いメモ",
    editTitle: "メンバーを編集",
    editProfileTitle: "プロフィールを編集",
    addTitle: "メンバーを追加",
    grantAccessTitle: "アクセスを付与",
    grantAccessSubtitle: "このメンバーがサインインできるようにアクセスを付与します。仮パスワードが生成されます。",
    email: "メール / ユーザー名",
    displayName: "表示名（任意）",
    displayNamePlaceholder: "アプリでの名前",
    credentialsSaveWarning: "⚠ このパスワードは再度表示されません。今すぐコピーして安全に共有してください。",
    newTemporaryPassword: "新しい仮パスワード",
    credentialsTitle: "この認証情報を共有",
    temporaryPassword: "仮パスワード",
  },

  errors: {
    updateFailed: "更新に失敗しました。",
    addFailed: "追加に失敗しました。",
    provisionFailed: "アクセス付与に失敗しました。",
    regenFailed: "パスワードリセットに失敗しました。",
    disableFailed: "アクセス無効化に失敗しました。",
    enableFailed: "アクセス有効化に失敗しました。",
    notFound: "メンバーが見つかりません。",
  },
} as const;
