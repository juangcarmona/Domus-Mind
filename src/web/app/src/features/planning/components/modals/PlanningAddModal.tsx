import { useState } from "react";
import { useTranslation } from "react-i18next";
import { PlanCrudForm } from "../../../editors/components/PlanCrudForm";
import { RoutineCrudForm } from "../../../editors/components/RoutineCrudForm";
import { TaskCrudForm } from "../../../editors/components/TaskCrudForm";
import "../../../editors/editors.css";

type ConceptStep = "choose" | "plan" | "task" | "routine";

export interface PlanningAddModalDefaults {
  /** Area to pre-select in the form picker. */
  areaId?: string;
  /** Member to pre-select as task assignee. */
  assigneeId?: string;
  /** Members to pre-select as plan participants. */
  participantMemberIds?: string[];
  /** Date to pre-fill in create forms (YYYY-MM-DD). */
  initialStartDate?: string;
  /** Clock time to pre-fill in the plan form (HH:MM). */
  initialStartClock?: string;
}

interface Props {
  familyId: string;
  members: { memberId: string; name: string }[];
  onClose: () => void;
  onSuccess: () => void;
  initialStep?: ConceptStep;
  /** Optional context defaults pre-populated in create forms. */
  defaults?: PlanningAddModalDefaults;
}

export function PlanningAddModal({ familyId, members, onClose, onSuccess, initialStep, defaults }: Props) {
  const { t } = useTranslation("plans");
  const { t: tCommon } = useTranslation("common");

  const [step, setStep] = useState<ConceptStep>(initialStep ?? "choose");

  return (
    <div className="modal-backdrop" onClick={onClose}>
      <div className="modal planning-modal" onClick={(e) => e.stopPropagation()}>
        {step === "choose" && (
          <>
            <h2>{t("chooser.title")}</h2>
            <div className="planning-choices">
              <button
                type="button"
                className="planning-choice-card"
                onClick={() => setStep("routine")}
              >
                <span className="planning-choice-label">{t("chooser.routine")}</span>
                <span className="planning-choice-hint">{t("chooser.routineHint")}</span>
              </button>
              <button
                type="button"
                className="planning-choice-card"
                onClick={() => setStep("task")}
              >
                <span className="planning-choice-label">{t("chooser.task")}</span>
                <span className="planning-choice-hint">{t("chooser.taskHint")}</span>
              </button>
              <button
                type="button"
                className="planning-choice-card"
                onClick={() => setStep("plan")}
              >
                <span className="planning-choice-label">{t("chooser.plan")}</span>
                <span className="planning-choice-hint">{t("chooser.planHint")}</span>
              </button>
            </div>
            <div className="modal-footer">
              <button type="button" className="btn btn-ghost" onClick={onClose}>
                {tCommon("cancel")}
              </button>
            </div>
          </>
        )}

        {step === "task" && (
          <TaskCrudForm
            mode="create"
            familyId={familyId}
            members={members}
            initialAreaId={defaults?.areaId}
            initialAssigneeId={defaults?.assigneeId}
            initialDueDate={defaults?.initialStartDate ?? null}
            onCancel={onClose}
            onSuccess={onSuccess}
          />
        )}

        {step === "plan" && (
          <PlanCrudForm
            mode="create"
            familyId={familyId}
            members={members}
            initialAreaId={defaults?.areaId}
            initialParticipantMemberIds={defaults?.participantMemberIds}
            initialStartDate={defaults?.initialStartDate ?? null}
            initialStartClock={defaults?.initialStartClock ?? null}
            initialEndDate={defaults?.initialStartDate ?? null}
            onCancel={onClose}
            onSuccess={onSuccess}
          />
        )}

        {step === "routine" && (
          <RoutineCrudForm
            mode="create"
            familyId={familyId}
            members={members}
            initialAreaId={defaults?.areaId}
            onCancel={onClose}
            onSuccess={onSuccess}
          />
        )}
      </div>
    </div>
  );
}
