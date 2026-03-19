import { useState, type FormEvent } from "react";
import { useTranslation } from "react-i18next";
import { useAppDispatch } from "../store/hooks";
import { scheduleEvent } from "../store/plansSlice";
import { createTask } from "../store/tasksSlice";
import { createRoutine } from "../store/routinesSlice";

type ConceptStep = "choose" | "plan" | "task" | "routine";

interface Props {
  familyId: string;
  members: { memberId: string; name: string }[];
  onClose: () => void;
  onSuccess: () => void;
  initialStep?: ConceptStep;
}

export function PlanningAddModal({ familyId, members, onClose, onSuccess, initialStep }: Props) {
  const dispatch = useAppDispatch();
  const { t } = useTranslation("timeline");
  const { t: tPlans } = useTranslation("plans");
  const { t: tTasks } = useTranslation("tasks");
  const { t: tRoutines } = useTranslation("routines");
  const { t: tCommon } = useTranslation("common");

  const [step, setStep] = useState<ConceptStep>(initialStep ?? "choose");

  // Plan form state
  const [planTitle, setPlanTitle] = useState("");
  const [planStartDate, setPlanStartDate] = useState("");
  const [planStartTime, setPlanStartTime] = useState("");
  const [planEndDate, setPlanEndDate] = useState("");
  const [planEndTime, setPlanEndTime] = useState("");
  const [planDesc, setPlanDesc] = useState("");
  const [planSubmitting, setPlanSubmitting] = useState(false);
  const [planError, setPlanError] = useState<string | null>(null);

  // Task form state
  const [taskTitle, setTaskTitle] = useState("");
  const [taskDue, setTaskDue] = useState("");
  const [taskSubmitting, setTaskSubmitting] = useState(false);
  const [taskError, setTaskError] = useState<string | null>(null);

  // Routine form state
  const [routineName, setRoutineName] = useState("");
  const [routineScope, setRoutineScope] = useState("Household");
  const [routineKind, setRoutineKind] = useState("Scheduled");
  const [routineColor, setRoutineColor] = useState("#3B82F6");
  const [routineFrequency, setRoutineFrequency] = useState("Weekly");
  const [routineDaysOfWeek, setRoutineDaysOfWeek] = useState<number[]>([]);
  const [routineDaysOfMonth, setRoutineDaysOfMonth] = useState("");
  const [routineMonthOfYear, setRoutineMonthOfYear] = useState("");
  const [routineTime, setRoutineTime] = useState("");
  const [routineTargetMemberIds, setRoutineTargetMemberIds] = useState<string[]>([]);
  const [routineSubmitting, setRoutineSubmitting] = useState(false);
  const [routineError, setRoutineError] = useState<string | null>(null);

  function goBack() {
    setStep("choose");
    setPlanError(null);
    setTaskError(null);
    setRoutineError(null);
  }

  async function handlePlanSubmit(e: FormEvent) {
    e.preventDefault();
    if (!planTitle.trim() || !planStartDate) return;
    setPlanSubmitting(true);
    setPlanError(null);
    const result = await dispatch(
      scheduleEvent({
        familyId,
        title: planTitle.trim(),
        date: planStartDate,
        time: planStartTime || undefined,
        endDate: planEndDate || undefined,
        endTime: planEndTime || undefined,
        description: planDesc.trim() || undefined,
      }),
    );
    setPlanSubmitting(false);
    if (scheduleEvent.fulfilled.match(result)) {
      onSuccess();
    } else {
      setPlanError((result.payload as string) ?? tCommon("failed"));
    }
  }

  async function handleTaskSubmit(e: FormEvent) {
    e.preventDefault();
    if (!taskTitle.trim()) return;
    setTaskSubmitting(true);
    setTaskError(null);
    const result = await dispatch(
      createTask({
        familyId,
        title: taskTitle.trim(),
        dueDate: taskDue ? new Date(taskDue).toISOString() : null,
      }),
    );
    setTaskSubmitting(false);
    if (createTask.fulfilled.match(result)) {
      onSuccess();
    } else {
      setTaskError((result.payload as string) ?? tCommon("failed"));
    }
  }

  function parseDaysOfMonth(raw: string): number[] {
    return raw
      .split(",")
      .map((s) => parseInt(s.trim(), 10))
      .filter((n) => !isNaN(n) && n >= 1 && n <= 31);
  }

  async function handleRoutineSubmit(e: FormEvent) {
    e.preventDefault();
    if (!routineName.trim()) return;
    setRoutineSubmitting(true);
    setRoutineError(null);
    const result = await dispatch(
      createRoutine({
        familyId,
        name: routineName.trim(),
        scope: routineScope,
        kind: routineKind,
        color: routineColor,
        frequency: routineFrequency,
        daysOfWeek: routineFrequency === "Weekly" ? routineDaysOfWeek : [],
        daysOfMonth:
          routineFrequency !== "Weekly" ? parseDaysOfMonth(routineDaysOfMonth) : [],
        monthOfYear:
          routineFrequency === "Yearly" && routineMonthOfYear
            ? parseInt(routineMonthOfYear, 10)
            : null,
        time: routineTime || null,
        targetMemberIds:
          routineScope === "Members" ? routineTargetMemberIds : [],
      }),
    );
    setRoutineSubmitting(false);
    if (createRoutine.fulfilled.match(result)) {
      onSuccess();
    } else {
      setRoutineError((result.payload as string) ?? tCommon("failed"));
    }
  }

  return (
    <div className="modal-backdrop" onClick={onClose}>
      <div className="modal planning-modal" onClick={(e) => e.stopPropagation()}>

        {/* ── Step 1: concept chooser ── */}
        {step === "choose" && (
          <>
            <h2>{t("planning.chooserTitle")}</h2>
            <div className="planning-choices">
              <button
                type="button"
                className="planning-choice-card"
                onClick={() => setStep("routine")}
              >
                <span className="planning-choice-label">{t("planning.routine")}</span>
                <span className="planning-choice-hint">{t("planning.routineHint")}</span>
              </button>
              <button
                type="button"
                className="planning-choice-card"
                onClick={() => setStep("task")}
              >
                <span className="planning-choice-label">{t("planning.task")}</span>
                <span className="planning-choice-hint">{t("planning.taskHint")}</span>
              </button>
              <button
                type="button"
                className="planning-choice-card"
                onClick={() => setStep("plan")}
              >
                <span className="planning-choice-label">{t("planning.plan")}</span>
                <span className="planning-choice-hint">{t("planning.planHint")}</span>
              </button>
            </div>
            <div className="modal-footer">
              <button type="button" className="btn btn-ghost" onClick={onClose}>
                {tCommon("cancel")}
              </button>
            </div>
          </>
        )}

        {/* ── Step 2a: Plan form ── */}
        {step === "plan" && (
          <>
            <h2>{tPlans("add")}</h2>
            <form onSubmit={handlePlanSubmit}>
              <div className="form-group">
                <label htmlFor="pm-plan-title">{tPlans("form.title")}</label>
                <input
                  id="pm-plan-title"
                  className="form-control"
                  type="text"
                  value={planTitle}
                  onChange={(e) => setPlanTitle(e.target.value)}
                  required
                  autoFocus
                />
              </div>
              <div className="inline-form">
                <div className="form-group" style={{ flex: 1 }}>
                  <label htmlFor="pm-plan-start-date">{tPlans("form.startDate")}</label>
                  <input
                    id="pm-plan-start-date"
                    className="form-control"
                    type="date"
                    value={planStartDate}
                    onChange={(e) => setPlanStartDate(e.target.value)}
                    required
                  />
                </div>
                <div className="form-group" style={{ flex: 1 }}>
                  <label htmlFor="pm-plan-start-time">{tPlans("form.startTime")}</label>
                  <input
                    id="pm-plan-start-time"
                    className="form-control"
                    type="time"
                    value={planStartTime}
                    onChange={(e) => setPlanStartTime(e.target.value)}
                  />
                </div>
              </div>
              <div className="inline-form">
                <div className="form-group" style={{ flex: 1 }}>
                  <label htmlFor="pm-plan-end-date">{tPlans("form.endDate")}</label>
                  <input
                    id="pm-plan-end-date"
                    className="form-control"
                    type="date"
                    value={planEndDate}
                    onChange={(e) => setPlanEndDate(e.target.value)}
                  />
                </div>
                <div className="form-group" style={{ flex: 1 }}>
                  <label htmlFor="pm-plan-end-time">{tPlans("form.endTime")}</label>
                  <input
                    id="pm-plan-end-time"
                    className="form-control"
                    type="time"
                    value={planEndTime}
                    onChange={(e) => setPlanEndTime(e.target.value)}
                  />
                </div>
              </div>
              <div className="form-group">
                <label htmlFor="pm-plan-desc">{tPlans("form.description")}</label>
                <input
                  id="pm-plan-desc"
                  className="form-control"
                  type="text"
                  value={planDesc}
                  onChange={(e) => setPlanDesc(e.target.value)}
                />
              </div>
              {planError && <p className="error-msg">{planError}</p>}
              <div className="modal-footer">
                <button type="button" className="btn btn-ghost" onClick={goBack}>
                  {t("planning.back")}
                </button>
                <button type="submit" className="btn" disabled={planSubmitting}>
                  {planSubmitting ? tCommon("saving") : tPlans("form.save")}
                </button>
              </div>
            </form>
          </>
        )}

        {/* ── Step 2b: Task form ── */}
        {step === "task" && (
          <>
            <h2>{tTasks("addHeading")}</h2>
            <form onSubmit={handleTaskSubmit}>
              <div className="form-group">
                <label htmlFor="pm-task-title">{tTasks("titleLabel")}</label>
                <input
                  id="pm-task-title"
                  className="form-control"
                  type="text"
                  value={taskTitle}
                  onChange={(e) => setTaskTitle(e.target.value)}
                  required
                  autoFocus
                  placeholder={tTasks("titlePlaceholder")}
                />
              </div>
              <div className="form-group">
                <label htmlFor="pm-task-due">{tTasks("dueLabel")}</label>
                <input
                  id="pm-task-due"
                  className="form-control"
                  type="date"
                  value={taskDue}
                  onChange={(e) => setTaskDue(e.target.value)}
                />
              </div>
              {taskError && <p className="error-msg">{taskError}</p>}
              <div className="modal-footer">
                <button type="button" className="btn btn-ghost" onClick={goBack}>
                  {t("planning.back")}
                </button>
                <button type="submit" className="btn" disabled={taskSubmitting}>
                  {taskSubmitting ? tCommon("adding") : tCommon("add")}
                </button>
              </div>
            </form>
          </>
        )}

        {/* ── Step 2c: Routine form ── */}
        {step === "routine" && (
          <>
            <h2>{tRoutines("addHeading")}</h2>
            <form onSubmit={handleRoutineSubmit}>
              <div className="form-group">
                <label htmlFor="pm-routine-name">{tRoutines("nameLabel")}</label>
                <input
                  id="pm-routine-name"
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
                <label htmlFor="pm-routine-scope">{tRoutines("scopeLabel")}</label>
                <select
                  id="pm-routine-scope"
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
                              setRoutineTargetMemberIds((prev) =>
                                prev.filter((id) => id !== m.memberId),
                              );
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
                <label htmlFor="pm-routine-frequency">{tRoutines("frequencyLabel")}</label>
                <select
                  id="pm-routine-frequency"
                  className="form-control"
                  value={routineFrequency}
                  onChange={(e) => {
                    setRoutineFrequency(e.target.value);
                    setRoutineDaysOfWeek([]);
                    setRoutineDaysOfMonth("");
                    setRoutineMonthOfYear("");
                  }}
                >
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
                              setRoutineDaysOfWeek((prev) =>
                                prev.filter((v) => v !== d.value),
                              );
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
                  <label htmlFor="pm-routine-days-of-month">
                    {tRoutines("daysOfMonthLabel")}
                  </label>
                  <input
                    id="pm-routine-days-of-month"
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
                  <label htmlFor="pm-routine-month">{tRoutines("monthOfYearLabel")}</label>
                  <select
                    id="pm-routine-month"
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
                <label htmlFor="pm-routine-kind">{tRoutines("executionTypeLabel")}</label>
                <select
                  id="pm-routine-kind"
                  className="form-control"
                  value={routineKind}
                  onChange={(e) => setRoutineKind(e.target.value)}
                >
                  <option value="Scheduled">{tRoutines("executionTypeGeneratesTasks")}</option>
                  <option value="Cue">{tRoutines("executionTypeReminderOnly")}</option>
                </select>
              </div>
              <div className="form-group">
                <label htmlFor="pm-routine-time">{tRoutines("timeLabel")}</label>
                <input
                  id="pm-routine-time"
                  className="form-control"
                  type="time"
                  value={routineTime}
                  onChange={(e) => setRoutineTime(e.target.value)}
                />
              </div>
              <div className="form-group">
                <label htmlFor="pm-routine-color">{tRoutines("colorLabel")}</label>
                <input
                  id="pm-routine-color"
                  className="form-control"
                  type="color"
                  value={routineColor}
                  onChange={(e) => setRoutineColor(e.target.value.toUpperCase())}
                />
              </div>
              {routineError && <p className="error-msg">{routineError}</p>}
              <div className="modal-footer">
                <button type="button" className="btn btn-ghost" onClick={goBack}>
                  {t("planning.back")}
                </button>
                <button type="submit" className="btn" disabled={routineSubmitting}>
                  {routineSubmitting ? tCommon("saving") : tRoutines("save")}
                </button>
              </div>
            </form>
          </>
        )}
      </div>
    </div>
  );
}
