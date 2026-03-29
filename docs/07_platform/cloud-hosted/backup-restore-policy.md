# CloudHosted - Backup and Restore Policy

## Status

Accepted — Managed backup baseline for first rollout

---

## Backup Posture

All persistent state lives in Azure Database for PostgreSQL Flexible Server.

Azure manages automated backups.

Default retention and behavior:

- automated daily backups are enabled by default on Flexible Server
- point-in-time restore (PITR) is available within the retention window
- minimum retention: 7 days
- recommended retention for first rollout: 7–35 days (operator configures at provisioning time)
- geo-redundant backups: optional, enables cross-region restore

The operator does not need to implement manual backup jobs for core data. Azure manages backup creation and storage.

---

## Restore Runbook Expectations

The operator must have a documented restore runbook before the deployment accepts its first real household.

Minimum runbook content:

1. confirm last known good backup point (Azure portal or CLI)
2. trigger point-in-time restore to a new Flexible Server instance
3. update App Service connection string to point at the restored instance
4. verify application startup and migration state
5. confirm household data is intact (spot-check via API or operator tooling)
6. communicate status to affected households if data loss occurred

The restore runbook must be tested at least once before CloudHosted goes live with real households.

---

## Operator Responsibilities

- configure backup retention at provisioning
- document the restore runbook
- test restore before live households are onboarded
- maintain the runbook after every infrastructure change
- execute restore if a data incident occurs
- notify affected households of any confirmed data loss

---

## Backup Validation Expectations

Automated backups do not guarantee recoverability without a restore test.

Before going live:

- perform at least one successful point-in-time restore to a staging or throwaway instance
- verify the application starts and migrations are in the correct state on the restored instance
- discard the test instance after validation

---

## Risk Notes for First Rollout

- Flexible Server enforces backup retention based on the tier. Verify the configured tier supports the desired retention window.
- Point-in-time restore creates a new server instance; connection strings must be updated.
- Data changes between the last backup point and the failure time are lost. Communicate this boundary honestly to affected households.
- Restore to the same server (in-place) is not supported in all scenarios. Plan for the new-instance restore path.

---

## Non-Goals for First Rollout

- no application-level backup tooling beyond Azure managed backup
- no cross-region replica for high availability (deferred)
- no PostgreSQL dump export automation (manual if needed via operator tooling)
- no automated restore testing pipeline
- no SLA guarantees on RTO or RPO
