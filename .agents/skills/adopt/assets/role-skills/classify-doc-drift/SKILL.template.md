---
name: classify-doc-drift
description: "Classify each mapped documentation pair as in sync, repo ahead, wiki ahead, diverged, or unmapped, without changing either side. Use before trusting a page, or after either side has been edited."
---

> **Template.** The lifecycle's `adopt` capability adapts this into `.apm/skills/classify-doc-drift/SKILL.md`: the provider-neutral source APM then projects into each harness the project runs. Adapt *how* the role is performed; never change *what* it guarantees to its callers.

One responsibility: say which side of a documentation pair is ahead, and by how much.

## Contract

| | |
| --- | --- |
| **Input** | The project's documentation mapping; which repository file corresponds to which page. |
| **Output** | Per pair: in sync, repo ahead, knowledge base ahead, diverged, or unmapped: with the evidence for the verdict. |
| **Writes** | Nothing. Read-only. |
| **Confirmation** | Not required, no external write. |

**Guarantees to the caller**

- **Diverged** is reported as its own verdict, never collapsed into one side being ahead: both changed, and no automatic sync is safe.
- An unmapped file or page is reported rather than ignored.

**Never**

- Perform the sync.
- Pick a winner. Choosing which side wins is a human decision.

## Adapting this template

1. Pick the reference below matching this project's tooling.
2. Replace every `<placeholder>` with a value **resolved from live tooling in the adoption session**: an identifier transcribed from a diagram or from memory breaks the loop a month later.
3. Keep the contract above intact. A command calling `classify-doc-drift` must get the same shape back on every stack.
4. No reference matches → author against the contract and **record that it was authored, not adapted**. A fabricated reference is worse than a missing one, because everything downstream trusts it.

## References

- [`references/wiki.md`](references/wiki.md): a hosted wiki
- [`references/repo-docs.md`](references/repo-docs.md): Markdown in the repository
