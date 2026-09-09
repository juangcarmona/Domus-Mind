---
name: refine-to-ready
description: "Lifecycle: take a work item to the project's Definition of Ready: interrogate a raw idea into a ready draft, or evaluate an existing item and name what is missing. Use when refining a backlog item before planning, when checking whether an item is ready to be picked up, or when a Ready gate needs a verdict."
metadata:
  plugin: pc-lifecycle
  author: "Juan García Carmona"
---

One responsibility: judge a work item against **this project's** Definition of Ready, and close the gaps or name them. The Ready gate asks *is the problem sufficiently defined?*, never *is the solution designed?*

Readiness has no build to run and no suite to go green. It is a **semantic** judgement about a piece of writing, which is why an agent can make it: read the item, read the definition, decide dimension by dimension, and escalate what you cannot settle.

## Load the definition first

Read `docs/engineering-lifecycle.md` in the repository, section `## Definition of Ready`. That section, including its *Additional to this project* table, is the **only** rubric. Do not carry one here, do not fall back to a generic checklist, and do not import another project's answers.

**The file is absent** → stop and report: *"This repository has no lifecycle configuration. Run the lifecycle's `adopt` capability first."* Never guess a team's definition.

- Done when: every dimension the project requires is in hand, with its wording.

## Mode: raw input

An idea, a paragraph, a document section: anything not yet a work item.

1. **Read the source and its context.** Follow any path or anchor given. Read what the definition points at (product rules, glossary, prior decisions, specifications), wherever this project records them.
   - Done when: the source and every referenced context document are in hand.
2. **Draft what the source already answers.** Fill every dimension you can from what you have read, and mark the rest as gaps. Lead with a draft; never open with a blank interrogation.
   - Done when: each dimension is answered, marked as a gap, or marked `N/A` with a reason.
3. **Close the gaps by asking.** One cluster of questions at a time, blocking ones first. Stop asking about a dimension the moment it is satisfied.
   - Done when: no gap remains that the human can close.
4. **Author the acceptance criteria.** Writing clear, verifiable criteria is part of refining, not a precondition for it and not a promise deferred to a later stage. Draft them, confirm the intent, record them.
   - Done when: the criteria are observable, verifiable, and confirmed.
5. **Surface blocking unknowns instead of assuming.** A dimension that depends on an unresolved decision is recorded as blocked on that decision, never filled in with a plausible answer.
   - Done when: no dimension silently assumes something nobody decided.
6. **Emit the draft.** Every dimension the project requires, in its wording, with `N/A` and a reason where inapplicable.
   - Done when: the draft is handed back for the caller to record. This skill does not create or update the work item.

## Mode: existing work item

The caller supplies the item's current content. Evaluate; do not interrogate.

1. **Judge every dimension** against what is actually written. Missing, vague and contradictory all count as unmet: a dimension that requires a rereading to find is not met.
   - Done when: every dimension is judged met, unmet, or `N/A` with a reason.
2. **Return a verdict.**
   - **Ready**: every applicable dimension met. Say which were judged `N/A` and why, so the judgement is auditable.
   - **Not ready**: name each unmet dimension and *what specifically is missing*, concrete enough for the caller to post verbatim. Never a bare refusal.
   - Done when: the caller has a verdict it can act on without re-reading the item.

## Escalate, do not block

A verdict of *not ready* is an **alert to a human**, not a veto. Report the named gaps and let the human decide whether to close them, override the gate, or reshape the item. Automating this check keeps items with undefined problems from reaching planning by accident; it does not remove the person from the decision.

## Do not

- **Design the solution.** Architecture, approach and task breakdown belong to the planning stage. A Definition of Ready that demands a design is one no item ever passes.
- **Write the test plan.** Identifying affected scenarios is a Ready dimension; the plan itself is a planning output.
- **Create, update or transition the work item**, post comments, or move state. Those are separate roles, composed by the command.
- **Duplicate the definition here.** It lives in `docs/engineering-lifecycle.md` and changes without touching this skill.
- **Invent acceptance criteria the human did not confirm.**
