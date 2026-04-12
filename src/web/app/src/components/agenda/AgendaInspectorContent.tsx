import { useEffect, useMemo } from "react";
import { useTranslation } from "react-i18next";
import { fetchPlans } from "../../store/plansSlice";
import { fetchRoutines } from "../../store/routinesSlice";
import { useAppDispatch, useAppSelector } from "../../store/hooks";
import { fetchTimeline } from "../../store/timelineSlice";
import { PlanCrudForm } from "../../features/editors/components/PlanCrudForm";
import { RoutineCrudForm } from "../../features/editors/components/RoutineCrudForm";
import { TaskCrudForm } from "../../features/editors/components/TaskCrudForm";
import type { CalendarEntry } from "../../features/today/utils/calendarEntry";

export interface AgendaInspectorMember {
  memberId: string;
  name: string;
  preferredName?: string | null;
  avatarIconId?: number | null;
  avatarColorId?: number | null;
  avatarInitial?: string;
}

export function AgendaInlineEntityEditor({
  entry,
  familyId,
  members,
  onCancel,
  onSaved,
}: {
  entry: CalendarEntry;
  familyId: string;
  members: AgendaInspectorMember[];
  onCancel: () => void;
  onSaved: () => void | Promise<void>;
}) {
  const dispatch = useAppDispatch();
  const { t } = useTranslation("common");

  const timeline = useAppSelector((s) => s.timeline.data);
  const timelineStatus = useAppSelector((s) => s.timeline.status);
  const routines = useAppSelector((s) => s.routines.items);
  const routinesStatus = useAppSelector((s) => s.routines.status);
  const plans = useAppSelector((s) => s.plans.items);
  const plansStatus = useAppSelector((s) => s.plans.status);

  useEffect(() => {
    if (!familyId) return;
    if (entry.sourceType === "task" && timelineStatus === "idle") {
      dispatch(fetchTimeline({ familyId, types: "Task" }));
    }
    if (entry.sourceType === "routine" && routinesStatus === "idle") {
      dispatch(fetchRoutines(familyId));
    }
    if (entry.sourceType === "event" && plansStatus === "idle") {
      dispatch(fetchPlans({ familyId }));
    }
  }, [dispatch, entry.sourceType, familyId, plansStatus, routinesStatus, timelineStatus]);

  const task = useMemo(() => {
    if (entry.sourceType !== "task" || !timeline) return null;
    const all = timeline.groups.flatMap((g) => g.entries);
    return all.find((item) => item.entryType === "Task" && item.entryId === entry.id) ?? null;
  }, [entry.id, entry.sourceType, timeline]);

  const routine = useMemo(() => {
    if (entry.sourceType !== "routine") return null;
    return routines.find((item) => item.routineId === entry.id) ?? null;
  }, [entry.id, entry.sourceType, routines]);

  const event = useMemo(() => {
    if (entry.sourceType !== "event") return null;
    return plans.find((item) => item.calendarEventId === entry.id) ?? null;
  }, [entry.id, entry.sourceType, plans]);

  const loading =
    (entry.sourceType === "task" && timelineStatus === "loading") ||
    (entry.sourceType === "routine" && routinesStatus === "loading") ||
    (entry.sourceType === "event" && plansStatus === "loading");

  if (loading) {
    return <div className="loading-wrap">{t("loading")}</div>;
  }

  if (entry.sourceType === "task" && task) {
    return (
      <TaskCrudForm
        mode="edit"
        familyId={familyId}
        taskId={task.entryId}
        initialTitle={task.title}
        initialDueDate={task.effectiveDate}
        initialColor={task.color}
        initialAssigneeId={task.assigneeId ?? undefined}
        members={members.map((m) => ({ memberId: m.memberId, name: m.preferredName || m.name }))}
        onCancel={onCancel}
        onSuccess={onSaved}
      />
    );
  }

  if (entry.sourceType === "routine" && routine) {
    return (
      <RoutineCrudForm
        mode="edit"
        familyId={familyId}
        members={members}
        initialRoutine={routine}
        onCancel={onCancel}
        onSuccess={onSaved}
      />
    );
  }

  if (entry.sourceType === "event" && event) {
    return (
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
        onCancel={onCancel}
        onSuccess={onSaved}
      />
    );
  }

  return <p className="agenda-inspector-warning">{t("failed")}</p>;
}

