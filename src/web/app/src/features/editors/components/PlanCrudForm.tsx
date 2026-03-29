import { useState, useEffect, type FormEvent } from "react";
import { useTranslation } from "react-i18next";
import { domusmindApi } from "../../../api/domusmindApi";
import { scheduleEvent } from "../../../store/plansSlice";
import { fetchAreas } from "../../../store/areasSlice";
import { useAppDispatch, useAppSelector } from "../../../store/hooks";
import { toLocalDateInput, toLocalTimeInput } from "../utils";
import { DateInput } from "../../../components/DateInput";
import { EventChecklistSection } from "../../shared-lists/components/EventChecklistSection";
import { calendarApi } from "../../../api/calendarApi";

interface PlanCrudFormProps {
  mode: "create" | "edit";
  familyId: string;
  eventId?: string;
  initialTitle?: string;
  initialStartDate?: string | null;
  initialStartClock?: string | null;
  initialEndDate?: string | null;
  initialEndClock?: string | null;
  initialStartTime?: string | null;
  initialEndTime?: string | null;
  initialDescription?: string | null;
  initialColor?: string | null;
  /** Pre-selects the area picker (create mode only). */
  initialAreaId?: string;
  initialParticipantMemberIds?: string[];
  members?: { memberId: string; name: string }[];
  onCancel: () => void;
  onSuccess: () => void | Promise<void>;
}

