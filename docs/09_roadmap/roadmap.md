# DomusMind - Roadmap

Status: Canonical
Audience: Product / Engineering / Architecture
Scope: Cross-version planning
Owns: Phased product evolution
Depends on: docs/00_product/strategy.md, docs/01_system/system-overview.md, specs/system/system-spec.md
Replaces: previous roadmap version

## Purpose

This document defines the phased evolution of DomusMind.

The roadmap prioritizes a strong operational core first, then controlled domain expansion, then intelligence and integrations.

It is intentionally conservative.

Each phase must extend the product without weakening:

- bounded context clarity
- aggregate ownership
- slice independence
- API consistency
- product coherence

---

## V1 - Household Operating Core

### Goal

Deliver the minimum viable household operating model.

### Core contexts

V1 includes five core bounded contexts:

- Family
- Responsibilities
- Calendar
- Tasks
- Shared Lists

### Core capabilities

V1 establishes the household coordination core through:

- household creation
- member management
- responsibility ownership
- event scheduling and reminders
- task execution
- routine management
- persistent shared lists
- household timeline visibility

### Product outcome

By the end of V1, DomusMind should provide:

- shared household identity
- explicit accountability
- visible plans in time
- visible operational work
- persistent shared list state
- a usable Today- and week-oriented household experience

### Success shape

V1 is successful when a household can:

- represent who belongs to the home
- make responsibility visible
- see what is happening
- track what needs doing
- maintain reusable shared lists
- understand what matters today without depending on one person’s memory

---

## V1.1 - Operational Hardening

### Goal

Stabilize the V1 core for real usage.

### Rule

V1.1 introduces no new bounded contexts.

### Focus areas

This phase focuses on hardening what already exists:

- reliability
- API maturity
- validation completeness
- security reinforcement
- projection quality
- UX tightening
- edge-case handling
- operational readiness

### Deferred capabilities

The following capabilities may land in V1.1 if needed to complete the current model cleanly:

#### Family
- assign-relationship
- remove-member

#### Shared Lists
- any incomplete list lifecycle operations needed to finish the current list model cleanly

### Outcome

V1.1 should make the core trustworthy, not broader.

The main result is confidence in the existing system, not expansion into new household domains.

---

## V2 - Household Domain Expansion

### Goal

Extend DomusMind beyond the operating core into adjacent household domains.

### Candidate expansion areas

These areas are intentionally downstream from the V1 core:

- administration
- documents
- property / maintenance
- inventory-aware household state
- food / meal coordination

### Possible capabilities

Examples of V2 capabilities include:

- important document tracking
- renewal and deadline visibility
- property maintenance planning
- richer household stock awareness
- pantry or supply-state modeling
- meal coordination linked to household reality

### Rule

V2 must build on the V1 core model.

New domains must not blur the boundaries between:

- time
- execution
- responsibility
- list-based coordination

### Outcome

V2 broadens household coverage while preserving the clarity of the V1 operating model.

---

## V3 - Intelligence and Integrations

### Goal

Add intelligence, interpretation, and external connectivity on top of a stable household core.

### Candidate capabilities

Examples include:

- messaging integrations
- calendar synchronization
- import and capture shortcuts
- reminder routing
- automation pipelines
- projections and analytics
- AI-assisted interpretation of household input

### Rule

Intelligence comes after clarity.

Automation must strengthen the existing model, not bypass it.

Integrations must respect context boundaries and aggregate ownership.

### Outcome

V3 should reduce friction further by making capture faster, interpretation smarter, and coordination more anticipatory.

---

## Out of Scope for the Current Phase

Not part of the current implementation focus:

- advanced autonomous agents
- full finance platform
- large integration catalog
- complex multi-tenant SaaS concerns as a primary driver
- external identity federation as a requirement

---

## Planning Rules

### 1. The roadmap is additive

New phases extend the model.
They do not rewrite the core every time.

### 2. Product clarity beats scope growth

A smaller coherent system is better than a larger ambiguous one.

### 3. Core semantics stay protected

No roadmap phase may weaken:

- bounded context ownership
- aggregate integrity
- cross-context isolation
- household-facing clarity

### 4. Public promise must remain truthful

The roadmap may describe future direction, but current product messaging must stay grounded in what exists now.

---

## Summary

DomusMind evolves in three major steps:

- V1: household operating core
- V1.1: operational hardening
- V2: household domain expansion
- V3: intelligence and integrations

Current priority is still clear:

build a strong household operating core first.