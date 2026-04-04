import { useState, useEffect, type FormEvent } from "react";
import { useTranslation } from "react-i18next";
import { createRoutine, updateRoutine } from "../../../store/routinesSlice";
import { fetchAreas } from "../../../store/areasSlice";
import { useAppDispatch, useAppSelector } from "../../../store/hooks";
import type { RoutineListItem } from "../../../api/domusmindApi";
import { toLocalTimeInput } from "../utils";
import { MemberAvatar } from "../../settings/components/avatar/MemberAvatar";
import { DAY_ORDER } from "../../today/utils/dateUtils";

interface RoutineCrudFormProps {
  mode: "create" | "edit";
  familyId: string;
  members: {
    memberId: string;
    name: string;
    preferredName?: string | null;
    avatarIconId?: number | null;
    avatarColorId?: number | null;
    avatarInitial?: string;
  }[];
  initialRoutine?: RoutineListItem;
  /** Pre-selects the area picker when creating a new routine. */
  initialAreaId?: string;
  onCancel: () => void;
  onSuccess: () => void | Promise<void>;
}

function parseDaysOfMonth(raw: string): number[] {
  return raw
    .split(",")
    .map((s) => parseInt(s.trim(), 10))
    .filter((n) => !isNaN(n) && n >= 1 && n <= 31);
}

function toRoutineDaysOfMonthValue(days: number[]): string {
  return days.length ? days.join(",") : "";
}

