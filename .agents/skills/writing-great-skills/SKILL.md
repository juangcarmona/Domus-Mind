---
name: writing-great-skills
description: "Lifecycle: author and review Agent Skills that stay valid, portable and predictable, against the Agent Skills specification and this package's own rules. Use when writing a new skill, reviewing or refactoring an existing one, adapting a role template into a project's own skill during adoption, or turning a delivery failure into a durable guard."
metadata:
  plugin: pc-lifecycle
  author: "Juan García Carmona"
---

A skill is procedural knowledge an agent loads on demand. It earns its place when it makes the agent do the same thing on a bad day as on a good one.

Terms in **bold** are defined in [`references/GLOSSARY.md`](references/GLOSSARY.md).

## 1. Satisfy the specification first

These are hard constraints from the [Agent Skills specification](https://agentskills.io/specification), not preferences. A skill that breaks one is invalid, and some harnesses will silently skip it rather than complain.

| Field | Rule |
| --- | --- |
| `name` | Required. 1 to 64 characters, lowercase letters, digits and hyphens only. No leading, trailing or doubled hyphen. **Must match the directory name.** |
| `description` | Required, non-empty, at most 1024 characters. |
| `license` | Optional. A licence name, or the name of a bundled licence file. |
| `compatibility` | Optional, at most 500 characters. Only if the skill has real environment requirements. |
| `metadata` | Optional. A map of string keys to string values, nothing else. |
| `allowed-tools` | Optional, experimental. Support varies between harnesses. |

Nothing else belongs in the frontmatter. A key a single harness happens to accept is not portable, and the next harness may reject the whole file over it.

The layout is equally fixed: `SKILL.md` at the root of the skill directory, with `scripts/`, `references/` and `assets/` as the conventional homes for anything else.

Run `python scripts/validate.py` from the package root. It checks every rule in this section, so do not check them by eye.

## 2. Write the description for discovery, not for a reader

Harnesses load only the `name` and `description` at startup and use them to decide whether the skill is relevant. The body is read only after that decision. So the description is not a summary; it is the **trigger surface**, and it is the only part that is always paying for itself.

Write it in two halves:

1. **What the skill does**, in one clause.
2. **When to reach for it**, naming the situations in the words a person would actually use.

```yaml
description: "Lifecycle: run the checks CI will run, locally, deriving them from the pipeline definitions instead of memory. Use before committing or pushing, before claiming a change is green, before opening or un-drafting a pull request, or after fixing anything a linter, analyzer or test reported."
```

- **Name the situations, not synonyms for the skill.** "Use when reviewing, checking, looking at or examining code" is one situation written four times. "Use before pushing, before un-drafting a pull request, or after a linter reported something" is three.
- **Do not restate the body.** Anything that does not help the agent decide *whether to open the file* is dead weight in every session.
- **Prefix consistently.** Every skill here opens with `Lifecycle:` so a reader can see at a glance which package a skill came from.

## 3. Keep the body short, and push detail down

The specification recommends a `SKILL.md` body under 500 lines and roughly 5000 tokens, and expects deeper material to sit in separate files that are read only when needed. That layering is **progressive disclosure**, and it is the main tool for keeping a skill affordable.

Three places to put a thing, in order of how urgently the agent needs it:

1. **A step in `SKILL.md`**, for what the agent does every run.
2. **A short table or list in `SKILL.md`**, for a rule it consults while running.
3. **A file under `references/`**, for material only some runs need. Link it with a relative path and say what it contains, so the agent can judge whether to open it.

`verify-like-ci` is the worked example: the procedure is in the body, and the two CI systems it knows about are in `references/azure-pipelines.md` and `references/github-actions.md`, read only when that stack is the one in front of it.

## 4. Give every step a finish line

A step that does not say when it is done invites the agent to declare it done early. End each one with a condition that can actually be checked:

- **Weak:** "Review the affected files."
- **Better:** "*Done when:* every file the diff touches has been opened, and anything unexpected is named."

Prefer conditions that are exhaustive where exhaustiveness is the point. "Every installed role has a verified implementation" is checkable; "roles are configured" is not.

## 5. One skill, one responsibility

If the one-line summary needs an "and", it is two skills. `read-work-item` reads; `update-work-item` writes; `transition-work-item` moves state. Splitting them is what lets a project swap Jira for GitHub Issues one capability at a time.

The counter-pressure is real: more skills means more descriptions loaded at startup. Split when the parts are genuinely used apart, not on principle.

## 6. Rules for skills in this package

Every skill here, and every role capability adoption installs into a project, must:

- **Do one thing**, as above.
- **Never invoke another skill's command or prompt.** A skill may name a capability it depends on (`verify-done` calls for `verify-like-ci`), but must never tell the agent to run a `/slash` command. On a harness where that command exists, it invokes this same skill, and the skill calls itself.
- **Be portable.** Plain Markdown, spec-legal frontmatter, no assumption about which harness is running. Do not reference `.claude/`, `.github/` or any other harness directory as though it were the source of truth. The source is `.apm/skills/`; everything else is a projection.
- **Be secure.** No secrets, no credentials. Confirm before any write to shared state: a work item, a pull request, a merge, a published page.
- **Keep the team's answers out.** A project's own definitions live in its `docs/engineering-lifecycle.md`, never hardcoded here.
- **Preserve the human gates.** Where the lifecycle says a person decides, the skill says so too, and does not offer the agent a way around it.

## 7. Adapting a role template

Adoption turns a template from `adopt/assets/role-skills/` into a project's own skill. Two further rules:

- **The template's contract is the fixed part.** Adapt *how* the role is performed on this project's tooling; never change *what* it guarantees to callers. A workflow calling `read-work-item` must get the same shape back on every stack.
- **Say when there was no reference.** Where no shipped reference matches the tooling, author against the contract and record that it was authored rather than adapted. A fabricated reference is worse than a missing one, because everything downstream trusts it.

## 8. Reviewing an existing skill

Read it against this list, in order:

1. Does `python scripts/validate.py` pass?
2. Would the description make you open this file, and not a different one?
3. Is anything in the body needed only sometimes? Move it to `references/`.
4. Does any step lack a finish line?
5. Is any instruction one the agent would follow anyway? Delete the sentence rather than rewording it.
6. Is any rule stated in two places? Keep the one nearer the point of use.
7. Does it name a harness directory, a slash command, or a tool the lifecycle does not require?
8. Has a real failure happened that this skill should have prevented? That is `harden-process`, and the guard belongs here or in a deterministic check, not in a reminder.
