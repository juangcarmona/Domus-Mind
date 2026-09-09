---
name: harden-process
description: "Plain Concepts Lifecycle: turn one concrete failure into the smallest durable guard in the process itself. Use after a red pipeline, a review that caught something the process should have, an escaped defect, or any moment a stage was reported done when it was not."
metadata:
  plugin: pc-lifecycle
  author: "Juan García Carmona"
---

One responsibility: convert one observed failure into one durable guard, so the same class of miss cannot pass silently again.

Fixing the instance restores the build. Fixing the **process** is what stops the next one, and the process is expected to improve itself as it runs, not at some distant retrospective.

**Input:** what failed, and the evidence: a pipeline log, a review comment, a defect report, a claim that turned out to be untrue. **Output:** the named process gap, the edit that closes it, and the counterfactual check. Or a reasoned *"no guard warranted"*.

## Steps

1. **State the failure as an observation**, with evidence: what was claimed, what was true, and how the difference surfaced.
   - Done when: the claim and the reality are both written down, separately.
2. **Name the process gap, not the code bug.** Ask what the process *permitted*: a verification that was partial; a step whose completion criterion could be satisfied without doing the work; a requirement nobody ever stated; a check reported as "queued" and accepted as "green".
   - Done when: the gap is a sentence about the process, and the code defect is explicitly named as *not* the gap.
3. **Locate the single source of truth** for that gap: the one skill, command, definition or convention that owns it. The same meaning in two places is duplication, and duplicated copies drift apart.
   - Done when: exactly one file is identified as the owner.
4. **Land the smallest edit that makes the gap checkable.** In order of preference: sharpen an existing completion criterion; add a step; add a new skill only when a genuinely distinct responsibility has appeared. **The edit must be one an agent can fail**: *"verify X exists"* beats *"remember to X"*.
   - Done when: the edit is written, and a run that skipped it would be detectably wrong.
5. **Apply the counterfactual.** Re-read the edited instruction against the original failure and state plainly whether, followed literally, it would have caught it.
   - Done when: the answer is yes. If it is no, the edit is not finished.
6. **Record it where the team will meet it**: the review-log entry for the work item, and a note on the item or the pull request when a reviewer needs to know the process moved.
   - Done when: the guard is discoverable by someone who was not in this session.

## Where a guard may live

| The gap is in | The guard goes in |
| --- | --- |
| How a stage is executed | The command that owns the stage |
| How one responsibility is performed | The skill that owns it |
| What the team calls ready | Their Definition of Ready |
| What the team calls done | Their Definition of Done, often as a lane change |
| A decision with lasting trade-offs | A decision record |

A gap in what "done" means is the most valuable kind to find, because moving an item from the judgement lane to the deterministic lane converts a thing people must remember into a thing that fails loudly.

## Rules

- **One failure, one guard.** Decompose a cluster; each miss gets its own smallest edit, so each can be judged and reverted independently.
- **No no-ops.** A line the agent already obeys by default changes nothing and becomes sediment. If the instruction restates a default, the real gap is elsewhere: keep looking.
- **Never widen a gate to make a failure disappear.** Loosening a rule, excluding a path, or lowering a threshold is not hardening. It is the opposite.
- **A guard that slows every future run to prevent a one-off is not worth it.** Say so and stop. *"No guard warranted"*, with the reason, is a legitimate and common outcome.
- **The guard belongs to the process, never to one item's code.**
