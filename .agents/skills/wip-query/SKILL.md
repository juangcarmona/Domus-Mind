---
name: wip-query
description: "Count the GitHub Issues currently in progress or in review, against the project's WIP cap of 3. Use at pickup, when a work-in-progress cap must be enforced."
---

One responsibility: count what is genuinely in progress right now.

## Contract

| | |
| --- | --- |
| **Input** | Nothing beyond the project's configuration. |
| **Output** | The count of items in progress, the items themselves, and the configured cap (3). |
| **Writes** | Nothing. Read-only. |
| **Confirmation** | Not required, no external write. |

**Guarantees to the caller**

- Container types (epics, features, initiatives) are excluded at **every** level the project uses, not only the topmost.
- The count is of items in progress **now**, queried in this session, never carried from an earlier answer.

**Never**

- Refuse the work itself: this role reports; the stage decides.
- Count items in states the configuration does not call in-progress.

## How to perform

The lifecycle state is carried as a `status:` label. Items in progress are those with `status:in-progress` or `status:in-review`.

```bash
gh issue list --label "status:in-progress" --state open --json number,title
gh issue list --label "status:in-review" --state open --json number,title
```

Combine the two lists and count. The cap is **3**.

## WIP cap

| Cap | States counted |
| --- | --- |
| 3 | `status:in-progress` + `status:in-review` |

The repository is `juangcarmona/domusmind`.
