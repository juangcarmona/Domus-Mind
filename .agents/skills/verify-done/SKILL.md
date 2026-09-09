---
name: verify-done
description: "Plain Concepts Lifecycle: assess a change against the project's Definition of Done: run the deterministic lane, collect its evidence, and present the judgement items to the roles that own them. Use at the Done gate, before marking a pull request ready for review, or when deciding whether a change may proceed to integration."
metadata:
  plugin: pc-lifecycle
  author: "Juan García Carmona"
---

One responsibility: produce a verdict on whether a change satisfies **this project's** Definition of Done, item by item, with each item's evidence or its owner attached.

The Done gate asks *is the resulting change sufficiently verified?* It has two lanes, and this skill drives both, but settles only one of them.

## Load the definition first

Read `docs/engineering-lifecycle.md`, section `## Definition of Done`, including its *Additional to this project* table and the *Deterministic checks* table. Each item carries a **lane** (`deterministic`, `judgement`, or `both`) and, where it is judgement, an **owning role** resolved through the `## Roles` section.

**The file is absent** → stop and report: *"This repository has no lifecycle configuration. Run the lifecycle's `adopt` capability first."* Never substitute a generic checklist; a Definition of Done nobody agreed to is not one.

- Done when: every item, its lane, and every judgement item's role are in hand.

## The deterministic lane

Items an agent or CI settles mechanically, producing **evidence**.

1. **Run the checks the pipeline runs**, via `verify-like-ci`, which derives them from the pipeline definitions rather than from this file. The configuration's *Deterministic checks* table records what the team **expects** to exist, so compare the two and report a check the team expects that the pipeline no longer runs. A silently disappeared gate is a finding.
   - Done when: every check has a terminal exit code and the two lists agree, or the difference is reported.
2. **Capture runtime evidence** where the definition's evidence dimension asks for it, via `verify-runtime`.
   - Done when: the artifacts exist and their paths are recorded.
3. **Map results onto items.** Each deterministic item is `satisfied` with its evidence, `failed` with what failed, or `unprovable` with the reason.
   - Done when: no deterministic item is left without one of those three.

**`unprovable` is never `satisfied`.** A check that could not run on this machine leaves its item unmet and says so by name.

## The judgement lane

Items a named human role decides, producing a **decision**.

4. **Assemble the case for each judgement item**: what the item asks, the evidence bearing on it, and what the deterministic lane could and could not answer. A reviewer should not have to reconstruct the change to decide.
   - Done when: each judgement item has its case written.
5. **Route each item to its owning role**, named from the configuration. Where a single role owns several items, present them together.
   - Done when: every judgement item names who must decide it.
6. **Record decisions as they arrive**: satisfied, not satisfied with what is missing, or waived with a reason and by whom.
   - Done when: no judgement item is silently absent from the report.

**Never mark a judgement item satisfied on your own.** Not on the strength of evidence that looks convincing, not because the deterministic lane was green, not because it seems obviously fine. An item is in the judgement lane precisely because someone decided a person must look.

## The verdict

7. **Report the full table** (item, lane, status, evidence or owner), followed by a single verdict:
   - **Done**: every applicable item satisfied. Items marked `N/A` are listed with their reasons, so the verdict is auditable.
   - **Not done**: every unmet item named, each with what specifically is missing and who or what would close it.
   - Done when: the caller can act on the verdict without re-deriving anything.

A *not done* verdict blocks integration; the integration stage refuses a change whose Done gate has not passed. It is not a judgement about the work or the person: it is a list of what remains.

## Rules

- **Both lanes, every time.** A green deterministic lane is not the gate; it is half of it.
- **The configuration is the only rubric.** Never add an item the team did not adopt, never skip one they did.
- **Evidence is linked, not asserted.** "Tests pass" without a run is not evidence.
- **An item with no lane is a configuration defect**: report it rather than guessing which lane it belongs to.
- **A judgement item whose role is unresolvable** is reported unmet, naming the gap in the configuration's `## Roles` section.

## Do not

- Run the deterministic checks by re-deriving them here; that is `verify-like-ci`'s single responsibility.
- Merge, transition, comment, or mark a pull request ready. This skill produces a verdict; the command acts on it.
- Soften a verdict to unblock a change. Widening the gate is not passing it.
