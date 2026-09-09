# Engineering lifecycle

**Project**: DomusMind · **Adopted**: 2026-09-08 · **Last amended**: 2026-09-08

## Definition of Ready

Governs the **Ready** gate at the end of the `refine` capability: *is the problem sufficiently defined?* Verified semantically by an agent, which alerts a human when a dimension is missing, vague or contradictory rather than refusing outright.

An item is ready when every applicable dimension is present. Inapplicable ones are marked `N/A` with a one-line reason.

| # | Dimension | This project requires |
| --- | --- | --- |
| 1 | Title | Kept — states the desired outcome, not the activity |
| 2 | Description | Kept — explains the intent, problem and relevant context |
| 3 | Actor / stakeholder | Kept — identifies who needs or is affected by the change |
| 4 | Priority | Dropped — no formal prioritisation; items are picked up by need |
| 5 | Dependencies | Kept — must be explicitly listed, including "none" |
| 6 | Acceptance criteria | Kept — observable and verifiable conditions for success |
| 7 | Scope | Adapted — explicit out-of-scope statement required when ambiguity exists |
| 8 | Product rules | Kept — recorded in `specs/` and `docs/` |
| 9 | Affected behaviour | Kept — when applicable, identify existing flows the change may alter |
| 10 | Quality expectations | Kept — security is relevant (full SAST adopted); performance, accessibility stated when applicable |
| 11 | Test impact | Adapted — identified during Plan, not required at Ready |
| 12 | Existing decisions | Kept — ADRs (`docs/02_architecture/adrs/`), specs (`specs/`), product docs (`docs/`) |
| 13 | Unknowns / risks | Kept — material uncertainty must be recorded, not necessarily resolved |

### Additional to this project

No additional dimensions. The reference dimensions are sufficient for this project.

## Definition of Done

Governs the **Done** gate at the end of the `implement` capability: *is the resulting change sufficiently verified?* The `integrate` capability refuses to merge a change that has not passed it.

**In this project, "done" means**: finished and proven, ready to integrate. Deployment is a separate act.

Lane is `deterministic` (an agent or CI settles it), `judgement` (a named role decides), or `both`. Judgement items must name a role from the **Roles** section.

| # | Dimension | This project requires | Lane | Role |
| --- | --- | --- | --- | --- |
| 1 | Acceptance criteria | A test per criterion where feasible; demonstrated in review | both | developer |
| 2 | Implementation completeness | No TODOs, no dead code, no silent scope loss; planned tasks finished or dropped with a recorded reason | both | developer |
| 3 | Tests | xunit + vitest suites green; coverage adequate to the change and the risk it carries | both | developer |
| 4 | Quality checks | `dotnet build`, `dotnet test`, `npm run build`, `eslint`, `tsc` all pass | deterministic | — |
| 5 | Security | Secret scan + dependency scan + SAST (CodeQL) pass; exposure judged by reviewer | both | developer |
| 6 | Architecture / design conformance | Change matches the approach approved at the Plan gate; deviations recorded, not silent | judgement | developer |
| 7 | Documentation | User-facing, operational and developer documentation updated wherever behaviour changed | both | developer |
| 8 | Product / spec consistency | Specs and product docs updated when behaviour changed | both | developer |
| 9 | Evidence | CI run linked; test results exist | deterministic | — |
| 10 | Human review | Self-review against every DoD dimension | judgement | developer |
| 11 | Known limitations | What is deliberately not addressed is stated in the pull request | both | developer |

### Additional to this project

No additional dimensions. The reference dimensions are sufficient for this project.

### Deterministic checks

The commands that produce the deterministic lane's evidence. `verify-like-ci` derives these from the project's pipeline configuration rather than from this list; the list records what the team expects to see, so a silent disappearance is noticed.

| Check | How it runs | Evidence it produces |
| --- | --- | --- |
| Backend build | `dotnet build DomusMind.slnx -c Release` | Build succeeds with no errors |
| Backend tests | `dotnet test DomusMind.slnx -c Release` | All xunit tests pass |
| Web app build | `npm run build` in `src/web/app` | `tsc -b && vite build` succeeds |
| Web app lint | `npm run lint` in `src/web/app` | eslint reports no errors |
| Web app tests | `npm run test` in `src/web/app` | vitest suite passes |
| Public site build | `npm run build` in `src/web/public` | Astro build succeeds |
| Secret scanning | GitHub built-in secret scanning | No secrets detected in diff |
| Dependency scanning | Dependabot alerts | No known vulnerable dependencies |
| SAST | CodeQL workflow | No security alerts |

**Note**: Web app lint, web app tests, secret scanning, dependency scanning, and CodeQL SAST are **expected** checks that are not yet in CI. They are recorded here so their absence is noticed. Adding them to CI is a follow-up task.

## Lifecycle stages and states

