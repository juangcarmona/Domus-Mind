# open-pull-request: GitHub

```bash
gh pr create --base <target> --head <branch> --title "<title>" --body "<body>" --draft
```

## Check for an existing one first

```bash
gh pr list --head <branch> --state open --json number,url
```

A duplicate pull request splits the review and the history.

## Draft

`--draft` is the review gate's mechanism: the pull request cannot be merged, and workflows gated on `pull_request` typically do not run for a draft.

## Linking

Put `Closes #<number>` in the **body**, not the title: the title becomes the squash commit subject, and a closing keyword there is noise. The body keyword is what closes the issue on merge.
