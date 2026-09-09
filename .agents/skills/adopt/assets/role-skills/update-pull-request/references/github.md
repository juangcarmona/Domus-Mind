# update-pull-request: GitHub

```bash
gh pr edit <number> --title "<title>" --body-file <path>
gh pr ready <number>
```

`gh pr ready` un-drafts and triggers workflows gated on `pull_request`. A title edit does not re-trigger CI, which makes it safe immediately before a merge.
