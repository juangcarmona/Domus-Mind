/**
 * AreaDetailPage — compatibility redirect.
 *
 * The primary area detail experience now lives entirely in the inspector on AreasPage.
 * Any direct deep-link to /areas/:areaId is redirected back to /areas, with the
 * target area id passed as location state so AreasPage can restore the selection.
 */
import { useParams, Navigate } from "react-router-dom";

export function AreaDetailPage() {
  const { areaId } = useParams<{ areaId: string }>();
  return <Navigate to="/areas" state={{ selectAreaId: areaId }} replace />;
}
