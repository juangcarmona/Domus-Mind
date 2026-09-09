# read-work-item-links: GitHub Issues

GitHub has no first-class link type. Relationships are expressed as references in the body or in timeline events, and as sub-issues where the repository uses them.

```bash
gh issue view <number> --json body,projectItems
gh api repos/{owner}/{repo}/issues/<number>/timeline --jq '.[] | select(.event=="cross-referenced")'
```

Report the relationship as the project expresses it, and say plainly that direction is a convention here rather than a typed fact.
