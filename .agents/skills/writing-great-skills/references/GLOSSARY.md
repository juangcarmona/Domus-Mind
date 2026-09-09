# Glossary

Terms used across `pc-lifecycle`, in [`SKILL.md`](../SKILL.md) and in the lifecycle capabilities. Where a term comes from a specification, the specification is named.

## Skills and packaging

- **Skill**: a directory holding a `SKILL.md` and any supporting files, in the format defined by the [Agent Skills specification](https://agentskills.io/specification). The unit of reusable procedural knowledge, and the only primitive that reaches every harness this package targets.
- **Trigger surface**: the `name` and `description`, which are the only parts loaded at startup. What a harness matches against when deciding whether the skill is relevant.
- **Progressive disclosure**: the Agent Skills term for loading in stages. Metadata at startup, the body once the skill is chosen, `references/` files only when a run needs them.
- **Prompt**: a short entry point under `.apm/prompts/`, giving a capability a convenient invocation on harnesses that have a command primitive. It delegates to a skill and holds no workflow of its own.
- **Capability**: what the lifecycle depends on, as opposed to the tool that implements it. The seven lifecycle capabilities are `adopt`, `refine`, `plan`, `implement`, `integrate`, `review` and `status`.
- **Role capability**: an adopted skill that abstracts one piece of a project's infrastructure, such as `read-work-item` or `open-pull-request`. Its contract is fixed; its implementation is whatever that project already uses.
- **Contract**: the input, output, writes and guarantees a role capability promises its callers. Adaptation may change how a role is performed, never what it guarantees.
- **Projection**: a generated copy of a source primitive in a harness's own directory, such as `.agents/skills/` or `.claude/skills/`. Never edited by hand; the source is `.apm/`.
- **Payload**: material a package carries for a capability to use, rather than for a harness to load. The role templates under `adopt/assets/role-skills/` are payload, which is why they are named `SKILL.template.md`.

## The lifecycle

- **Gate**: a named point where work is judged before it may continue. There are three: Ready, Planned and Done.
- **Ready gate**: at the end of `refine`. Asks whether the *problem* is sufficiently defined. Verified by an agent, which alerts a human on anything it cannot settle.
- **Planned gate**: at the end of `plan`. Asks whether the proposed *solution* is sufficiently designed. Verified by a person reading the plan.
- **Done gate**: at the end of `implement`. Asks whether the resulting *change* is sufficiently verified. Both lanes must pass.
- **Lane**: which kind of judgement a Definition of Done item needs. **Deterministic** items are settled by a check that passes or fails. **Judgement** items are settled by a named person.
- **Definition of Ready**: the project's own contract for the Ready gate, derived during adoption and recorded in `docs/engineering-lifecycle.md`.
- **Definition of Done**: the project's own contract for the Done gate, with a lane and, where needed, an owning role for each item.
- **Configuration**: `docs/engineering-lifecycle.md` in the adopting repository. The single place a project's answers live. Its headings are a contract that consumers locate material by.
- **Drift**: a disagreement between what is recorded and what is true, such as an open pull request against an item still marked ready. Reported by `status`, never silently corrected.
- **Review debt**: a merged item with no entry in the review log. Reported, never blocking.

## Writing

- **Finish line**: the condition that tells the agent a step is genuinely done. Checkable, and exhaustive where exhaustiveness is the point.
- **Responsibility**: the one thing a skill does. A summary needing an "and" describes two skills.
- **Human gate**: a decision the lifecycle reserves for a person. A skill must state it and must not offer a way around it.
- **Guard**: the smallest durable change that stops a specific failure recurring, added by `harden-process`. Best expressed as a deterministic check rather than a reminder.
