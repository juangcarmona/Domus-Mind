import { useEffect, useState, useCallback } from "react";
import { useTranslation } from "react-i18next";
import { useAppDispatch, useAppSelector } from "../../../store/hooks";
import { fetchPlans, cancelEvent } from "../../../store/plansSlice";
import { fetchRoutines, pauseRoutine, resumeRoutine } from "../../../store/routinesSlice";
import { fetchAreas } from "../../../store/areasSlice";
import { completeTask, cancelTask, assignTask } from "../../../store/tasksSlice";
import { domusmindApi } from "../../../api/domusmindApi";
import { ConfirmDialog } from "../../../components/ConfirmDialog";
import { PlanningAddModal } from "../components/modals/PlanningAddModal";
import { EditEntityModal } from "../../editors/components/EditEntityModal";
import { AssignTaskModal } from "../components/modals/AssignTaskModal";
import { RoutinesTab } from "../components/RoutinesTab";
import { TasksTab } from "../components/TasksTab";
import { PlansTab } from "../components/PlansTab";
import type {
  FamilyTimelineEventItem,
  EnrichedTimelineEntry,
  RoutineListItem,
} from "../../../api/domusmindApi";

type PlanningTab = "routines" | "tasks" | "plans";

function todayIso(): string {
  const d = new Date();
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, "0")}-${String(d.getDate()).padStart(2, "0")}`;
}

interface OverviewProps {
  activeTasks: EnrichedTimelineEntry[];
  activePlans: FamilyTimelineEventItem[];
  routineItems: RoutineListItem[];
  onJumpToTab: (tab: PlanningTab) => void;
}

function PlanningOverview({ activeTasks, activePlans, routineItems, onJumpToTab }: OverviewProps) {
  const { t: tTasks } = useTranslation("tasks");
  const { t: tPlans } = useTranslation("plans");
  const { t: tRoutines } = useTranslation("routines");

  const today = todayIso();
  const overdueCount = activeTasks.filter((task) => task.isOverdue).length;
  const todayTaskCount = activeTasks.filter((task) => task.group === "Today").length;
  const todayPlanCount = activePlans.filter((p) => p.date === today).length;
  const activeRoutineCount = routineItems.filter((r) => r.status === "Active").length;

  if (!overdueCount && !todayTaskCount && !todayPlanCount && !activeRoutineCount) return null;

  return (
    <div className="planning-overview">
      {overdueCount > 0 && (
        <button
          type="button"
          className="planning-stat planning-stat--danger"
          onClick={() => onJumpToTab("tasks")}
        >
          <span className="planning-stat-count">{overdueCount}</span>
          <span className="planning-stat-label">{tTasks("groupOverdue")}</span>
        </button>
      )}
      {todayTaskCount > 0 && (
        <button
          type="button"
          className="planning-stat"
          onClick={() => onJumpToTab("tasks")}
        >
          <span className="planning-stat-count">{todayTaskCount}</span>
          <span className="planning-stat-label">{tTasks("groupToday")}</span>
        </button>
      )}
      {todayPlanCount > 0 && (
        <button
          type="button"
          className="planning-stat"
          onClick={() => onJumpToTab("plans")}
        >
          <span className="planning-stat-count">{todayPlanCount}</span>
          <span className="planning-stat-label">{tPlans("groupToday")}</span>
        </button>
      )}
      {activeRoutineCount > 0 && (
        <button
          type="button"
          className="planning-stat planning-stat--quiet"
          onClick={() => onJumpToTab("routines")}
        >
          <span className="planning-stat-count">{activeRoutineCount}</span>
          <span className="planning-stat-label">{tRoutines("active")}</span>
        </button>
      )}
    </div>
  );
}

export function PlanningPage() {
  const dispatch = useAppDispatch();
  const { family, members } = useAppSelector((s) => s.household);
  const { items: planItems, status: plansStatus } = useAppSelector((s) => s.plans);
  const { items: routineItems, status: routinesStatus } = useAppSelector((s) => s.routines);
  const { status: areasStatus } = useAppSelector((s) => s.areas);
  const familyId = family?.familyId;

  const { t: tRoutines } = useTranslation("routines");
  const { t: tTasks } = useTranslation("tasks");
  const { t: tPlans } = useTranslation("plans");
  const { t: tCommon } = useTranslation("common");
  const { t: tNav } = useTranslation("nav");

  const [activeTab, setActiveTab] = useState<PlanningTab>("routines");
  const [addModal, setAddModal] = useState<"plan" | "task" | "routine" | "choose" | null>(null);
  const [cancelTarget, setCancelTarget] = useState<FamilyTimelineEventItem | null>(null);
  const [assignTarget, setAssignTarget] = useState<EnrichedTimelineEntry | null>(null);
  const [editTarget, setEditTarget] = useState<{ type: "routine" | "task" | "event"; id: string } | null>(null);

  // ---- Tasks: local state (separate from shared timelineSlice) ----
  const [activeTasks, setActiveTasks] = useState<EnrichedTimelineEntry[]>([]);
  const [tasksLoading, setTasksLoading] = useState(false);
  const [taskHistory, setTaskHistory] = useState<EnrichedTimelineEntry[] | null>(null);
  const [taskHistoryLoading, setTaskHistoryLoading] = useState(false);

  // ---- Plans: history is local state, active plans live in plansSlice ----
  const [pastPlans, setPastPlans] = useState<FamilyTimelineEventItem[] | null>(null);
  const [pastPlansLoading, setPastPlansLoading] = useState(false);

  const memberMap = Object.fromEntries(members.map((m) => [m.memberId, m.name]));

  const loadPlans = useCallback(() => {
    if (familyId) dispatch(fetchPlans({ familyId, from: todayIso() }));
  }, [familyId, dispatch]);

  const loadActiveTasks = useCallback(async () => {
    if (!familyId) return;
    setTasksLoading(true);
    try {
      const res = await domusmindApi.getEnrichedTimeline(familyId, {
        types: "Task",
        statuses: "Pending",
      });
      setActiveTasks(
        res.groups.flatMap((g) => g.entries),
      );
    } finally {
      setTasksLoading(false);
    }
  }, [familyId]);

  const loadTaskHistory = useCallback(async () => {
    if (!familyId) return;
    setTaskHistoryLoading(true);
    try {
      const res = await domusmindApi.getEnrichedTimeline(familyId, {
        types: "Task",
        statuses: "Completed,Cancelled",
      });
      setTaskHistory(res.groups.flatMap((g) => g.entries));
    } finally {
      setTaskHistoryLoading(false);
    }
  }, [familyId]);

  const loadPastPlans = useCallback(async () => {
    if (!familyId) return;
    setPastPlansLoading(true);
    try {
      const res = await domusmindApi.getEvents(familyId, undefined, todayIso());
      // Exclude today's events (today is already shown in active plans) and active-status items
      // that might overlap due to the to= boundary being exclusive-ish; filter to truly past.
      const today = todayIso();
      const past = res.events.filter((e) => (e.date ?? "") < today);
      setPastPlans(past);
    } finally {
      setPastPlansLoading(false);
    }
  }, [familyId]);

  function loadRoutines() {
    if (familyId && routinesStatus === "idle") dispatch(fetchRoutines(familyId));
  }

  useEffect(() => {
    loadPlans();
    loadActiveTasks();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [familyId]);

  useEffect(() => {
    if (familyId && areasStatus === "idle") dispatch(fetchAreas(familyId));
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [familyId, areasStatus]);

  useEffect(() => {
    // Reset local task history when family changes so stale data isn't shown
    setTaskHistory(null);
    setPastPlans(null);
  }, [familyId]);

  useEffect(() => {
    loadRoutines();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [familyId, routinesStatus]);

  async function handleCancelPlan() {
    if (!cancelTarget || !familyId) return;
    await dispatch(cancelEvent({ eventId: cancelTarget.calendarEventId, familyId, from: todayIso() }));
    setCancelTarget(null);
  }

  async function handleCompleteTask(taskId: string) {
    await dispatch(completeTask(taskId));
    setTaskHistory(null); // invalidate stale history
    await loadActiveTasks();
  }

  async function handleCancelTask(taskId: string) {
    await dispatch(cancelTask(taskId));
    setTaskHistory(null); // invalidate stale history
    await loadActiveTasks();
  }

  async function handleAssign(taskId: string, memberId: string) {
    await dispatch(assignTask({ taskId, assigneeId: memberId }));
    await loadActiveTasks();
  }

  async function handlePauseRoutine(routineId: string) {
    if (!familyId) return;
    await dispatch(pauseRoutine({ routineId, familyId }));
  }

  async function handleResumeRoutine(routineId: string) {
    if (!familyId) return;
    await dispatch(resumeRoutine({ routineId, familyId }));
  }

  if (!familyId) return null;

  // Active plans: non-cancelled, start date >= today (enforced by from= parameter)
  const activePlans = planItems.filter((p) => p.status !== "Cancelled");

  const tabs: { key: PlanningTab; label: string }[] = [
    { key: "routines", label: tRoutines("title") },
    { key: "tasks", label: tTasks("title") },
    { key: "plans", label: tPlans("title") },
  ];

  return (
    <div>
      <div className="page-header">
        <h1>{tNav("planning")}</h1>
        <button className="btn" onClick={() => setAddModal("choose")}>
          + {tCommon("add")}
        </button>
      </div>

      <PlanningOverview
        activeTasks={activeTasks}
        activePlans={activePlans}
        routineItems={routineItems}
        onJumpToTab={setActiveTab}
      />

      <div className="settings-tabs" style={{ marginBottom: "1.5rem" }}>
        {tabs.map((tab) => (
          <button
            key={tab.key}
            type="button"
            className={`settings-tab${activeTab === tab.key ? " active" : ""}`}
            onClick={() => setActiveTab(tab.key)}
          >
            {tab.label}
          </button>
        ))}
      </div>

      {activeTab === "routines" && (
        <RoutinesTab
          routineItems={routineItems}
          routinesStatus={routinesStatus}
          memberMap={memberMap}
          onEdit={(id) => setEditTarget({ type: "routine", id })}
          onPause={handlePauseRoutine}
          onResume={handleResumeRoutine}
        />
      )}

      {activeTab === "tasks" && (
        <TasksTab
          activeTasks={activeTasks}
          tasksLoading={tasksLoading}
          taskHistory={taskHistory}
          taskHistoryLoading={taskHistoryLoading}
          memberMap={memberMap}
          onEdit={(id) => setEditTarget({ type: "task", id })}
          onAssign={setAssignTarget}
          onComplete={handleCompleteTask}
          onCancel={handleCancelTask}
          onLoadHistory={loadTaskHistory}
        />
      )}

      {activeTab === "plans" && (
        <PlansTab
          activePlans={activePlans}
          plansStatus={plansStatus}
          pastPlans={pastPlans}
          pastPlansLoading={pastPlansLoading}
          onEdit={(id) => setEditTarget({ type: "event", id })}
          onCancelPlan={setCancelTarget}
          onLoadPastPlans={loadPastPlans}
        />
      )}

      {addModal && (
        <PlanningAddModal
          familyId={familyId}
          members={members}
          initialStep={addModal === "choose" ? "choose" : addModal}
          onClose={() => setAddModal(null)}
          onSuccess={() => {
            setAddModal(null);
            loadPlans();
            loadActiveTasks();
            dispatch(fetchRoutines(familyId));
          }}
        />
      )}

      {assignTarget && (
        <AssignTaskModal
          entry={assignTarget}
          members={members}
          onAssign={handleAssign}
          onClose={() => setAssignTarget(null)}
        />
      )}

      {editTarget && (
        <EditEntityModal
          type={editTarget.type}
          id={editTarget.id}
          onClose={() => setEditTarget(null)}
          onEntitySaved={async () => {
            setEditTarget(null);
            loadPlans();
            await loadActiveTasks();
            await dispatch(fetchRoutines(familyId));
          }}
        />
      )}

      <ConfirmDialog
        isOpen={!!cancelTarget}
        title={tPlans("cancelEvent")}
        message={tPlans("confirmCancel")}
        confirmLabel={tPlans("yes")}
        onConfirm={handleCancelPlan}
        onCancel={() => setCancelTarget(null)}
      />
    </div>
  );
}
