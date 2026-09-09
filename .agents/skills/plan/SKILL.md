---
name: plan
description: "Plain Concepts Lifecycle stage 2: turn a ready work item into a written, reviewable solution design with tasks and test impact, pick it up, and raise a draft pull request carrying the plan. Use when the operator asks to plan a work item, design the solution for a ready item, or produce a plan artifact for review."
metadata:
  plugin: pc-lifecycle
  author: "Juan García Carmona"
---

Stage 2. A ready item → a **designed solution**, written down and open for review. Ends at the Planned gate.

**Input**: a work item identifier. Required.

Invoke deliberately. No lifecycle stage runs this one on its own.

## Steps

1. **Load the configuration** (`docs/engineering-lifecycle.md`; absent → stop and say *"run the lifecycle's **adopt** capability first"*).

2. **Check readiness.** `read-work-item`, then `refine-to-ready` in evaluate mode. Not ready → stop and point at the **refine** capability for this item, naming the unmet dimensions. Never plan around an undefined problem; the design will encode the ambiguity and nobody will see it again until implementation.

3. **Inspect the code.** Identify the affected components, the seams the change runs through, and what it will touch that nobody expects.

4. **Check documentation drift** via `classify-doc-drift` where installed. A plan built on a page that is behind the code is a plan built on fiction. A pair reported **diverged** is a finding for a human, not something to fix here.

5. **Design the solution**: approach, architecture impact, the trade-offs considered and why this one wins. A decision with lasting consequences graduates to an `adr`.

6. **Plan the work**: ordered tasks, each small enough to be verifiably done.

7. **Plan the tests.** What proves this change works: new and affected unit, integration and end-to-end scenarios. This is a first-class output, not an afterthought.

8. **Name the Definition of Done items this change will have to satisfy**, from the configuration. The Done gate should never be the first time anyone reads them, and an item that will be expensive to satisfy is far cheaper to discover now.

9. **Write the plan artifact** via `propose-change`: proposal, design, tasks and test plan, carrying the work item identifier.

10. **Pick the item up** (each external write confirmation-gated):
    - `wip-query` where a cap is configured: at or over it, stop and report which items hold it;
    - `commit-to-sprint` where the project commits to sprints;
    - `transition-work-item` to the in-progress state the mapping names.

11. **Branch and commit**: `create-branch` from the current target, commit the plan artifact, push (confirmation-gated).

12. **Raise the draft pull request** via `open-pull-request` with `draft` set. The draft state **is** the gate: unmergeable by construction, and CI does not run on it. The pull request body carries the plan summary and links the item.

13. **Share it back**: `comment-work-item` with the plan summary and the pull request link, where that role is installed.

## The Planned gate

A **human** gate. A person reads the plan in the pull request thread and answers: is this the right problem, is this the right solution, and do we have the right tests?

Stop here. Name the **implement** capability as what follows once the plan is approved, and do not invoke it.

## Guardrails

- **Never implement.** This stage writes a plan, not code. The only thing committed is the artifact.
- **Never plan an item that is not ready**: point at the **refine** capability instead of a bare refusal.
- **The pull request is a draft on purpose.** Marking it ready is the **implement** stage's act, and it means something.
- **One branch and one pull request per item.** An item that already has one gets that one reused, never a second.
- **The work-in-progress cap is a hard stop**, not a warning, where the project configured one.
- **Every external write is confirmed.**
- **Never invoke the next stage.** This stage ends at its gate and names what follows.
