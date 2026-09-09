# inspect-ci-result: GitHub Actions

```bash
gh pr view <number> --json statusCheckRollup
gh run list --branch <branch> --limit 5
gh run view <run-id> --log-failed
```

## What gates the merge

```bash
gh api repos/{owner}/{repo}/branches/{branch}/protection \
  --jq '.required_status_checks.contexts'
```

A `403` means protection is not readable: on some plans it is not configured at all. Report that, rather than assuming either answer.

## The empty-rollup trap

A head commit with **no check runs** returns an empty rollup, which reads as "nothing failing" while proving nothing. This happens after a push whose commit message carries a CI-skip marker. An empty rollup is *no result*, never a pass.
