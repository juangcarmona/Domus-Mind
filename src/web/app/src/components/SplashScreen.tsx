export function SplashScreen() {
  return (
    <div className="splash">
      <div className="splash-center">
        <div className="splash-track" />
        <div className="splash-arc" />
        <div className="splash-glow" />
        <div className="splash-icon">
          <svg width="62" height="62" viewBox="0 0 256 256" xmlns="http://www.w3.org/2000/svg" aria-hidden="true">
            <polygon fill="var(--primary)" points="128,24 24,112 24,232 232,232 232,112" />
            <rect   fill="var(--primary)" x="172" y="52" width="20" height="44" />
            <polygon fill="var(--surface)" points="60,216 60,112 128,168 196,112 196,216 164,216 164,166 128,220 92,166 92,216" />
          </svg>
        </div>
      </div>
      <span className="splash-name">DomusMind</span>
    </div>
  );
}
