---
name: inspect-ci-result
description: "Return the GitHub Actions CI result for a branch or pull request: outcome, failing job, and log excerpts. Use when checking whether CI passed or diagnosing why it did not."
---

One responsibility: report what CI actually did for a branch or pull request.

## Contract

| | |
| --- | --- |
| **Input** | A branch name or a pull request number. |
| **Output** | Per required check: its name, conclusion, and where it failed: the failing job and a log excerpt. Plus one overall verdict. |
| **Writes** | Nothing. Read-only. |
| **Confirmation** | Not required, no external write. |

**Guarantees to the caller**

- Three states are distinguished and never collapsed: passed, failed, and **not yet run**.
- A check with no run for the current head is reported as having no result, never as passing.
- The set of checks reported is the set the branch protection **requires**, not merely those that happen to have run.

**Never**

- Treat a queued or in-progress run as a passing one.
- Report an empty result set as success: no checks having run is not the same as nothing failing.
- Re-run or cancel anything.

## How to perform

### Check rollup for a PR

```bash
gh pr view <number> --json statusCheckRollup
```

### List recent runs for a branch

```bash
gh run list --branch <branch> --limit 5
```

### View failed run logs

```bash
gh run view <run-id> --log-failed
```

### What gates the merge

```bash
gh api repos/juangcarmona/domusmind/branches/main/protection \
  --jq '.required_status_checks.contexts'
```

A `403` means protection is not readable: on some plans it is not configured at all. Report that, rather than assuming either answer.

## The empty-rollup trap

A head commit with **no check runs** returns an empty rollup, which reads as "nothing failing" while proving nothing. This happens after a push whose commit message carries a CI-skip marker. An empty rollup is *no result*, never a pass.

## Required checks for this project

| Check | Workflow | Runs on |
| --- | --- | --- |
| Backend build + test | `backend-ci` | PRs touching `src/backend/**` or `tests/backend/**` |
| Web app build | `webapp-ci` | PRs touching `src/web/app/**` |
| Public site build | `public-site-ci` | PRs touching `src/web/public/**` |

The repository is `juangcarmona/domusmind`.
