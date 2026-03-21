import { useState, type FormEvent } from "react";
import { useTranslation } from "react-i18next";
import { createRoutine, updateRoutine } from "../../../store/routinesSlice";
import { useAppDispatch } from "../../../store/hooks";
import type { RoutineListItem } from "../../../api/domusmindApi";
import { toLocalTimeInput } from "../utils";

interface RoutineCrudFormProps {
  mode: "create" | "edit";
  familyId: string;
  members: { memberId: string; name: string }[];
  initialRoutine?: RoutineListItem;
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
  onCancel,
  onSuccess,
}: RoutineCrudFormProps) {
  const dispatch = useAppDispatch();
  const { t: tRoutines } = useTranslation("routines");
  const { t: tCommon } = useTranslation("common");

  const [routineName, setRoutineName] = useState(initialRoutine?.name ?? "");
  const [routineScope, setRoutineScope] = useState(initialRoutine?.scope ?? "Household");
  const [routineColor, setRoutineColor] = useState(initialRoutine?.color ?? "#3B82F6");
  const [routineFrequency, setRoutineFrequency] = useState(initialRoutine?.frequency ?? "Weekly");
  const [routineDaysOfWeek, setRoutineDaysOfWeek] = useState<number[]>(initialRoutine?.daysOfWeek ?? []);
  const [routineDaysOfMonth, setRoutineDaysOfMonth] = useState(toRoutineDaysOfMonthValue(initialRoutine?.daysOfMonth ?? []));
  const [routineMonthOfYear, setRoutineMonthOfYear] = useState(initialRoutine?.monthOfYear ? String(initialRoutine.monthOfYear) : "");
  const [routineTime, setRoutineTime] = useState(toLocalTimeInput(initialRoutine?.time));
  const [routineTargetMemberIds, setRoutineTargetMemberIds] = useState<string[]>(initialRoutine?.targetMemberIds ?? []);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

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
      targetMemberIds: routineScope === "Members" ? routineTargetMemberIds : [],
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
              {members.map((m) => (
                <label
                  key={m.memberId}
                  style={{ display: "flex", alignItems: "center", gap: "0.25rem" }}
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
                  {m.name}
                </label>
              ))}
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
            <div style={{ display: "flex", flexWrap: "wrap", gap: "0.5rem" }}>
              {[
                { value: 0, label: tRoutines("sun") },
                { value: 1, label: tRoutines("mon") },
                { value: 2, label: tRoutines("tue") },
                { value: 3, label: tRoutines("wed") },
                { value: 4, label: tRoutines("thu") },
                { value: 5, label: tRoutines("fri") },
                { value: 6, label: tRoutines("sat") },
              ].map((d) => (
                <label
                  key={d.value}
                  style={{ display: "flex", alignItems: "center", gap: "0.25rem" }}
                >
                  <input
                    type="checkbox"
                    checked={routineDaysOfWeek.includes(d.value)}
                    onChange={(e) => {
                      if (e.target.checked) {
                        setRoutineDaysOfWeek((prev) => [...prev, d.value].sort());
                      } else {
                        setRoutineDaysOfWeek((prev) => prev.filter((v) => v !== d.value));
                      }
                    }}
                  />
                  {d.label}
                </label>
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
        <div className="form-group">
          <label htmlFor="routine-form-time">{tRoutines("timeLabel")}</label>
          <input
            id="routine-form-time"
            className="form-control"
            type="time"
            value={routineTime}
            onChange={(e) => setRoutineTime(e.target.value)}
          />
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
