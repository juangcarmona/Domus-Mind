---
name: collect-usage
description: "Plain Concepts Lifecycle: summarise human-versus-agent effort for a work item from captured agent telemetry (active time, cost and tokens), reporting missing or truncated data honestly. Read-only. Use when a review entry needs measured effort figures rather than a guess."
metadata:
  plugin: pc-lifecycle
  author: "Juan García Carmona"
---

One responsibility: turn captured telemetry into a compact effort summary for one work item (**human active time, agent active time, cost, tokens**), and say plainly where the figures came from and what is missing.

**Read-only.** This skill never writes a file, never starts a collector, and never touches the review log. The caller passes the figures on.

## Steps

1. **Locate the telemetry.** Read the project's own metrics documentation for where captured data lives and how sessions are mapped to work items. No such configuration → report *effort unavailable: this project captures no agent telemetry* and stop. That is a complete, correct answer.
   - Done when: the data files and the mapping are known, or their absence is established.

2. **Resolve the item's sessions** through the project's session-to-item mapping. Prefer a shared, tolerant reader where the project provides one: a malformed record is repaired or skipped and **reported as skipped**, never silently dropped and never fatal.
   - Done when: the set of session identifiers is in hand, possibly empty. Empty is a result, not an error.

3. **Join on the session identifier, and only on it.** Do not attribute effort by process-level or resource-level attributes: they are read once at process start and are unreliable across hosts and clients. A join on the wrong key produces confident, wrong numbers.
   - Done when: only datapoints carrying a resolved session identifier are included.

4. **Aggregate**: cost, tokens by type, agent active time, and human active time. State how human time was derived (if it is an estimate from idle-capped turn gaps rather than a directly recorded figure, say so and give the cap. Where two sources overlap (a shared summary and a local join), report their **union** and say so), one may hold other people's sessions, the other only this machine's.
   - Done when: every figure has a value or is marked unavailable.

5. **Report, gaps included.** The item, sessions covered, the window, and every figure with its source. Relay every gap verbatim: missing telemetry, a cross-check mismatch, a repaired or skipped mapping line, a session mapped but carrying no usage records.
   - Done when: the caller has the figures and knows exactly what they do not cover.

## Rules

- **Read-only. Always.**
- **A mapped session with no usage records is lost, not zero.** The difference matters: zero is a measurement, lost is a gap.
- **A figure with no source signal is `unavailable`, never `0`.**
- **Never round an absent figure up to a plausible one.** Every summary built on these numbers inherits the lie, and nobody can tell afterwards which figures were measured.
- **No gaps → add no gap note.** An honesty signal that fires every time stops being read.
- **No personal identity beyond what the review entry needs.**

## Do not

- Read from an in-memory dashboard as the source of truth: it loses data on restart.
- Write the review entry. That is a separate responsibility, and it takes these figures as input.
- Re-derive a join the project already implements in a script; report that script's output so a shared summary and a local run can never disagree.
