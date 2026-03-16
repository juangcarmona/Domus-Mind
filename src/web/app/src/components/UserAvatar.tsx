interface UserAvatarProps {
  name: string;
  imageUrl?: string;
  size?: number;
  onClick?: () => void;
}

export function UserAvatar({ name, imageUrl, size = 32, onClick }: UserAvatarProps) {
  const initial = name.trim() ? name.trim()[0].toUpperCase() : "?";

  return (
    <button
      className="user-avatar"
      style={{ width: size, height: size, fontSize: Math.round(size * 0.42) }}
      onClick={onClick}
      aria-label="Open user menu"
      title={name}
      type="button"
    >
      {imageUrl ? (
        <img src={imageUrl} alt="" aria-hidden="true" />
      ) : (
        <span aria-hidden="true">{initial}</span>
      )}
    </button>
  );
}
