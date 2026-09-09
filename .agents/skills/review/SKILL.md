---
name: review
description: "Lifecycle stage 5: learn from a completed cycle: record what happened with measured effort, and propose amendments to the process itself. Use when the operator asks to review or retrospect a merged work item, close out a cycle, or improve the lifecycle from what just happened. Post-merge only; gates nothing."
metadata:
  plugin: pc-lifecycle
  author: "Juan García Carmona"
---

Stage 5. A merged change → **a better process**.

Post-merge, and deliberately so. This stage sees what no pre-merge retrospective can: the deploy outcome, CI on the target branch, and anything that broke *because* of the merge.

**Input**: a work item identifier. If omitted, offer the merged items with no review entry.

Invoke deliberately. The **integrate** stage offers this stage; it never runs it.

## What this is not

The Done gate asked *is this change done enough to integrate?*, a question about one change. This asks *what should we change about how we work?*, a question about the cycle. They are never merged into one step.

## Steps

1. **Load the configuration** (absent → stop and say *"run the lifecycle's **adopt** capability first"*).

2. **Look at what happened after the merge**: the pipeline on the target branch via `inspect-ci-result`, the deploy outcome where the project has one, and anything reported since. This is the material a pre-merge retrospective cannot have.

3. **Measure the effort** via `collect-usage`. Missing telemetry is reported as missing, never replaced with a plausible figure, because every summary built on the log afterwards inherits the lie.

4. **Draft the reflections** (what worked, what did not, what to start, what to stop), from what actually happened: the commit narrative, the blockers hit, the review findings, the gate that fired or failed to. **Lead with a draft** for the human to correct. A cold "how did it go?" gets a shrug.

5. **Record it** via `review-entry`. Append-only.

6. **Route what graduates.** Each structural lesson goes where it can take effect:

   | Lesson | Destination |
   | --- | --- |
   | A problem was underspecified | An amendment to the **Definition of Ready** |
   | Something shipped that should not have | An amendment to the **Definition of Done**: a new item, or a lane change from judgement to deterministic |
   | A role skill misbehaved | The installed **role skill**, at its source in `.apm/skills/<role>/SKILL.md`, never in a projected harness copy, which the next projection overwrites |
   | A gate fired at the wrong moment | A **gate** change |
   | A decision with lasting trade-offs | `adr` |
   | A missing guard | `harden-process` |

7. **Propose the amendment as a diff.** Any change to `docs/engineering-lifecycle.md` is shown and confirmed by a human before it is written. Nothing edits that file silently: it is the contract the whole loop reads.

8. **Feed the next item.** Report what changed and why, so the next **refine** run starts from a better definition than the last one did.

## The improvement that matters most

A Definition of Done improves mainly by **moving items from the judgement lane to the deterministic lane**: turning something people must remember into something that fails loudly. When a lesson can be expressed that way, prefer it: it is the only kind of process change that keeps working when everybody is busy.

## Guardrails

- **Append-only.** A correction is a new, later entry that says what it corrects.
- **Effort figures come from telemetry, not a guess.** Missing is a valid, honest answer.
- **Never amend the configuration silently.** Diff, then confirm.
- **Never turn a one-off into a permanent guard.** A cost paid on every future run to prevent something that happened once is not worth it. "No change warranted", with the reason, is a legitimate outcome.
- **This stage gates nothing.** It runs after the merge and blocks no one.
