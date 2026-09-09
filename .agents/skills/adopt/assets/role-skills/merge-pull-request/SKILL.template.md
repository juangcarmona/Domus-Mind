---
name: merge-pull-request
description: "Complete an approved pull request into its target branch. Use at close-out, once the Done gate has passed and the review is approved."
---

> **Template.** The lifecycle's `adopt` capability adapts this into `.apm/skills/merge-pull-request/SKILL.md`: the provider-neutral source APM then projects into each harness the project runs. Adapt *how* the role is performed; never change *what* it guarantees to its callers.

One responsibility: merge one approved pull request, composing the commit message that lands on the target branch.

## Contract

| | |
| --- | --- |
| **Input** | A pull request identifier, the merge strategy, and the subject and body for the resulting commit. |
| **Output** | The merge commit's identifier and the pull request's final state. |
| **Writes** | Merges into the target branch and, where configured, deletes the source branch. |
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

## Adapting this template

1. Pick the reference below matching this project's tooling.
2. Replace every `<placeholder>` with a value **resolved from live tooling in the adoption session**: an identifier transcribed from a diagram or from memory breaks the loop a month later.
3. Keep the contract above intact. A command calling `merge-pull-request` must get the same shape back on every stack.
4. No reference matches → author against the contract and **record that it was authored, not adapted**. A fabricated reference is worse than a missing one, because everything downstream trusts it.

## References

- [`references/azure-devops.md`](references/azure-devops.md): Azure DevOps Repos
- [`references/github.md`](references/github.md): GitHub
