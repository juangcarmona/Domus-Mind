---
name: rebase-safely
description: "Lifecycle: conduct a git rebase safely: preflight, a backup ref, and conflict-by-conflict resolution that challenges the operator instead of guessing. Use when rebasing a branch onto an updated upstream, replaying commits, or resolving rebase conflicts."
metadata:
  plugin: pc-lifecycle
  author: "Juan García Carmona"
---

One responsibility: drive a rebase to completion without a single silently resolved conflict: preflight, replay, challenge every conflict, verify.

Not for `git merge`. Flag before rewriting a branch other people build on.

## Orientation: read before touching a conflict

**During a rebase, `ours` and `theirs` are inverted** from what intuition says. The rebase replays *your* commits on top of the upstream, so at each stop:

- `ours` = the **upstream**: the branch you are rebasing onto.
- `theirs` = **your commit** being replayed.

Getting this backwards silently discards exactly the change you meant to keep. Always name which **real** branch each side is before resolving anything.

## Steps

1. **Preflight.** Establish a safe starting point before rewriting any history.
   - Clean the tree: commit or stash everything, so `git status` is clean.
   - Name the three points: the current branch, the target to rebase **onto**, and the merge-base. Confirm the target with the operator wherever there is any ambiguity.
   - `git fetch` the target, so the rebase lands on its true latest rather than a stale local ref.
   - Create a dated backup: `git branch backup/<branch>-<yyyymmdd>`.
   - Enable resolution reuse once: `git config rerere.enabled true`.
   - Scope the work and preview the collisions: `git log --oneline <target>..HEAD` for the commits to replay, then intersect `git diff --name-only <merge-base> HEAD` with `git diff --name-only <merge-base> <target>` to list the files touched on both sides.
   - Done when: the tree is clean, the target is confirmed and fetched, the backup ref exists, and the operator has seen the commit count and the overlapping-file list.

2. **Start the rebase.** `git rebase <target>`: plain, unless the branch carries merge commits genuinely worth preserving. No conflicts → go to step 4.
   - Done when: the rebase completes cleanly or stops at its first conflict.

3. **Resolve each conflict: challenge, never guess.** A loop, repeated at every stop. For each conflicted file:
   - **State the orientation for this stop**: which real branch is `ours`, which is `theirs`, and the subject of the commit being replayed.
   - For each hunk, explain what each side changed and **why they collide**: a textual overlap and a genuine semantic conflict need different answers.
   - Propose a resolution **with reasoning**, as a recommendation and not a settled fact. Ask the operator to confirm or redirect.
   - **Challenge lazy answers.** "Just take mine" gets named consequences, *"that discards the null-check the target added to X"*, and requires acknowledgement before it is applied.
   - After resolving, confirm no conflict markers remain anywhere in the file and that the result reads coherently, then stage it. When the stop is fully resolved, continue the rebase.
   - **Escape hatch**: if resolution turns unsafe, or the operator loses confidence, offer `git rebase --abort`, it restores the exact pre-rebase state, rather than forcing through.
   - Done when: every conflict of every replayed commit is resolved with confirmed intent, no markers remain, and the rebase reports it finished.

4. **Finish and verify.** A cleanly-applying rebase can still be semantically broken.
   - Run the project's checks. Replayed commits can each compile alone and break in combination.
   - Confirm intent survived: `git range-diff <target> backup/<branch>-<yyyymmdd> HEAD` compares the old and new commit series so each commit can be seen to have survived as meant.
   - Push only with `--force-with-lease`, **never** `--force`, and confirm first: this rewrites shared history.
   - Keep the backup ref until the operator confirms the result is good. Delete it only then.
   - Done when: the checks passed, the range-diff was reviewed, and the branch was pushed on confirmation.

## Rules

- **Never resolve a conflict without first stating the inverted orientation** for that stop.
- **Never apply `-X ours` / `-X theirs`, `git rebase --skip`, or a bulk `git checkout --ours/--theirs`** without explicit, per-conflict consent. These discard changes wholesale and leave no trace of what was lost.
- **Never `git push --force`.** Always `--force-with-lease`, always confirmed.
- **Flag before rebasing a branch others are actively building on.**
- **Never delete the backup ref before the operator confirms the result.**