export function RoutineCrudForm({
  mode,
  familyId,
  members,
  initialRoutine,
  initialAreaId,
  onCancel,
  onSuccess,
}: RoutineCrudFormProps) {
  const dispatch = useAppDispatch();
  const { t: tRoutines } = useTranslation("routines");
  const { t: tCommon } = useTranslation("common");
  const { t: tAreas } = useTranslation("areas");

  const areas = useAppSelector((s) => s.areas.items);
  const areasStatus = useAppSelector((s) => s.areas.status);
  const familyFirstDayOfWeek = useAppSelector((s) => s.household.family?.firstDayOfWeek ?? null);

  const ALL_DAYS = [
    { value: 0, label: tRoutines("sun") },
    { value: 1, label: tRoutines("mon") },
    { value: 2, label: tRoutines("tue") },
    { value: 3, label: tRoutines("wed") },
    { value: 4, label: tRoutines("thu") },
    { value: 5, label: tRoutines("fri") },
    { value: 6, label: tRoutines("sat") },
  ] as const;
  const fdow = Math.max(0, DAY_ORDER.indexOf((familyFirstDayOfWeek ?? "monday").toLowerCase()));
  const orderedDays = [...ALL_DAYS.slice(fdow), ...ALL_DAYS.slice(0, fdow)];

  useEffect(() => {
    if (areasStatus === "idle") dispatch(fetchAreas(familyId));
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [areasStatus, familyId]);

  const [routineName, setRoutineName] = useState(initialRoutine?.name ?? "");
  const [routineScope, setRoutineScope] = useState(initialRoutine?.scope ?? "Household");
  const [routineColor, setRoutineColor] = useState(
    initialRoutine?.color ?? "#3B82F6",
  );
  const [selectedAreaId, setSelectedAreaId] = useState(initialAreaId ?? initialRoutine?.areaId ?? "");
  const [routineFrequency, setRoutineFrequency] = useState(initialRoutine?.frequency ?? "Weekly");
  const [routineDaysOfWeek, setRoutineDaysOfWeek] = useState<number[]>(initialRoutine?.daysOfWeek ?? []);
  const [routineDaysOfMonth, setRoutineDaysOfMonth] = useState(toRoutineDaysOfMonthValue(initialRoutine?.daysOfMonth ?? []));
  const [routineMonthOfYear, setRoutineMonthOfYear] = useState(initialRoutine?.monthOfYear ? String(initialRoutine.monthOfYear) : "");
  const [routineTime, setRoutineTime] = useState(toLocalTimeInput(initialRoutine?.time));
  const [routineEndTime, setRoutineEndTime] = useState(toLocalTimeInput(initialRoutine?.endTime));
  const [routineTargetMemberIds, setRoutineTargetMemberIds] = useState<string[]>(initialRoutine?.targetMemberIds ?? []);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (initialRoutine?.color || !selectedAreaId) return;
    const selected = areas.find((a) => a.areaId === selectedAreaId);
    if (selected?.color) setRoutineColor(selected.color);
  }, [areas, selectedAreaId, initialRoutine?.color]);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    if (!routineName.trim()) return;

    setSubmitting(true);
    setError(null);

    const payload = {
      name: routineName.trim(),
      scope: routineScope,
      kind: "Scheduled",
      color: routineColor,
      frequency: routineFrequency,
      daysOfWeek: routineFrequency === "Weekly" ? routineDaysOfWeek : [],
      daysOfMonth: routineFrequency !== "Weekly" ? parseDaysOfMonth(routineDaysOfMonth) : [],
      monthOfYear:
        routineFrequency === "Yearly" && routineMonthOfYear
          ? parseInt(routineMonthOfYear, 10)
          : null,
      time: routineTime || null,
      endTime: (routineTime && routineEndTime) ? routineEndTime : null,
      targetMemberIds: routineScope === "Members" ? routineTargetMemberIds : [],
      areaId: selectedAreaId || null,
    };

    if (mode === "create") {
      const result = await dispatch(createRoutine({ familyId, ...payload }));
      setSubmitting(false);
      if (createRoutine.fulfilled.match(result)) {
        await Promise.resolve(onSuccess());
      } else {
        setError((result.payload as string) ?? tCommon("failed"));
      }
      return;
    }

    if (!initialRoutine?.routineId) {
      setSubmitting(false);
      return;
    }

    const result = await dispatch(
      updateRoutine({
        routineId: initialRoutine.routineId,
        ...payload,
      }),
    );
    setSubmitting(false);
    if (updateRoutine.fulfilled.match(result)) {
      await Promise.resolve(onSuccess());
    } else {
      setError((result.payload as string) ?? tCommon("failed"));
    }
  }

  return (
    <>
      <h2>{mode === "create" ? tRoutines("addHeading") : tRoutines("title")}</h2>
      <form onSubmit={handleSubmit}>
        <div className="form-group">
          <label htmlFor="routine-form-name">{tRoutines("nameLabel")}</label>
          <input
            id="routine-form-name"
            className="form-control"
            type="text"
            value={routineName}
            onChange={(e) => setRoutineName(e.target.value)}
            required
            autoFocus
            placeholder={tRoutines("namePlaceholder")}
          />
        </div>
        {areas.length > 0 && (
          <div className="form-group">
            <label htmlFor="routine-form-area">{tAreas("areaLabel")}</label>
            <select
              id="routine-form-area"
              className="form-control"
              value={selectedAreaId}
              onChange={(e) => {
                const id = e.target.value;
                setSelectedAreaId(id);
                if (!id) return;
                const selected = areas.find((a) => a.areaId === id);
                if (selected?.color) setRoutineColor(selected.color);
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
          <label htmlFor="routine-form-scope">{tRoutines("scopeLabel")}</label>
          <select
            id="routine-form-scope"
            className="form-control"
            value={routineScope}
            onChange={(e) => setRoutineScope(e.target.value)}
          >
            <option value="Household">{tRoutines("scopeHousehold")}</option>
            <option value="Members">{tRoutines("scopeMembers")}</option>
          </select>
        </div>
        {routineScope === "Members" && members.length > 0 && (
          <div className="form-group">
            <label>{tRoutines("targetMembersLabel")}</label>
            <div style={{ display: "flex", flexWrap: "wrap", gap: "0.5rem" }}>
              {members.map((m) => {
                const displayName = m.preferredName || m.name;
                return (
                  <label
                    key={m.memberId}
                    style={{ display: "flex", alignItems: "center", gap: "0.4rem", cursor: "pointer" }}
                  >
                    <input
                      type="checkbox"
                      checked={routineTargetMemberIds.includes(m.memberId)}
                      onChange={(e) => {
                        if (e.target.checked) {
                          setRoutineTargetMemberIds((prev) => [...prev, m.memberId]);
                        } else {
                          setRoutineTargetMemberIds((prev) => prev.filter((id) => id !== m.memberId));
                        }
                      }}
                    />
                    <MemberAvatar
                      initial={m.avatarInitial ?? displayName[0]?.toUpperCase() ?? "?"}
                      avatarIconId={m.avatarIconId}
                      avatarColorId={m.avatarColorId}
                      size={22}
                    />
                    {displayName}
                  </label>
                );
              })}
            </div>
          </div>
        )}
        <div className="form-group">
          <label htmlFor="routine-form-frequency">{tRoutines("frequencyLabel")}</label>
          <select
            id="routine-form-frequency"
            className="form-control"
            value={routineFrequency}
            onChange={(e) => {
              setRoutineFrequency(e.target.value);
              setRoutineDaysOfWeek([]);
              setRoutineDaysOfMonth("");
              setRoutineMonthOfYear("");
            }}
          >
            <option value="Daily">{tRoutines("frequencyDaily")}</option>
            <option value="Weekly">{tRoutines("frequencyWeekly")}</option>
            <option value="Monthly">{tRoutines("frequencyMonthly")}</option>
            <option value="Yearly">{tRoutines("frequencyYearly")}</option>
          </select>
        </div>
        {routineFrequency === "Weekly" && (
          <div className="form-group">
            <label>{tRoutines("daysOfWeekLabel")}</label>
            <div className="editor-day-chips">
              {orderedDays.map((d) => (
                <button
                  key={d.value}
                  type="button"
                  className={`editor-day-chip${routineDaysOfWeek.includes(d.value) ? " editor-day-chip--active" : ""}`}
                  onClick={() => {
                    setRoutineDaysOfWeek((prev) =>
                      prev.includes(d.value)
                        ? prev.filter((v) => v !== d.value)
                        : [...prev, d.value].sort(),
                    );
                  }}
                >
                  {d.label}
                </button>
              ))}
            </div>
          </div>
        )}
        {(routineFrequency === "Monthly" || routineFrequency === "Yearly") && (
          <div className="form-group">
            <label htmlFor="routine-form-days-of-month">{tRoutines("daysOfMonthLabel")}</label>
            <input
              id="routine-form-days-of-month"
              className="form-control"
              type="text"
              value={routineDaysOfMonth}
              onChange={(e) => setRoutineDaysOfMonth(e.target.value)}
              placeholder={tRoutines("daysOfMonthPlaceholder")}
            />
          </div>
        )}
        {routineFrequency === "Yearly" && (
          <div className="form-group">
            <label htmlFor="routine-form-month">{tRoutines("monthOfYearLabel")}</label>
            <select
              id="routine-form-month"
              className="form-control"
              value={routineMonthOfYear}
              onChange={(e) => setRoutineMonthOfYear(e.target.value)}
            >
              <option value="">{tRoutines("selectMonth")}</option>
              {Array.from({ length: 12 }, (_, i) => (
                <option key={i + 1} value={String(i + 1)}>
                  {new Date(2000, i, 1).toLocaleString(undefined, { month: "long" })}
                </option>
              ))}
            </select>
          </div>
        )}
        <div className="inline-form">
          <div className="form-group" style={{ flex: 1 }}>
            <label htmlFor="routine-form-time">{tRoutines("timeLabel")}</label>
            <input
              id="routine-form-time"
              className="form-control"
              type="time"
              value={routineTime}
              onChange={(e) => {
                setRoutineTime(e.target.value);
                if (!e.target.value) setRoutineEndTime("");
              }}
            />
          </div>
          {routineTime && (
            <div className="form-group" style={{ flex: 1 }}>
              <label htmlFor="routine-form-end-time">{tRoutines("endTimeLabel")}</label>
              <input
                id="routine-form-end-time"
                className="form-control"
                type="time"
                value={routineEndTime}
                min={routineTime}
                onChange={(e) => setRoutineEndTime(e.target.value)}
              />
            </div>
          )}
        </div>
        <div className="form-group">
          <label htmlFor="routine-form-color">{tRoutines("colorLabel")}</label>
          <input
            id="routine-form-color"
            className="form-control"
            type="color"
            value={routineColor}
            onChange={(e) => setRoutineColor(e.target.value.toUpperCase())}
          />
        </div>
        {error && <p className="error-msg">{error}</p>}
        <div className="modal-footer">
          <button type="button" className="btn btn-ghost" onClick={onCancel}>
            {tCommon("cancel")}
          </button>
          <button type="submit" className="btn" disabled={submitting}>
            {submitting ? tCommon("saving") : tCommon("save")}
          </button>
        </div>
      </form>
    </>
  );
}
