---
name: open-pull-request
description: "Open a pull request from the current branch, linked to its work item. Use when a stage needs a pull request raised, in draft or ready for review."
---

One responsibility: create one pull request for the current branch and link it to its work item.

## Contract

| | |
| --- | --- |
| **Input** | The source branch, the target branch (`main`), a title, a body, and whether it opens as a draft. |
| **Output** | The pull request's number and URL, and its draft state. |
| **Writes** | Creates a pull request in GitHub. |
| **Confirmation** | **Required.** Confirm before creating. |

**Guarantees to the caller**

- Exactly one pull request per work item: an existing open one is reported and reused, never duplicated.
- The work item number appears in the body via `Closes #<number>`, so the issue and the change are traceable to each other.
- The draft state is exactly as the caller asked; a stage that wants a review gate gets an unmergeable draft.

**Never**

- Open a second pull request for an item that already has one.
- Mark a pull request ready for review; that is a separate role.
- Add reviewers the project's configuration does not name.

## How to perform

### Check for an existing one first

```bash
gh pr list --head <branch> --state open --json number,url
```

A duplicate pull request splits the review and the history.

### Create

```bash
gh pr create --base main --head <branch> --title "<title>" --body "<body>" --draft
```

### Linking

Put `Closes #<number>` in the **body**, not the title: the title becomes the squash commit subject, and a closing keyword there is noise. The body keyword is what closes the issue on merge.

### Draft

`--draft` is the review gate's mechanism: the pull request cannot be merged. Use it when the `/implement` stage opens the PR. The `/integrate` stage marks it ready for review (a separate act).

The repository is `juangcarmona/domusmind`. The default branch is `main`.
