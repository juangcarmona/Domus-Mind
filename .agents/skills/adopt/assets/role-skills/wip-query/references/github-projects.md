# wip-query: GitHub Projects

```bash
gh issue list --label "status:in-progress" --state open --json number,title
```

Where the lifecycle state lives in a Projects field rather than a label, query the project's items and filter on that field instead.
