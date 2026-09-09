# create-work-item: GitHub Issues

```bash
gh issue create --title "<title>" --body "<body>" --label "<labels>"
```

The initial lifecycle state is applied separately by `transition-work-item`: creation and state are two acts, and conflating them hides which one failed.

Where the project uses an issue template, pass the body in the template's shape so the sections the project parses are present.
