interface AvatarProps {
  initial: string;
  size?: number;
}

export function Avatar({ initial, size = 36 }: AvatarProps) {
  return (
    <div
      style={{
        width: size,
        height: size,
        borderRadius: "50%",
        background: "color-mix(in srgb, var(--primary) 15%, transparent)",
        color: "var(--primary)",
        display: "flex",
        alignItems: "center",
        justifyContent: "center",
        fontWeight: 700,
        fontSize: size >= 40 ? "1rem" : "0.85rem",
        flexShrink: 0,
      }}
    >
      {initial}
    </div>
  );
}
