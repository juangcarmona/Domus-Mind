---
name: git-worktrees
description: "Plain Concepts Lifecycle: run parallel work in git worktrees without cross-contaminating branches, build output, tooling configuration or telemetry. Use when starting a second work item while one is in flight, reviewing a branch without disturbing the current one, or when a worktree's tooling behaves differently from the main checkout."
metadata:
  plugin: pc-lifecycle
  author: "Juan García Carmona"
---

One responsibility: create, use and retire a worktree so that two work items can proceed at once without either one polluting the other.

A worktree is a second checkout of the same repository sharing one `.git` directory. It earns its keep when switching branches would cost more than the disk: a long-running item in flight, a review to perform without disturbing the current tree, an urgent fix while a feature is half-applied.

It is not free. Everything below is a way the second checkout differs from the first in a way that has bitten someone.

## Steps

1. **Decide whether a worktree earns its place.** A quick branch switch on a clean tree does not need one. A worktree is worth it when the current tree holds work you would otherwise stash, or when both checkouts must exist simultaneously.
   - Done when: the reason is stated. "It seemed tidier" is not one.

2. **Place it outside the repository, or somewhere the repository ignores.** A worktree nested in the main checkout appears in every glob, every search, every file watcher, and every tool that walks the tree: including the ones that then try to build it.
   - Done when: the path is either outside the repository root, or inside a directory the repository already ignores.

3. **Avoid a leading dot anywhere in the worktree's path.** Several test runners and bundlers treat a path segment beginning with `.` as an escaped glob character, and silently match nothing. The failure mode is the worst kind: the runner exits 0 having collected zero tests.
   - Done when: no path segment starts with `.`, or a run has proved the project's test command still collects its tests there.

4. **Create it with a branch, named after the work item.**
   ```bash
   git worktree add <path> -b <branch-name>     # new branch
   git worktree add <path> <existing-branch>    # existing branch
   ```
   Git refuses to check the same branch out twice. That refusal is a feature: two checkouts of one branch is how you lose a commit.
   - Done when: `git worktree list` shows the new tree on its intended branch.

5. **Install dependencies in the worktree.** Dependency directories, build output, caches and virtual environments are **not** shared: the new tree starts empty even though the source is there. A build that "works" before installing is reading artifacts from somewhere it should not be.
   - Done when: the project's install command has run in the worktree.

6. **Reproduce the untracked local configuration.** Environment files, local settings and credentials are untracked by design, so they do not come along. Copy what the project needs, and **never** copy a secret into a path that is not ignored in the new tree.
   - Done when: the project's own instructions for local setup have been satisfied in the worktree.

7. **Check what tooling resolves to the main checkout.** Hooks, generated configuration, telemetry mapping files and anything holding an absolute path may still point at the original tree. Two symptoms to look for: work performed in the worktree recorded against the main checkout, and a tool reading a configuration file the worktree does not have.
   - Done when: anything that writes a record has been confirmed to write it against *this* worktree, or the discrepancy is reported.

8. **Retire it deliberately.**
   ```bash
   git worktree remove <path>       # refuses if the tree is dirty
   git worktree prune               # clears entries whose directory is gone
   ```
   Deleting the directory by hand leaves a stale administrative entry that makes git refuse to reuse the path or the branch later.
   - Done when: `git worktree list` no longer shows it, and the branch is either merged or deliberately kept.

## Rules

- **One work item per worktree, one worktree per work item.** Two items sharing a tree is the situation worktrees exist to prevent.
- **Never delete a worktree directory by hand.** Use `git worktree remove`, then `prune`.
- **A worktree is not a backup.** It shares one object database with the main checkout; a destructive history operation reaches every tree.
- **Verify in the worktree you changed.** A verdict belongs to the tree that produced it, and having two trees makes it far easier to prove one and push the other.
- **Do not commit a worktree's path into shared configuration.** It is local to one machine and meaningless to everyone else.

## Symptoms and causes

| Symptom | Cause |
| --- | --- |
| The test command exits 0 having run nothing | A dot-segment in the worktree path being read as an escaped glob character (step 3) |
| A build fails on missing dependencies that exist in the main checkout | Dependency directories are not shared (step 5) |
| Effort or session records attributed to the wrong branch | Tooling resolving a mapping file relative to the main checkout (step 7) |
| Git refuses to create a worktree at a path that no longer exists | A stale administrative entry; run `git worktree prune` (step 8) |
| Git refuses to check out a branch | It is already checked out in another worktree. Find it with `git worktree list` |
