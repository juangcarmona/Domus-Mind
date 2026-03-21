import { useEffect, useMemo } from "react";
import { useTranslation } from "react-i18next";
import { fetchPlans } from "../../../store/plansSlice";
import { fetchRoutines } from "../../../store/routinesSlice";
import { useAppDispatch, useAppSelector } from "../../../store/hooks";
import { fetchTimeline } from "../../../store/timelineSlice";
import { PlanCrudForm } from "./PlanCrudForm";
import { RoutineCrudForm } from "./RoutineCrudForm";
import { TaskCrudForm } from "./TaskCrudForm";

export type EditableEntityType = "task" | "routine" | "event";

interface EditEntityModalProps {
  type: EditableEntityType;
  id: string;
  onClose: () => void;
  onEntitySaved?: () => void | Promise<void>;
}

export function EditEntityModal({ type, id, onClose, onEntitySaved }: EditEntityModalProps) {
  const dispatch = useAppDispatch();
  const { t: tCommon } = useTranslation("common");

  const familyId = useAppSelector((s) => s.household.family?.familyId ?? "");
  const members = useAppSelector((s) => s.household.members);
  const timeline = useAppSelector((s) => s.timeline.data);
  const timelineStatus = useAppSelector((s) => s.timeline.status);
  const routines = useAppSelector((s) => s.routines.items);
  const routinesStatus = useAppSelector((s) => s.routines.status);
  const plans = useAppSelector((s) => s.plans.items);
  const plansStatus = useAppSelector((s) => s.plans.status);

  useEffect(() => {
    if (!familyId) return;
    if (type === "task" && timelineStatus === "idle") {
      dispatch(fetchTimeline({ familyId, types: "Task" }));
    }
    if (type === "routine" && routinesStatus === "idle") {
      dispatch(fetchRoutines(familyId));
    }
    if (type === "event" && plansStatus === "idle") {
      dispatch(fetchPlans({ familyId }));
    }
  }, [dispatch, familyId, plansStatus, routinesStatus, timelineStatus, type]);

  const task = useMemo(() => {
    if (type !== "task" || !timeline) return null;
    const all = timeline.groups.flatMap((g) => g.entries);
    return all.find((entry) => entry.entryType === "Task" && entry.entryId === id) ?? null;
  }, [id, timeline, type]);

  const routine = useMemo(() => {
    if (type !== "routine") return null;
    return routines.find((r) => r.routineId === id) ?? null;
  }, [id, routines, type]);

  const event = useMemo(() => {
    if (type !== "event") return null;
    return plans.find((p) => p.calendarEventId === id) ?? null;
  }, [id, plans, type]);

  const notFound =
    (type === "task" && !task && timelineStatus !== "loading") ||
    (type === "routine" && !routine && routinesStatus !== "loading") ||
    (type === "event" && !event && plansStatus !== "loading");

  const loading =
    (type === "task" && timelineStatus === "loading") ||
    (type === "routine" && routinesStatus === "loading") ||
    (type === "event" && plansStatus === "loading");

  async function refreshAfterSave() {
    if (!familyId) {
      return;
    }

    if (type === "task") {
      await dispatch(fetchTimeline({ familyId }));
      return;
    }

    if (type === "routine") {
      await dispatch(fetchRoutines(familyId));
      return;
    }

    await dispatch(fetchPlans({ familyId }));
  }

  async function handleSuccess() {
    await refreshAfterSave();
    if (onEntitySaved) {
      await Promise.resolve(onEntitySaved());
      return;
    }
    onClose();
  }

  return (
    <div className="modal-backdrop" onClick={onClose}>
      <section
        className="modal planning-modal"
        onClick={(e) => e.stopPropagation()}
        role="region"
        aria-live="polite"
      >
        {familyId && type === "task" && task && (
          <TaskCrudForm
            mode="edit"
            familyId={familyId}
            taskId={task.entryId}
            initialTitle={task.title}
            initialDueDate={task.effectiveDate}
            initialColor={task.color}
            onCancel={onClose}
            onSuccess={handleSuccess}
          />
        )}

        {familyId && type === "routine" && routine && (
          <RoutineCrudForm
            mode="edit"
            familyId={familyId}
            members={members}
            initialRoutine={routine}
            onCancel={onClose}
            onSuccess={handleSuccess}
          />
        )}

        {familyId && type === "event" && event && (
          <PlanCrudForm
            mode="edit"
            familyId={familyId}
            eventId={event.calendarEventId}
            initialTitle={event.title}
            initialStartDate={event.date ?? null}
            initialStartClock={event.time ?? null}
            initialEndDate={event.endDate ?? null}
            initialEndClock={event.endTimeValue ?? null}
            initialStartTime={event.startTime}
            initialEndTime={event.endTime}
            initialColor={event.color}
            initialParticipantMemberIds={event.participantMemberIds}
            members={members}
            onCancel={onClose}
            onSuccess={handleSuccess}
          />
        )}

        {notFound && <p className="error-msg">{tCommon("failed")}</p>}
        {loading && <div className="loading-wrap">{tCommon("loading")}</div>}
      </section>
    </div>
  );
}
