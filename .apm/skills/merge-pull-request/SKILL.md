---
name: merge-pull-request
description: "Complete an approved pull request into main. Use at close-out, once the Done gate has passed and the review is approved."
---

One responsibility: merge one approved pull request, composing the commit message that lands on the target branch.

## Contract

| | |
| --- | --- |
| **Input** | A pull request number, the merge strategy (`squash`), and the subject and body for the resulting commit. |
| **Output** | The merge commit's SHA and the pull request's final state. |
| **Writes** | Merges into `main` and deletes the source branch. |
| **Confirmation** | **Required.** Confirm before merging. |

**Guarantees to the caller**

- The pull request was still open and still approved immediately before the merge, re-checked rather than assumed.
- The commit subject describes the **implemented change**, not the proposal the pull request opened with.
- The message is set explicitly, so no auto-generated body carries anything unintended onto the target branch.

**Never**

- Merge a draft pull request.
- Merge a pull request whose required checks have not passed.
- Cast an approval on the author's behalf without explicit human confirmation.
- Let the forge auto-generate the squash body.

## How to perform

### Fix the title before merging if it still describes the proposal

```bash
gh pr edit <number> --title "<subject>"
```

A title edit does not re-trigger CI. GitHub appends the pull request number itself, so never put it in the title.

### Merge with explicit subject and body

```bash
gh pr merge <number> --squash --subject "<subject>" --body "<body>"
```

### Never let the body auto-generate

GitHub's default squash body concatenates every branch commit message. Anything unintended in a branch commit, a CI-skip marker especially, lands on the default branch inside that body, and GitHub scans the whole message. Pass `--subject` and `--body` explicitly. An empty `--body ""` is acceptable.

## Merge strategy

This project uses **squash** merge. The squash commit subject becomes the history entry on `main`.

The repository is `juangcarmona/domusmind`. The default branch is `main`.
