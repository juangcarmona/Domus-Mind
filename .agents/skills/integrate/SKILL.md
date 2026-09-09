---
name: integrate
description: "Plain Concepts Lifecycle stage 4: land an approved change: verify the Done gate passed, fold in the specifications, archive the plan, merge, and close the loop in the tracker. Use when the operator asks to integrate, land or merge an approved work item and close it out."
metadata:
  plugin: pc-lifecycle
  author: "Juan García Carmona"
---

Stage 4. An approved change → **merged**, with its specifications and its record landing alongside it.

**Input**: a work item identifier. Required.

Invoke deliberately. No lifecycle stage runs this one on its own.

## Steps

1. **Load the configuration** (absent → stop and say *"run the lifecycle's **adopt** capability first"*).

2. **Refuse a change whose Done gate has not passed.** Run `verify-done` and require every applicable item satisfied: **both lanes**, with the judgement items decided by the roles the configuration names. Unmet → stop, list what remains, and point at the **implement** capability.

   This is the Definition of Done's enforcement point. Everything before it is preparation; this is where it means something.

3. **Check the review.** `inspect-pull-request`: approved, no unresolved blocking threads, not a draft.

4. **Bring the branch up to date with the target** via `rebase-safely`, so the specifications fold into current ones rather than stale ones. This is what keeps sequential merges from conflicting.

5. **Fold the specifications** via `sync-specs`, on the branch.

6. **Archive the plan artifact** via `archive-change`, on the branch, **after** the fold, never before.

7. **Push** the branch (confirmation-gated), so the pull request carries the complete final state.

8. **Re-check immediately before merging.** Reviewer approval can be reset by a push, and steps 4 through 7 pushed. Read the pull request's state again; a vote read ten minutes ago is not evidence now.

9. **Merge** via `merge-pull-request` (confirmation-gated). Set the commit message **explicitly**: the subject describes the *implemented change*, not the proposal the pull request opened with, and no auto-generated body is allowed to carry anything unintended onto the target branch.

10. **Verify traceability**: the branch, the commits, the pull request and the artifacts all carry the work item identifier, and the item links to the merge.

11. **Close the loop in the tracker**: `transition-work-item` to the state the mapping names, and `comment-work-item` with the merge link, both confirmation-gated.

12. **Publish documentation** where a pair is repository-ahead: `sync-doc-to-kb`, confirmation-gated. A pair reported **diverged** is refused: both sides changed, and either write destroys work.

13. **Offer the review now.** Draft the review entry from what just happened (the effort figures via `collect-usage`, what worked, what did not, what to change), and offer it while the human is still here. Declining is one word.

    This is the moment attention exists. It never blocks the merge, which has already happened.

    Offering is not running. The **review** capability runs when a human accepts.

## Guardrails

- **The Done gate is a hard precondition.** Never merge a change that has not passed it, whatever the pull request's review state says.
- **Never merge a draft.**
- **Re-check approval immediately before merging**, not at the start.
- **Set the merge message explicitly.** Never let the forge auto-generate a body.
- **Bring the branch up to date before folding specifications**, always.
- **Archive after folding**, never before.
- **Every external write is confirmed.** The merge most of all.
