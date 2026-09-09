# inspect-pull-request: GitHub

```bash
gh pr view <number> --json isDraft,reviewDecision,reviews,statusCheckRollup,mergeStateStatus
gh api repos/{owner}/{repo}/pulls/<number>/comments
```

`reviewDecision` is `APPROVED`, `CHANGES_REQUESTED`, `REVIEW_REQUIRED`, or empty when no review is required at all. Empty is not approval.
