import { useState } from "react";
import { useTranslation } from "react-i18next";
import { PlanCrudForm } from "./PlanCrudForm";
import { RoutineCrudForm } from "./RoutineCrudForm";
import { TaskCrudForm } from "./TaskCrudForm";

type ConceptStep = "choose" | "plan" | "task" | "routine";

interface Props {
  familyId: string;
  members: { memberId: string; name: string }[];
  onClose: () => void;
  onSuccess: () => void;
  initialStep?: ConceptStep;
}

export function PlanningAddModal({ familyId, members, onClose, onSuccess, initialStep }: Props) {
  const { t } = useTranslation("timeline");
  const { t: tCommon } = useTranslation("common");

  const [step, setStep] = useState<ConceptStep>(initialStep ?? "choose");

  return (
    <div className="modal-backdrop" onClick={onClose}>
      <div className="modal planning-modal" onClick={(e) => e.stopPropagation()}>
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

        {step === "task" && (
          <TaskCrudForm
            mode="create"
            familyId={familyId}
            onCancel={onClose}
            onSuccess={onSuccess}
          />
        )}

        {step === "plan" && (
          <PlanCrudForm
            mode="create"
            familyId={familyId}
            onCancel={onClose}
            onSuccess={onSuccess}
          />
        )}

        {step === "routine" && (
          <RoutineCrudForm
            mode="create"
            familyId={familyId}
            members={members}
            onCancel={onClose}
            onSuccess={onSuccess}
          />
        )}
      </div>
    </div>
  );
}
