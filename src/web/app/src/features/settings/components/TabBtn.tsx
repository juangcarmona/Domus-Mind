interface TabBtnProps {
  id: string;
  label: string;
  active: string;
  onSelect: (id: string) => void;
}

export function TabBtn({ id, label, active, onSelect }: TabBtnProps) {
  return (
    <button
      type="button"
      onClick={() => onSelect(id)}
      style={{
        padding: "0.4rem 0.75rem",
        fontSize: "0.82rem",
        border: "none",
        borderBottom: active === id ? "2px solid var(--primary)" : "2px solid transparent",
        background: "none",
        cursor: "pointer",
        color: active === id ? "var(--primary)" : "var(--muted)",
        fontWeight: active === id ? 600 : 400,
      }}
    >
      {label}
    </button>
  );
}
