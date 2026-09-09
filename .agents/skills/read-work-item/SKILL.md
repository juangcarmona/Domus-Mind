---
name: read-work-item
description: "Read a GitHub Issue by number into structured fields. Use when any stage needs an item's current content or state."
---

One responsibility: return one work item's content and current state as structured fields.

## Contract

| | |
| --- | --- |
| **Input** | A GitHub Issue number. |
| **Output** | Structured fields: number, title, body, state (open/closed), lifecycle state (from `status:` label), assignees, labels, acceptance criteria (from body section). |
| **Writes** | Nothing. Read-only. |
| **Confirmation** | Not required, no external write. |

**Guarantees to the caller**

- Every field the project's Definition of Ready references is present, or explicitly reported absent.
- The lifecycle state is read from the `status:<state>` label, reported verbatim, never normalised into a guess.
- An issue number that does not resolve is reported as not found, never as an empty item.

**Never**

- Modify the issue.
- Infer a field the tracker did not return.
- Follow links or fetch attachments: those are separate roles.

## How to perform

```bash
gh issue view <number> --json number,title,body,state,labels,assignees,projectItems
```

| Contract field | GitHub source |
| --- | --- |
| identifier | `number` |
| title | `title` |
| description | `body` (Markdown) |
| state | `OPEN` / `CLOSED` |
| lifecycle state | the `status:<state>` label, or `Backlog` if none |
| assignee | `assignees` |
| labels | `labels` (excluding `status:` labels, which carry lifecycle state) |
| acceptance criteria | the `## Acceptance Criteria` section of `body` |

## Notes

- GitHub's own `state` is only open or closed. The lifecycle state lives on a `status:<state>` label. If no `status:` label is present, the item is in `Backlog`.
- `body` is Markdown with no schema. Acceptance criteria are kept under a `## Acceptance Criteria` heading by project convention.
- The repository is `juangcarmona/domusmind`.
