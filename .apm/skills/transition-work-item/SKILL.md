---
name: transition-work-item
description: "Apply exactly one lifecycle state transition to one GitHub Issue by swapping its status label. Use when a stage advances an item."
---

One responsibility: move one work item through exactly one state transition.

## Contract

| | |
| --- | --- |
| **Input** | A GitHub Issue number, and the target lifecycle state. |
| **Output** | The issue's new lifecycle state, and the transition actually applied. |
| **Writes** | The issue's `status:` label in GitHub. |
| **Confirmation** | **Required.** Confirm before applying. |

**Guarantees to the caller**

- The transition applied was fetched from the issue's **current** `status:` label, never assumed from a mapping table.
- Exactly one transition per invocation.
- A transition unavailable from the current state is reported together with the list of what is available.

**Never**

- Chain several transitions to reach a target state.
- Fire a transition the lifecycle configuration marks human-only.
- Reverse a human-only transition found already applied: report it as a signal instead.

## How to perform

The lifecycle state is carried as a `status:<state>` label on the GitHub Issue. Transitioning means removing the old state label and adding the new one in a single call.

```bash
gh issue edit <number> --remove-label "status:<from>" --add-label "status:<to>"
```

Remove the old state label in the same call. Two `status:` labels on one issue makes every downstream query ambiguous, and nothing in GitHub prevents it.

## Lifecycle states

| State | Label | Meaning |
| --- | --- | --- |
| Backlog | (no `status:` label) | Item exists, not yet refined |
| Refined | `status:refined` | Ready gate passed, ready to plan |
| Planned | `status:planned` | Plan gate passed, ready to implement |
| In Progress | `status:in-progress` | Implementation started |
| In Review | `status:in-review` | Pull request opened |
| Done | `status:done` | Done gate passed, ready to merge |

## Closing

Closing an issue is a separate act from reaching a terminal lifecycle state. The `/integrate` stage sets `status:done` and closes the issue in the same transition, because Done is terminal for this project.

```bash
gh issue close <number>
```

## Transitions no command ever fires

| Transition | Meaning | Who fires it |
| --- | --- | --- |
| Reopen | Reversing Done | human only |
| Block | Item is blocked by a dependency | human only |
| Deprioritise | Item moved back to Backlog from a later state | human only |

The repository is `juangcarmona/domusmind`.
