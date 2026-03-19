import { useNavigate, useParams } from "react-router-dom";
import { EditEntityModal, type EditableEntityType } from "../../../components/EditEntityModal";

function isDetailType(value: string | undefined): value is EditableEntityType {
  return value === "task" || value === "routine" || value === "event";
}

export function DetailPage() {
  const { type: rawType, id } = useParams<{ type: string; id: string }>();
  const navigate = useNavigate();

  const type = isDetailType(rawType) ? rawType : null;
  if (!type || !id) return null;

  return <EditEntityModal type={type} id={id} onClose={() => navigate(-1)} />;
}