export function PlanCrudForm({
  mode,
  familyId,
  eventId,
  initialTitle,
  initialStartDate,
  initialStartClock,
  initialEndDate,
  initialEndClock,
  initialStartTime,
  initialEndTime,
  initialDescription,
  initialColor,
  initialAreaId,
  initialParticipantMemberIds,
  members,
  onCancel,
  onSuccess,
}: PlanCrudFormProps) {
  const dispatch = useAppDispatch();
  const { t: tPlans } = useTranslation("plans");
  const { t: tCommon } = useTranslation("common");
  const { t: tAreas } = useTranslation("areas");

  const areas = useAppSelector((s) => s.areas.items);
  const areasStatus = useAppSelector((s) => s.areas.status);

  useEffect(() => {
    if (areasStatus === "idle") dispatch(fetchAreas(familyId));
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [areasStatus, familyId]);

  const [title, setTitle] = useState(initialTitle ?? "");
  const [startDate, setStartDate] = useState(
    initialStartDate ?? toLocalDateInput(initialStartTime),
  );
  const [startTime, setStartTime] = useState(
    initialStartClock !== undefined
      ? (initialStartClock ?? "")
      : toLocalTimeInput(initialStartTime),
  );
  const [endDate, setEndDate] = useState(
    initialEndDate ?? toLocalDateInput(initialEndTime),
  );
  const [endTime, setEndTime] = useState(
    initialEndClock !== undefined
      ? (initialEndClock ?? "")
      : toLocalTimeInput(initialEndTime),
  );
  const [description, setDescription] = useState(initialDescription ?? "");
  const [color, setColor] = useState(
    initialColor ?? "#3B82F6",
  );
  const [selectedAreaId, setSelectedAreaId] = useState(initialAreaId ?? "");
  const [scope, setScope] = useState<"Household" | "Members">(
    (initialParticipantMemberIds?.length ?? 0) > 0 ? "Members" : "Household",
  );
  const [participantMemberIds, setParticipantMemberIds] = useState<string[]>(
    initialParticipantMemberIds ?? [],
  );
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (initialColor || !selectedAreaId) return;
    const selected = areas.find((a) => a.areaId === selectedAreaId);
    if (selected?.color) setColor(selected.color);
  }, [areas, selectedAreaId, initialColor]);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setSubmitting(true);
    setError(null);

    // Domain invariant: every plan must span a range (DayRange or Range).
    if (!endDate) {
      setError(tPlans("form.endDateRequired"));
      setSubmitting(false);
      return;
    }

    if (endDate && startTime && !endTime) {
      setError(tPlans("form.endTimeRequired"));
      setSubmitting(false);
      return;
    }

    if (mode === "create") {
      if (!title.trim() || !startDate) {
        setSubmitting(false);
        return;
      }
      const result = await dispatch(
        scheduleEvent({
          familyId,
          title: title.trim(),
          date: startDate,
          time: startTime || undefined,
          endDate: endDate,
          endTime: endTime || undefined,
          description: description.trim() || undefined,
          color,
          areaId: selectedAreaId || null,
          participantMemberIds: participantMemberIds.length > 0 ? participantMemberIds : undefined,
        }),
      );
      setSubmitting(false);
      if (scheduleEvent.fulfilled.match(result)) {
        await Promise.resolve(onSuccess());
      } else {
        setError((result.payload as string) ?? tCommon("failed"));
      }
      return;
    }

    if (!eventId || !startDate) {
      setSubmitting(false);
      return;
    }

    try {
      await domusmindApi.rescheduleEvent(eventId, {
        date: startDate,
        time: startTime || undefined,
        endDate: endDate,
        endTime: endTime || undefined,
        title: title.trim(),
        description: description.trim() || null,
        color,
      });

      // Sync participant changes: diff initial vs current and drive
      // the add/remove endpoints individually (RescheduleEvent has no
      // participant field — participants are managed by their own commands).
      const initial = initialParticipantMemberIds ?? [];
      const toAdd = participantMemberIds.filter((id) => !initial.includes(id));
      const toRemove = initial.filter((id) => !participantMemberIds.includes(id));

      await Promise.all([
        ...toAdd.map((id) => calendarApi.addEventParticipant(eventId, id)),
        ...toRemove.map((id) => calendarApi.removeEventParticipant(eventId, id)),
      ]);

      setSubmitting(false);
      await Promise.resolve(onSuccess());
    } catch (err) {
      setError((err as { message?: string }).message ?? tCommon("failed"));
      setSubmitting(false);
    }
  }

  return (
    <>
      <h2>{mode === "create" ? tPlans("add") : tPlans("title")}</h2>
      <form onSubmit={handleSubmit}>
        <div className="form-group">
          <label htmlFor="plan-form-title">{tPlans("form.title")}</label>
          <input
            id="plan-form-title"
            className="form-control"
            type="text"
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            required
            autoFocus
          />
        </div>
        <div className="inline-form">
          <div className="form-group" style={{ flex: 1 }}>
            <label htmlFor="plan-form-start-date">{tPlans("form.startDate")}</label>
            <DateInput
              id="plan-form-start-date"
              className="form-control"
              value={startDate}
              onChange={setStartDate}
              required
            />
          </div>
          <div className="form-group" style={{ flex: 1 }}>
            <label htmlFor="plan-form-start-time">{tPlans("form.startTime")}</label>
            <input
              id="plan-form-start-time"
              className="form-control"
              type="time"
              value={startTime}
              onChange={(e) => setStartTime(e.target.value)}
            />
          </div>
        </div>
        <div className="inline-form">
          <div className="form-group" style={{ flex: 1 }}>
            <label htmlFor="plan-form-end-date">{tPlans("form.endDate")}</label>
            <DateInput
              id="plan-form-end-date"
              className="form-control"
              value={endDate}
              onChange={setEndDate}
              required
            />
          </div>
          <div className="form-group" style={{ flex: 1 }}>
            <label htmlFor="plan-form-end-time">{tPlans("form.endTime")}</label>
            <input
              id="plan-form-end-time"
              className="form-control"
              type="time"
              value={endTime}
              onChange={(e) => setEndTime(e.target.value)}
            />
          </div>
        </div>
        <div className="form-group">
          <label htmlFor="plan-form-desc">{tPlans("form.description")}</label>
          <input
            id="plan-form-desc"
            className="form-control"
            type="text"
            value={description}
            onChange={(e) => setDescription(e.target.value)}
          />
        </div>
        {areas.length > 0 && (
          <div className="form-group">
            <label htmlFor="plan-form-area">{tAreas("areaLabel")}</label>
            <select
              id="plan-form-area"
              className="form-control"
              value={selectedAreaId}
              onChange={(e) => {
                const id = e.target.value;
                setSelectedAreaId(id);
                if (!id) return;
                const selected = areas.find((a) => a.areaId === id);
                if (selected?.color) setColor(selected.color);
              }}
            >
              <option value="">{tAreas("noArea")}</option>
              {areas.map((a) => (
                <option key={a.areaId} value={a.areaId}>{a.name}</option>
              ))}
            </select>
          </div>
        )}
        <div className="form-group">
          <label htmlFor="plan-form-color">{tPlans("form.colorLabel")}</label>
          <input
            id="plan-form-color"
            className="form-control"
            type="color"
            value={color}
            onChange={(e) => setColor(e.target.value.toUpperCase())}
          />
        </div>
        {members && members.length > 0 && (
          <>
            <div className="form-group">
              <label htmlFor="plan-form-scope">{tPlans("form.scopeLabel")}</label>
              <select
                id="plan-form-scope"
                className="form-control"
                value={scope}
                onChange={(e) => {
                  const next = e.target.value as "Household" | "Members";
                  setScope(next);
                  if (next === "Household") {
                    setParticipantMemberIds([]);
                  }
                }}
              >
                <option value="Household">{tPlans("form.scopeHousehold")}</option>
                <option value="Members">{tPlans("form.scopeMembers")}</option>
              </select>
            </div>
            {scope === "Members" && (
              <div className="form-group">
                <label>{tPlans("form.targetMembersLabel")}</label>
                <div style={{ display: "flex", flexWrap: "wrap", gap: "0.5rem" }}>
                  {members.map((m) => (
                    <label
                      key={m.memberId}
                      style={{ display: "flex", alignItems: "center", gap: "0.25rem" }}
                    >
                      <input
                        type="checkbox"
                        checked={participantMemberIds.includes(m.memberId)}
                        onChange={(e) => {
                          if (e.target.checked) {
                            setParticipantMemberIds((prev) => [...prev, m.memberId]);
                          } else {
                            setParticipantMemberIds((prev) => prev.filter((id) => id !== m.memberId));
                          }
                        }}
                      />
                      {m.name}
                    </label>
                  ))}
                </div>
              </div>
            )}
          </>
        )}
        {error && <p className="error-msg">{error}</p>}
        {mode === "edit" && eventId && (
          <EventChecklistSection eventId={eventId} familyId={familyId} />
        )}
        <div className="modal-footer">
          <button type="button" className="btn btn-ghost" onClick={onCancel}>
            {tCommon("cancel")}
          </button>
          <button type="submit" className="btn" disabled={submitting}>
            {submitting ? tCommon("saving") : mode === "create" ? tPlans("form.save") : tCommon("save")}
          </button>
        </div>
      </form>
    </>
  );
}
