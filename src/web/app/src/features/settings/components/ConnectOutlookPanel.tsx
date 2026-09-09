import { useState, useEffect } from "react";
import { useTranslation } from "react-i18next";
import { useAppDispatch } from "../../../store/hooks";
import { connectOutlook } from "../../../store/externalCalendarSlice";
import { externalCalendarApi } from "../../../api/externalCalendarApi";
import type { ApiError } from "../../../api/request";

interface Props {
  familyId: string;
  memberId: string;
  onConnected: () => void;
  onCancel: () => void;
}

const REDIRECT_PATH = "/settings/oauth/outlook/callback";

export function ConnectOutlookPanel({ familyId, memberId, onConnected, onCancel }: Props) {
  const { t } = useTranslation("settings");
  const { t: tCommon } = useTranslation("common");
  const dispatch = useAppDispatch();

  const [label, setLabel] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Detect OAuth callback: if the URL contains ?code=... and we're on the callback path,
  // complete the connection automatically.
  useEffect(() => {
    const url = new URL(window.location.href);
    const code = url.searchParams.get("code");
    const state = url.searchParams.get("state");

    if (!code || !window.location.pathname.includes("oauth/outlook/callback")) return;

    const redirectUri = `${window.location.origin}${REDIRECT_PATH}`;

    // Parse state: familyId:memberId
    const [stateFamilyId, stateMemberId] = (state ?? ":").split(":");
    if (!stateFamilyId || !stateMemberId) return;

    // Clear the query string so a reload doesn't re-submit
    window.history.replaceState({}, "", window.location.pathname);

    setLoading(true);
    dispatch(
      connectOutlook({
        familyId: stateFamilyId,
        memberId: stateMemberId,
        body: { authorizationCode: code, redirectUri },
      }),
    )
      .unwrap()
      .then(() => onConnected())
      .catch((err: ApiError) => setError(err.message ?? tCommon("failed")))
      .finally(() => setLoading(false));
  }, []); // eslint-disable-line react-hooks/exhaustive-deps

  async function handleConnect() {
    setLoading(true);
    setError(null);
    try {
      const redirectUri = `${window.location.origin}${REDIRECT_PATH}`;
      const { authUrl } = await externalCalendarApi.getOutlookAuthUrl(familyId, memberId, redirectUri);
      // Redirect to Microsoft OAuth
      window.location.href = authUrl;
    } catch (err) {
      setError((err as ApiError).message ?? tCommon("failed"));
      setLoading(false);
    }
  }

  return (
    <div className="connect-outlook-panel">
      <h4 className="connect-outlook-title">{t("connectedCalendars.connectOutlookTitle")}</h4>
      <p className="connect-outlook-description">{t("connectedCalendars.connectOutlookDescription")}</p>

      {error && <p className="form-error">{error}</p>}

      <div className="form-group" style={{ maxWidth: 360 }}>
        <label htmlFor="outlook-label">{t("connectedCalendars.accountLabel")}</label>
        <input
          id="outlook-label"
          type="text"
          className="form-control"
          placeholder={t("connectedCalendars.accountLabelPlaceholder")}
          value={label}
          onChange={(e) => setLabel(e.target.value)}
          disabled={loading}
        />
      </div>

      <div className="connect-outlook-actions">
        <button
          type="button"
          className="btn btn-primary"
          onClick={handleConnect}
          disabled={loading}
        >
          {loading ? tCommon("loading") : t("connectedCalendars.authorizeWithMicrosoft")}
        </button>
        <button
          type="button"
          className="btn btn-secondary"
          onClick={onCancel}
          disabled={loading}
        >
          {tCommon("cancel")}
        </button>
      </div>
    </div>
  );
}
