---
name: refine
description: "Plain Concepts Lifecycle stage 1: take a raw idea or an existing work item to the project's Definition of Ready, authoring the acceptance criteria along the way, and end at the Ready gate. Use when the operator asks to refine, groom or shape a backlog item, or to take an idea to ready."
metadata:
  plugin: pc-lifecycle
  author: "Juan García Carmona"
---

Stage 1. Fuzzy input → a work item whose **problem** is sufficiently defined. Ends at the Ready gate.

**Input**: a work item identifier, or a raw idea. Neither given → ask.

Invoke deliberately. No lifecycle stage runs this one on its own.

## Steps

0. **Report review debt.** Name any merged item with no entry in the review log, then **continue anyway**. This is the loop's feedback arrow: observable, never blocking. Nobody is stopped from starting work because a past retrospective is outstanding, but nobody gets to not know either.

1. **Load the configuration.** Read `docs/engineering-lifecycle.md`. Absent → stop and say *"run the lifecycle's **adopt** capability first"*. Never invent a Definition of Ready.

2. **Gather the item's context** (raw idea → skip to 3): `read-work-item`, then `read-work-item-links` and `read-work-item-attachments` where installed. An item whose requirement lives in an attachment is common, and unread attachments are how a stage confidently refines the wrong thing.

3. **Gather documentation context**: `discover-related-docs` from the item's summary and components, then `read-kb-page` on the promising ones. Where those roles are not installed, read what the project's own documentation points at.

4. **Judge against the Definition of Ready** via `refine-to-ready`. It reads the project's adopted definition and returns either a ready verdict or the named unmet dimensions.

5. **Close the gaps.** Ask the blocking questions first, grouped, leading with a draft rather than a blank question. Stop asking about a dimension the moment it is satisfied.

6. **Author the acceptance criteria.** Writing clear, verifiable criteria is **this stage's job**, not a precondition for it, and never a promise deferred to planning. Draft them, confirm the intent, record them.

7. **Record** (confirmation-gated): an existing item → `update-work-item`; a raw idea → `create-work-item`. Show the content before writing.

8. **Move the state** where the configuration's mapping says refinement changes it: `transition-work-item`, confirmation-gated. A configuration that maps no transition to this stage means the item's state does not move here, and that is a valid answer.

## The Ready gate

An **agent-verified** gate. Readiness is a semantic judgement about a piece of writing, so the agent makes it and escalates what it cannot settle: every applicable dimension satisfied → ready; anything missing, vague or contradictory → alert the human, naming the dimensions and what could not be found.

A verdict of *not ready* is an alert, **not a veto**. The human decides whether to close the gaps, override, or reshape the item.

Stop here. Name the **plan** capability as what follows when the verdict is ready, and do not invoke it.

## Guardrails

- **Never design the solution.** Architecture, approach and task breakdown belong to the **plan** stage. A Definition of Ready that demands a design is one no item ever passes.
- **Never grant readiness on criteria promised for later.** Authoring them is this stage's work.
- **Never assume an unresolved decision.** A dimension depending on one is recorded as blocked on it, not filled in with something plausible.
- **Every external write is confirmed**, and the content is shown first.
- **Never fire a transition the configuration marks human-only.** Report finding one; never reverse one.
- **Never invoke the next stage.** This stage ends at its gate and names what follows.
