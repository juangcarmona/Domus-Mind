import { useTranslation } from "react-i18next";
import { AREA_PALETTE } from "../utils/areaColors";

interface AreaDetailHeaderProps {
  areaName: string;
  color: string;
  isEditingName: boolean;
  nameInput: string;
  renaming: boolean;
  renameError: string | null;
  onSwatchClick: (color: string) => void;
  onColorChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
  onNameClick: () => void;
  onNameInputChange: (value: string) => void;
  onNameSave: () => void;
  onNameKeyDown: (e: React.KeyboardEvent<HTMLInputElement>) => void;
}

export function AreaDetailHeader({
  areaName,
  color,
  isEditingName,
  nameInput,
  renaming,
  renameError,
  onSwatchClick,
  onColorChange,
  onNameClick,
  onNameInputChange,
  onNameSave,
  onNameKeyDown,
}: AreaDetailHeaderProps) {
  const { t } = useTranslation("areas");
  const isCustomColor = !AREA_PALETTE.includes(color);

  return (
    <div className="area-detail-header" style={{ borderLeftColor: color }}>
      <div className="area-detail-color-wrap">
        <div className="area-color-picker">
          {AREA_PALETTE.map((c) => (
            <button
              key={c}
              type="button"
              className={`area-color-swatch${color === c ? " area-color-swatch--active" : ""}`}
              style={{ background: c }}
              onClick={() => onSwatchClick(c)}
              aria-label={c}
            />
          ))}
          <div
            className={`area-color-custom-trigger${isCustomColor ? " area-color-custom-trigger--active" : ""}`}
            style={isCustomColor ? { background: color } : undefined}
            title={t("customColor")}
          >
            {!isCustomColor && <span className="area-color-custom-label" aria-hidden="true">+</span>}
            <input
              type="color"
              value={color}
              onChange={onColorChange}
              aria-label={t("customColor")}
            />
          </div>
        </div>
        <span className="area-color-hex">{color.toUpperCase()}</span>
      </div>
      <div className="area-detail-identity">
        {isEditingName ? (
          <input
            className="area-detail-name-input"
            value={nameInput}
            autoFocus
            disabled={renaming}
            onChange={(e) => onNameInputChange(e.target.value)}
            onBlur={onNameSave}
            onKeyDown={onNameKeyDown}
            aria-label={t("renameHint")}
          />
        ) : (
          <h1
            className="area-detail-name"
            style={{ color, cursor: "pointer" }}
            title={t("renameHint")}
            onClick={onNameClick}
          >
            {areaName}
          </h1>
        )}
        <p className="area-detail-subtitle">{t("renameHint")}</p>
        {renameError && <p className="error-msg" style={{ marginTop: "0.4rem" }}>{renameError}</p>}
      </div>
    </div>
  );
}
