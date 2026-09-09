---
name: status
description: "Lifecycle: report where a work item sits in the lifecycle, which capability comes next, and what has drifted. Read-only: mutates no state, comment or branch. Use when the operator asks where an item stands, what to do next with it, whether its Done gate has passed, or what lifecycle drift exists."
metadata:
  plugin: pc-lifecycle
  author: "Juan García Carmona"
---

Read-only. Diagnose a work item's position in the lifecycle and name the next capability. Mutates nothing, no state, no comment, no branch.

**Input**: a work item identifier. If omitted, infer it from the current branch; if that fails, ask.

## Steps

1. **Load the configuration.** Read `docs/engineering-lifecycle.md`. Absent → report *"This repository has no lifecycle configuration. Run the lifecycle's **adopt** capability first."* and stop.
2. **Read the item** via `read-work-item`, and its branch and pull request where they exist.
3. **Place it** against the configuration's stage-to-state mapping, and report the next capability:

   | Where it is | What comes next |
   | --- | --- |
   | Not yet refined | **refine** |
   | Refined, Ready gate unmet | **refine** again: the named dimensions are still open |
   | Ready | **plan** |
   | Planned, awaiting review | a human reviews the plan in the draft pull request |
   | Plan approved | **implement** |
   | In progress | **implement** finishes it out to the Done gate |
   | Done gate unmet | the unmet items, each with its lane and owning role |
   | Done, awaiting review | a human reviews the change |
   | Approved | **integrate** |
   | Merged | **review** |

4. **Report the Done gate's detail** where the item has reached implementation: run `verify-done` in report-only mode and list the unmet items with their lanes and roles. "Not done" is only useful with the list attached.
5. **Report review debt**: merged items with no entry in the review log. Naming it is the whole job here: this command never chases anyone.
6. **Flag drift**, where the item's recorded state and reality disagree: an open pull request against an item still marked ready, a merged pull request against an item still in progress, a branch with no item, an item in progress with no branch.

## Guardrails

- **Never change anything.** No transition, no comment, no label, no branch, no push. A read-only capability that writes once is a read-only capability nobody trusts again.
- **Report what is, not what should be.** Drift is a finding, not something to quietly correct.
- **Name the next capability explicitly.** This is the discoverability surface for a seven-stage loop; a status that leaves the reader guessing has failed.
- **Never invoke the capability it names.** Reporting the next step is the job; taking it is the operator's.
