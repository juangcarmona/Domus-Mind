# propose-change: OpenSpec

Where OpenSpec is present, `openspec init` generates its own skills and commands, version-matched to the installed CLI. **Adoption does not install the plan-artifact templates at all in that case**: it runs the init and lets the CLI own these names.

This reference exists to record that decision, not to duplicate the CLI's behaviour. A vendored copy would drift from what the CLI produces and collide with it under the same names.
