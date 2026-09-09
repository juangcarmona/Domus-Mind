---
name: adopt
description: "Lifecycle: scaffold a repository for the lifecycle: discover how the team already works, derive their Definition of Ready and Definition of Done, and install only the role capabilities the project needs. Use when the operator asks to adopt, set up or onboard the lifecycle in a repository, or to re-adopt after the project's tooling or its definitions changed."
metadata:
  plugin: pc-lifecycle
  author: "Juan García Carmona"
---

Not a stage. Runs once per repository, and again whenever the project's tooling or its definitions change.

Adopt **discovers and adapts**. It never imposes a methodology, never assumes a tracker, and never starts from nothing.

**Input**: none required.

Invoke deliberately. Nothing in the lifecycle runs adoption on its own.

## Steps

### 1. Discover before asking

Resolve from the repository and its tooling everything that can be resolved, and bring findings as **confirmations** rather than blank questions:

- the tracker, the forge, the CI system;
- the test frameworks, analyzers, formatters and scanners the pipeline already runs: this is most of the deterministic half of the Definition of Done, and it is discoverable without asking anyone;
- whether a specification tool is present;
- an existing `.apm/skills/` directory, and any already-projected harness skill directory (`.agents/skills/`, `.claude/skills/`) from a previous run;
- an existing `docs/engineering-lifecycle.md` from a previous run.

*Done when:* every discoverable fact is in hand, and the human has seen the list.

### 2. Derive the Definition of Ready

Walk [`assets/definition-of-ready.md`](assets/definition-of-ready.md) with the team: **one concrete question per dimension**, about their actual work items, with the reference on the table. Never *"do you have a Definition of Ready?"*; that question produces a shrug.

Each answer produces **keep**, **adapt**, or **drop with a recorded reason**.

Then the question no reference can answer:

> What else must be true before your team is comfortable saying: *"this item is ready to be planned"*?

*Done when:* every dimension has a disposition and the team's own additions are captured.

### 3. Derive the Definition of Done

Walk [`assets/definition-of-done.md`](assets/definition-of-done.md) the same way, leading with what step 1 found in CI. For every dimension the team keeps, settle two further things:

- its **lane**: deterministic, judgement, or both;
- for judgement items, the **role** that decides.

Push against both failure modes. Everything in the judgement lane means the loop automates nothing and the definition is theatre. Everything in the deterministic lane means nobody ever looks at the change, and *human review* is never satisfiable mechanically.

Then the closing question, again:

> What else must be true before your team is comfortable saying: *"this change is done"*?

*Done when:* every dimension has a disposition, a lane, and, where it needs one, a named role.

### 4. Map the lifecycle onto the project's states

The stage-to-state mapping, the transitions each stage fires, the fields each transition's screen requires, whether the team caps work in progress, whether they commit to sprints.

**Every identifier is resolved from live tooling in this session.** A transition id transcribed from a diagram or from memory breaks the loop a month later, and the failure will look like something else entirely.

*Done when:* every value in the mapping was verified against the real system.

### 5. Install the roles the project actually needs

For each required role (`read-work-item`, `transition-work-item`, `open-pull-request`, `inspect-ci-result`, `merge-pull-request`), and each detected role the team's practice calls for:

1. Take the template from [`assets/role-skills/<role>/`](assets/role-skills).
2. Pick the reference matching this project's tooling.
3. Adapt it, replacing every placeholder with a verified value.
4. Install it as `.apm/skills/<role>/SKILL.md` in the adopting repository: renamed from `SKILL.template.md`.

`.apm/skills/` is the **provider-neutral source of truth** for the roles this project adopts. It is not a harness directory. See step 6 for projecting it into the harnesses the team actually runs.

Where **no reference matches**, author against the contract using `writing-great-skills`, and **record that it was authored, not adapted**. A fabricated reference is worse than a missing one, because everything downstream trusts it.

**Install only what is needed.** A team without sprints gets no sprint role and no mention of one. *Agile does not imply Scrum.*

Where a specification tool is present and generates its own skills under the same names, run its initialiser and **do not install the plan-artifact templates at all**: a vendored copy would drift from what the tool produces and collide with it.

*Done when:* every installed role has a verified implementation, and every uninstalled role has a reason.

### 6. Project the roles into the project's harnesses

The adapted role skills are source. Each harness reads them from its own directory, so the source has to be projected there.

Establish which harnesses the team actually runs, then:

- **Where the project uses APM**: record the harnesses in the adopting repository's `apm.yml` (`targets:`, plus `includes:` covering `.apm/skills`) and project with `apm install`. APM writes each harness's own layout: `.agents/skills/<role>/SKILL.md` for Copilot, Codex, Cursor, OpenCode and Gemini, `.claude/skills/<role>/SKILL.md` for Claude Code.
- **Where it does not**: copy each adapted `SKILL.md` into the harness directories the team runs, and **say plainly that these copies are generated** and that `.apm/skills/` remains the file to edit. `.agents/skills/` is read natively by Copilot, Codex, Cursor, OpenCode and Gemini; Claude Code reads `.claude/skills/`.

Whether the projected copies are committed is the **project's** decision, and it follows how that project already treats generated files. Record the decision in the configuration; do not impose one.

*Done when:* every installed role is reachable from every harness the team runs, and the team knows which files are source and which are projections.

### 7. Write the configuration

Fill [`assets/engineering-lifecycle.md`](assets/engineering-lifecycle.md) and write it to `docs/engineering-lifecycle.md`. Show it and confirm before writing.

Its section headings are a **contract**: every lifecycle capability and skill locates what it needs by heading. Headings are never renamed or removed, even when a section is empty.

`docs/engineering-lifecycle.md` is the adopted lifecycle contract. It is **not** `CLAUDE.md`, `AGENTS.md`, `GEMINI.md` or a Copilot instructions file. Those are harness context files; a harness may point *at* the configuration, but never replaces it.

### 8. Report

What was installed, what was detected and declined and why, what needs doing by hand (a board's own column limits, for instance, which no tooling reaches), which files are generated projections, and the first suggested run: the **refine** capability on a real item.

## Idempotence

A second run reconciles; it never overwrites blind.

- An existing configuration is updated **section by section against a shown diff**.
- An existing role skill is compared and the difference shown before anything is replaced.
- A role installed by a previous run that the project no longer needs is reported, not silently deleted.

## Guardrails

- **Never impose a universal checklist.** The references are material to walk, not a standard to comply with.
- **Never write a workflow value that was not verified in this session.**
- **Never fire a transition, and never touch the knowledge base.** Adoption configures; it does not operate the loop.
- **Every file write is shown and confirmed.**
- **Never install a role the project has no use for.** Every unnecessary skill is permanent context cost and one more thing to keep true.
- **Never write an adapted role skill only into a harness directory.** `.apm/skills/` is the source; a harness copy with no source behind it is lost the next time anything is projected.
- **Never invoke the next stage.** Adoption ends by naming what follows.
