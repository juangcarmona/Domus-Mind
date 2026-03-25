import { useState, type FormEvent } from "react";
import { useTranslation } from "react-i18next";
import { useAppDispatch, useAppSelector } from "../../../store/hooks";
import { createSharedList } from "../../../store/sharedListsSlice";

interface CreateSharedListModalProps {
  onClose: () => void;
}

export function CreateSharedListModal({ onClose }: CreateSharedListModalProps) {
  const { t } = useTranslation("sharedLists");
  const { t: tCommon } = useTranslation("common");
  const dispatch = useAppDispatch();
  const familyId = useAppSelector((s) => s.household.family?.familyId);

  const [name, setName] = useState("");
  const [kind, setKind] = useState("General");
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    if (!name.trim() || !familyId) return;
    setSubmitting(true);
    setError(null);
    const result = await dispatch(createSharedList({ familyId, name: name.trim(), kind }));
    setSubmitting(false);
    if (createSharedList.fulfilled.match(result)) {
      onClose();
    } else {
      setError((result.payload as string) ?? t("createError"));
    }
  }

  return (
    <div className="modal-backdrop" onClick={onClose}>
      <div className="modal" onClick={(e) => e.stopPropagation()}>
        <h2>{t("createHeading")}</h2>
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label htmlFor="list-name-input">{t("nameLabel")}</label>
            <input
              id="list-name-input"
              className="form-control"
              type="text"
              value={name}
              onChange={(e) => setName(e.target.value)}
              required
              autoFocus
              placeholder={t("namePlaceholder")}
            />
          </div>
          <div className="form-group">
            <label htmlFor="list-kind-select">{t("kindLabel")}</label>
            <select
              id="list-kind-select"
              className="form-control"
              value={kind}
              onChange={(e) => setKind(e.target.value)}
            >
              <option value="General">{t("kindGeneral")}</option>
              <option value="Groceries">{t("kindGroceries")}</option>
              <option value="Packing">{t("kindPacking")}</option>
            </select>
          </div>
          {error && <p className="error-msg">{error}</p>}
          <div className="modal-footer">
            <button type="button" className="btn btn-ghost" onClick={onClose}>
              {tCommon("cancel")}
            </button>
            <button
              type="submit"
              className="btn"
              disabled={submitting || !name.trim()}
            >
              {submitting ? tCommon("creating") : tCommon("create")}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
