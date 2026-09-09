---
name: implement
description: "Plain Concepts Lifecycle stage 3: apply the approved plan on its branch, verify the change against the project's Definition of Done in both lanes, and mark the pull request ready for review. Use when the operator asks to implement, build out or finish a planned work item, or to take a change to the Done gate."
metadata:
  plugin: pc-lifecycle
  author: "Juan García Carmona"
---

Stage 3. An approved plan → a **change that is finished and proven**. Ends at the Done gate.

**Input**: a work item identifier. Required.

Invoke deliberately. No lifecycle stage runs this one on its own.

## Steps

1. **Load the configuration** (absent → stop and say *"run the lifecycle's **adopt** capability first"*).

2. **Check the plan was approved.** `read-work-item` plus the pull request's review state via `inspect-pull-request`. No approved plan → stop and point at the **plan** capability for this item. Implementing an unreviewed plan is how a stage builds the wrong thing correctly.

3. **Continue on the plan's branch**: `checkout-branch`. Never a new branch, never a second pull request. The branch already carries the plan; the code joins it.

4. **Apply the plan, task by task**, via `apply-change`, committing incrementally so the branch keeps a narrative a reviewer can follow.

5. **Surface deviations immediately.** A plan step that cannot be followed as written is reported when it is found, not absorbed silently and mentioned at the end. Update the artifact so the plan and the code stay in step.

6. **Update the documentation the change affects**, in the same branch. Behaviour that changed and documentation that did not is a defect with a delay on it.

7. **Reconcile the task list against the actual diff**: a dedicated pass, not a memory check. A task ticked that the diff does not support is worse than a task left open.

8. **Verify against the Definition of Done** via `verify-done`. Two lanes:

   - **Deterministic**: `verify-like-ci` runs what the pipeline runs, deriving it from the pipeline definitions rather than memory; `verify-runtime` captures evidence for a user-facing change. Both feed `verify-done`.
   - **Judgement**: every remaining item is presented to the role the configuration names for it.

   Not done → the unmet items, each with what is missing and who or what closes it. Fix, and re-run the **whole** check, never the sub-command that failed.

9. **Commit and push** (push confirmation-gated). Before staging, confirm the branch is the item's branch. After pushing, confirm the remote matches local.

10. **Mark the pull request ready for review** via `update-pull-request` (confirmation-gated). This starts CI and notifies people, which is why it is a deliberate act and not a side effect.

11. **Move the state** to what the mapping names for review (`transition-work-item`, confirmation-gated).

12. **Harden the process if this stage went red.** A failed pipeline, a check that should have caught something earlier, a step reported done that was not: each becomes one durable guard via `harden-process`. The loop improves while it runs, not at some distant retrospective.

## The Done gate

**Both lanes, or it has not passed.** A green deterministic lane is half the gate, not the gate. `verify-done` never marks a judgement item satisfied on its own: an item is in that lane precisely because someone decided a person must look.

After it: a **human** reviews the change. Stop here, and name the **integrate** capability as what follows once that review passes.

## Guardrails

- **Never implement an unapproved plan.**
- **Never open a second pull request.** One branch, one pull request, one item.
- **Never report a subset of checks as green for the whole.** A chained command stops at the first failure: the later checks never ran, and their silence is not a pass.
- **`unrunnable` is never `passing`.** A check this machine cannot run leaves its Definition of Done item unmet, by name.
- **Never soften the gate to unblock the change.** Widening it is not passing it.
- **Never approve your own work.** The review after this gate is a human's.
- **Never invoke the next stage.** This stage ends at its gate and names what follows.