How the loop's stages map onto this project's work-item states. The lifecycle state is carried as a `status:<state>` label on each GitHub Issue.

| Stage | State on entry | State on exit | Transition | Fired by |
| --- | --- | --- | --- | --- |
| `/refine` | Backlog (no `status:` label) | Refined | add `status:refined` | agent, human-confirmed |
| `/plan` | Refined | Planned | swap `status:refined` → `status:planned` | `/plan` |
| `/implement` | Planned | In Review | swap `status:planned` → `status:in-progress` on start; `status:in-progress` → `status:in-review` on PR open | `/implement` |
| `/integrate` | In Review | Done | swap `status:in-review` → `status:done`; close issue | `/integrate` |
| `/review` | Done | — | — | — |

### Transitions no command ever fires

| Transition | Meaning | Who fires it |
| --- | --- | --- |
| Reopen | Reversing Done back to an earlier state | human only |
| Block | Item is blocked by a dependency or external factor | human only |
| Deprioritise | Item moved back to Backlog from a later state | human only |

## Work item fields

Fields a transition's screen requires. GitHub Issues has no transition screens; the lifecycle state is carried as a `status:<state>` label.

| Field | Identifier | Required by | Notes |
| --- | --- | --- | --- |
| Lifecycle state | `status:<state>` label | every transition | One `status:` label per issue; remove the old label when adding the new one |
| Acceptance criteria | `## Acceptance Criteria` section in issue body | `/refine` (Ready gate) | Markdown heading in the issue body |
| Work item link | `Closes #<number>` in PR body | `/implement` | Placed in the PR body, not the title |

## Work in progress

**WIP cap**: 3 · **Query used at pickup**: `gh issue list --label "status:in-progress" --state open` + `gh issue list --label "status:in-review" --state open` · **Sprint commitment**: Not used · **Board**: GitHub Issues with `status:` labels. Column limits are not set by any tool; the WIP cap is enforced by the `wip-query` role at pickup.

## Roles

Who decides the judgement items above. A role is a person, a team, or a named responsibility, not a job title for its own sake.

| Role | Who | Decides |
| --- | --- | --- |
| developer | Solo developer (Juan García Carmona) | All Definition of Done judgement dimensions: acceptance criteria, implementation completeness, tests adequacy, security exposure, architecture conformance, documentation, spec consistency, human review, known limitations |

## Installed roles

Role skills the `adopt` capability installed into `.apm/skills/`: the provider-neutral source. Re-running adopt reconciles against this list rather than overwriting blind.

Projected harness copies (`.agents/skills/`, `.claude/skills/`, …) are generated from that source; edit the source, never a projection.

| Role skill | Adapted from | Installed | Projected to |
| --- | --- | --- | --- |
| `read-work-item` | `references/github-issues.md` | 2026-09-08 | `.agents/skills/` (via `apm install`) |
| `transition-work-item` | `references/github-issues.md` | 2026-09-08 | `.agents/skills/` (via `apm install`) |
| `open-pull-request` | `references/github.md` | 2026-09-08 | `.agents/skills/` (via `apm install`) |
| `inspect-ci-result` | `references/github.md` | 2026-09-08 | `.agents/skills/` (via `apm install`) |
| `merge-pull-request` | `references/github.md` | 2026-09-08 | `.agents/skills/` (via `apm install`) |
| `wip-query` | `references/github-projects.md` | 2026-09-08 | `.agents/skills/` (via `apm install`) |

### Roles not installed and why

| Role | Reason |
| --- | --- |
| `commit-to-sprint` | No sprints |
| `create-branch` | Not required; implement creates branches directly |
| `comment-work-item` | Solo developer; progress recorded in PR, not issue comments |
| `create-work-item` | Items created manually |
| `update-work-item` | Items updated manually |
| `read-work-item-attachments` | No attachments used |
| `read-work-item-links` | No linking used |
| `read-kb-page` | No specification tool; docs are hand-authored Markdown in-repo |
| `search-kb` | No specification tool |
| `sync-doc-to-kb` | No specification tool |
| `discover-related-docs` | No specification tool |
| `classify-doc-drift` | No specification tool |
| `sync-specs` | No specification tool |
| `propose-change` | No specification tool |
| `apply-change` | No specification tool |
| `archive-change` | No specification tool |
| `inspect-pull-request` | Covered by `open-pull-request` and `merge-pull-request` |
| `update-pull-request` | Covered by `open-pull-request` and `merge-pull-request` |
| `checkout-branch` | Not required; covered by direct git commands |

## Amendment log

The `review` capability proposes changes to this file from delivery evidence. Every amendment is shown as a diff and confirmed by a human; nothing edits this file silently.

| Date | Section | Change | Why |
| --- | --- | --- | --- |
| 2026-09-08 | All | Initial adoption | First run of the `adopt` capability |
