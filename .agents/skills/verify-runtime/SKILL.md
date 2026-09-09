---
name: verify-runtime
description: "Plain Concepts Lifecycle: drive the real application to observe a change working, and capture the evidence that proves it. Use when a change alters or adds a user-facing surface, when tests pass but nobody has seen the change run, or when the Definition of Done requires runtime evidence."
metadata:
  plugin: pc-lifecycle
  author: "Juan García Carmona"
---

One responsibility: make a change **observable**, and leave behind artifacts a reviewer can inspect without rebuilding anything.

Tests prove the code does what the tests say. They do not prove a person can use the change. A green suite and a broken screen coexist comfortably.

**Output:** the evidence artifacts, their paths, and a plain statement of what was observed: including anything observed that was *not* expected.

## Steps

1. **Establish how this project runs.** Read the project's own instructions: its contributing guide, its run scripts, its end-to-end harness. Where the project has a skill or documented recipe for booting the application, use it; this skill does not invent a boot procedure.
   - Done when: the command that starts the application, and the command that drives it, are both known and came from the project.

2. **Prefer the end-to-end harness over manual driving.** Where one exists it already solves the hard parts: dependencies started, data seeded, a real browser or client attached, artifacts written on every run. Driving by hand is the fallback, not the default.

3. **Build what the runtime actually serves.** A change to a front end that is served from a built bundle is not visible until the bundle is rebuilt. Running against a stale artifact produces evidence of the *previous* change.
   - Done when: what is running was built from the current tree.

4. **Exercise the change's own path.** Reach the surface this change altered and perform the action a user would. Where the change has several branches (an error state, an empty state, a permission denial), exercise the ones the acceptance criteria mention.
   - Done when: every criterion claiming a visible behaviour has been observed.

5. **Capture the evidence.** Screenshots, recordings, traces, response payloads, log excerpts: whatever the project's Definition of Done requires under its evidence dimension. Store them where the project keeps such artifacts and report the paths.
   - Done when: each observation has an artifact a reviewer can open.

6. **Read the evidence you captured.** Open the screenshots. Look at them. Capturing an artifact and never inspecting it is a ritual, not a verification, and the defects this step exists to catch are exactly the ones the tests did not encode.
   - Done when: each artifact has been inspected and what it shows is stated.

7. **Report what was observed**, including anything unexpected. A layout that is wrong, a label that is stale, a flash of the wrong state: these are findings, not noise, and they are cheapest to fix now.

## Rules

- **Evidence is what a reviewer can open**, not a claim in a summary. "Verified manually" is not evidence.
- **An artifact nobody looked at proves nothing.** Step 6 is not optional; it is the step that finds defects.
- **Never present evidence from a stale build** as evidence of this change.
- **Report what you saw, not what should have happened.** If the observation contradicts the acceptance criteria, that is the finding.
- **A surface that cannot be driven on this machine** is reported as unobserved, by name, and the Definition of Done's evidence dimension stays unmet, never quietly satisfied.

## Do not

- Substitute a passing test suite for observing the change.
- Boot infrastructure the project does not use, or invent a run procedure.
- Leave credentials, tokens or personal data inside a captured artifact.