export function AgendaProjectedListItemBridge({
  entry,
  onOpenInLists,
}: {
  entry: CalendarEntry;
  onOpenInLists: (listId: string, itemId: string) => void;
}) {
  const { t } = useTranslation("agenda");
  const timeSummary = [entry.dueDate, entry.reminder, entry.repeat].filter(Boolean).join(" · ");

  return (
    <div className="agenda-inspector-item">
      <p className="agenda-inspector-item-type-cue">{t("item.typeListItem")}</p>
      <p className="agenda-inspector-item-meta">
        <span className="agenda-inspector-item-label">{t("item.status")}</span>
        {" "}{entry.isCompleted ? t("item.checked") : t("item.unchecked")}
      </p>
      {entry.listName && (
        <p className="agenda-inspector-item-meta">
          <span className="agenda-inspector-item-label">{t("item.listName")}</span>
          {" "}{entry.listName}
        </p>
      )}
      {timeSummary && (
        <p className="agenda-inspector-item-meta">
          <span className="agenda-inspector-item-label">{t("item.timeSummary")}</span>
          {" "}{timeSummary}
        </p>
      )}
      {entry.itemAreaName && (
        <p className="agenda-inspector-item-meta">
          <span className="agenda-inspector-item-label">{t("item.itemArea")}</span>
          {" "}{entry.itemAreaName}
        </p>
      )}
      {entry.targetMemberName && (
        <p className="agenda-inspector-item-meta">
          <span className="agenda-inspector-item-label">{t("item.targetMember")}</span>
          {" "}{entry.targetMemberName}
        </p>
      )}
      {entry.note && (
        <p className="agenda-inspector-item-meta">
          <span className="agenda-inspector-item-label">{t("item.note")}</span>
          {" "}{entry.note}
        </p>
      )}
      {entry.listId && (
        <div className="agenda-inspector-item-actions">
          <button
            type="button"
            className="btn btn-ghost btn-sm"
            onClick={() => onOpenInLists(entry.listId!, entry.id)}
          >
            {t("item.openInLists")}
          </button>
        </div>
      )}
    </div>
  );
}

export function AgendaReadOnlyEntryDetail({ entry }: { entry: CalendarEntry }) {
  const { t } = useTranslation("agenda");

  const typeLabel =
    entry.isReadOnly && entry.sourceLabel
      ? entry.sourceLabel.toUpperCase()
      : entry.sourceType === "routine"
      ? t("item.typeRoutine")
      : entry.sourceType === "task"
      ? t("item.typeTask")
      : t("item.typePlan");

  const dateLine = entry.date
    ? entry.endDate && entry.endDate !== entry.date
      ? `${entry.date} - ${entry.endDate}`
      : entry.date
    : null;

  return (
    <div className="agenda-inspector-item">
      <p className="agenda-inspector-item-type-cue">
        {typeLabel}
        {entry.isReadOnly && (
          <span className="agenda-inspector-item-readonly-badge">
            {" · "}{t("item.readOnly")}
          </span>
        )}
      </p>

      {dateLine && <p className="agenda-inspector-item-meta">{dateLine}</p>}
      {entry.isAllDay && <p className="agenda-inspector-item-meta">{t("day.allDay")}</p>}
      {entry.time && (
        <p className="agenda-inspector-item-meta">
          {entry.time}
          {entry.endTime ? ` - ${entry.endTime}` : ""}
        </p>
      )}

      {entry.subtitle && (
        <p className="agenda-inspector-item-meta">
          <span className="agenda-inspector-item-label">{t("item.participants")}</span>
          {" "}{entry.subtitle}
        </p>
      )}

      {entry.calendarName && (
        <p className="agenda-inspector-item-meta">
          <span className="agenda-inspector-item-label">{t("item.calendar")}</span>
          {" "}{entry.calendarName}
        </p>
      )}

      {entry.location && (
        <p className="agenda-inspector-item-meta">
          <span className="agenda-inspector-item-label">{t("item.location")}</span>
          {" "}{entry.location}
        </p>
      )}

      {entry.openInProviderUrl && (
        <a
          href={entry.openInProviderUrl}
          target="_blank"
          rel="noreferrer"
          className="agenda-inspector-external-link"
        >
          {t("item.openInProvider")}
        </a>
      )}
    </div>
  );
}
