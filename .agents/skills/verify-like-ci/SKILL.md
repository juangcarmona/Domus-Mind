---
name: verify-like-ci
description: "Lifecycle: run the checks CI will run, locally, deriving them from the pipeline definitions instead of memory. Use before committing or pushing, before claiming a change is green, before opening or un-drafting a pull request, or after fixing anything a linter, analyzer or test reported."
metadata:
  plugin: pc-lifecycle
  author: "Juan García Carmona"
---

One responsibility: prove locally that every check the pipeline runs passes, so CI is never the first place a failure appears.

A change is green when **the commands CI runs** exit 0 here, in CI's configuration. A subset that passes proves nothing about the rest.

**Output:** one line per check (command, observed exit code, verdict), and a single overall verdict. For a test check, the line also names a test *this change touched* that appeared in the run.

CI-system specifics live in `references/`. Read the one matching this project.

## Steps

1. **Verify the tree CI verifies, not the branch's own.** Most systems validate the *merge* of the branch with the current target branch, so checks passing on a stale branch prove nothing about the run CI will do. Fetch, then require the branch to be zero commits behind the target; if it is not, bring it up to date before running any check.
   - Done when: the branch is not behind in this session. A run on a behind tree is reported `stale-tree(<n> behind)`, never green.

2. **Verify the tree in front of you, not one verified earlier.** A verdict belongs to the tree that produced it; nothing else inherits it. Content edited after a green run is `stale-verification`; content never verified is `unverified`. Neither is green, and both clear the same way: run the checks again.
   - Done when: the reported verdict describes the current content.

3. **Enumerate the checks from the pipeline definitions, not from memory.** Read the validation pipeline and every template or workflow it includes, and list each command it executes for the areas this change touches, in order.
   - Done when: the list was derived from the definitions **in this session**, never recalled.

4. **Resolve which checks actually gate the merge from the branch protection, not from the pipeline file.** A pipeline's trigger block says how a run *starts*; whether a run is *required* is a branch-protection fact the pipeline file cannot express, and the two answers differ. Query the target branch's protection rules.
   - Done when: the gating set came from that query in this session. **A pipeline file, a README, or a task list is never evidence for this**: a task list records what someone intended, not what is configured.

5. **Mirror CI's configuration.** Carry over what the definition passes: build configuration, the exact script name, the working directory. A debug build does not stand in for a release one, and one project's tests do not stand in for the whole run.

6. **Run each command to a terminal exit code** and record it.
   - Done when: every enumerated check has an exit code observed in this session. No check is marked green by inference from another.

7. **On a test check, confirm the change's own tests were among those that ran.** Exit 0 proves the command succeeded, not that it covered anything: a runner exits 0 just as happily when it collected none of the changed code's specs. Read the run's own output and name a test or spec file **this change added or edited** in it; where the change added tests, their count must be visible in the total.
   - Done when: such a test is named. Cannot name one → the check is `uncovering(<what ran instead>)`, never green, and the cause is found before going further.

8. **After any fix, re-run the whole command** the pipeline runs, not the sub-command that failed. See the short-circuit rule.

9. **Report the table and the overall verdict.** All zero → green. Any non-zero → red, naming the check. A check that cannot run on this machine is `unrunnable(<reason>)`, never passing, and the caller is told which pipeline check remains unproven rather than left to assume coverage.

## Rules

**The short-circuit rule.** A chained command (`a && b`, `a; b`) stops at the first failure, so the later checks **never ran**: their silence is not a pass. This is the single most common way a red pipeline gets reported green: an early failure hides a later check, the early one is fixed, only *it* is re-run, and the hidden check fails in CI. Re-run the full chain.

**A step's exit code must be the step's own.** A chain whose steps are piped, `lint | tail -2 && build`, reads the exit code of `tail`, which is always 0, so `&&` never stops and a failed check scrolls past as though it passed. Run every check so its own status is what the chain reads: no pipe on the checked command, or `set -o pipefail` before the chain.
- Done when: the command that produced each reported verdict *could have* failed the chain.

**An auto-fixer exiting 0 is not the check's verdict.** A formatter or `--fix` run succeeding says the fixer worked, not that the check passes. Run the check again.

**`unrunnable` is not `passing`.** A check needing credentials or infrastructure this machine lacks is reported as unproven, by name.

## Observed failure modes

Each of these shipped a green claim that CI then contradicted. They are why the steps above are worded the way they are.

| What happened | Which step catches it |
| --- | --- |
| Green on the branch; red on the merge build, because the target branch had landed a change the branch's own checks scanned | 1 |
| A green run, then one more edit, pushed without re-running. Nothing recorded *what* had been verified, so the new tree inherited the old verdict by implication | 2 |
| A pipeline carrying `trigger: none` was assumed not to gate the merge. It was a required check by branch policy; the claim reached three separate artifacts before the gate went red | 4 |
| A test script was a bare runner invocation that ran a single workspace project: 22 passing tests reported while 79 others, including 11 written that day, were never collected. Green on every pull request for three weeks | 7 |
| A lint failure scrolled past because its command was piped into `tail`, and the chain read `tail`'s exit code | Pipe rule |

## Do not

- Enumerate checks from memory of what they were last month.
- Report a subset as green for the whole.
- Treat a queued or in-progress run as a passing one.
