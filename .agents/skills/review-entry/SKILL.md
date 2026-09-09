---
name: review-entry
description: "Lifecycle: append one dated entry to the project's review log after a change has merged, recording what worked, what did not, and what to change next time, alongside the effort figures the caller supplies. Use at loop close-out, when recording a retrospective for a completed work item, or when a post-merge finding needs capturing."
metadata:
  plugin: pc-lifecycle
  author: "Juan García Carmona"
---

One responsibility: append **exactly one** dated entry to the project's review log. Append-only: the log is a record, and a record that gets rewritten is not one.

**This skill does not compute effort figures.** The caller runs `collect-usage` and passes the numbers in. Nothing here starts a collector, reads telemetry, or re-derives a figure.

## Input

- The work item's identifier and a short title.
- Date (`YYYY-MM-DD`, defaults to today).
- The reflections: **what worked**, **what did not**, **what to change next time**.
- Effort figures **and their source**: human time, agent time, cost, tokens, and where they came from, or an explicit statement that telemetry was missing.
- The log path. Default `docs/review-log.md`; the project's lifecycle configuration overrides it when the team keeps one elsewhere.

## Steps

1. **Read the log** to find where it ends and to match the entry format its own header documents. Do not parse or modify existing entries: read only far enough to write the next one correctly.
   - Done when: the file's format and its last entry are in hand.
2. **Compose one entry**: a dated heading carrying the item identifier and title, then the three reflections, the effort figures, and the amendment line.
   - Done when: every field is filled or explicitly marked absent.
3. **Name the effort source.** The figures line says where the numbers came from, so a reader knows what they cover. Telemetry missing → write `not captured`. **Never invent a number to fill the line**, and never round an absent figure up to a plausible one.
   - Done when: every figure carries a source, or is marked `not captured`.
4. **Route what graduates.** A reflection that is a recurring, structural change to how the team works does not belong buried in prose:
   - a decision with lasting trade-offs → note it as an ADR candidate and put the resulting record's identifier on the entry's amendment line;
   - a missing guard the process should have had → note it for hardening;
   - a gap in what the team calls ready or done → note it as a proposed amendment to their Definition of Ready or Definition of Done.
   - Done when: each structural reflection names its destination, or the amendment line reads `none`.
5. **Append, and only append.** The entry becomes the **last** block in the file. Every existing byte above it is untouched, no edits, no reordering, no deletions.
   - Done when: the file's prior content is byte-identical and the new entry is last.
6. **Report** the appended entry and the file path to the caller.

## Rules

- **Append-only.** Editing, reordering or deleting a prior entry is forbidden. A correction is a new, later entry that says what it corrects.
- **One entry per invocation.** Never batch several work items into one call.
- **Figures come from the caller.** Do not reach for telemetry from here.
- **Honest absence beats a plausible number.** `not captured` is a real answer; an invented figure corrupts every summary built on the log afterwards.
- **Structural lessons graduate.** A recurring change to how the team works belongs in a decision record or an amendment to their definitions: linked from the entry, not visible only to whoever reads the log.

## Do not

- Call another skill. Composition happens in the command.
- Propose the amendment itself: this skill notes that one is warranted and names where it goes. The review stage proposes it, a human confirms it.
