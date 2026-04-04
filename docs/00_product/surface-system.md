Status: Canonical
Audience: Product / Design / Engineering
Scope: Cross-surface UX system for V1 and the current surface-system-reboot
Owns: App shell, layout grammar, density rules, visual tone, interaction grammar, responsive behavior, and surface anti-patterns
Depends on: docs/00_product/strategy.md, docs/00_product/experience.md

# DomusMind - Surface System

This document defines the shared UX system for DomusMind.

It exists to keep Today, Planning, Lists, Areas, and Member Agenda inside one coherent product shell.

It does not define domain behavior.
It defines how product surfaces should be structured and behave.

---

# Core Direction

DomusMind should move toward:

- persistent app shell
- stronger layout discipline
- denser information
- quieter styling
- desktop-first split views
- inspector/sidebar over modal chaos
- real hierarchy over floating cards
- neutral base with restrained accents
- mobile collapse of the same product, not a redesign

---

# Shell Model

## Desktop

The default desktop shell is:

- left navigation rail
- compact page header
- central work canvas
- optional right contextual inspector

This is the standard composition model for major surfaces.

## Mobile

Mobile keeps the same product logic in collapsed form:

- top header
- primary content first
- compact controls
- contextual detail as bottom sheet or pushed section
- drawer or compact navigation pattern where needed

Mobile must feel like a smaller DomusMind, not a different product.

---

# Layout Grammar

Every major surface should use the same structural zones when relevant:

- navigation
- page header
- controls
- main content
- contextual detail

## Page Header

Page headers stay compact.

They may include:

- title
- breadcrumb or back path
- date context
- primary action
- compact filters
- search when relevant

Do not use tall decorative headers.

## Main Content

Main content is where household state is read and manipulated.

It must prioritize:

- scan speed
- visible state
- compact grouping
- low navigation cost

## Contextual Detail

Secondary detail belongs in the inspector by default on desktop.

Use it for:

- item inspection
- lightweight editing
- related metadata
- participants
- notes
- linked entities

Avoid full-page navigation for simple inspection.

---

# Visual Tone

DomusMind should feel calm, modern, and operational.

Use:

- warm or neutral backgrounds
- white or near-white surfaces
- subtle borders
- restrained shadows
- one accent system
- consistent radius and spacing

Avoid:

- loud gradients as default surface treatment
- heavy card framing
- decorative containers with little content
- strong shadows
- colorful noise

Theme should support identity, not dominate the interface.

---

# Density Rules

DomusMind must become denser.

That means:

- tighter vertical rhythm
- lower default component height
- less decorative padding
- fewer empty wrappers
- stronger typography hierarchy
- more useful state in view

Density does not mean clutter.
Density means removing waste.

---

# Interaction Grammar

## Content first

The content is the hero.

The interface must make household state more visible than chrome.

## Detail is secondary

Overview comes first.
Detail comes second.

Selecting an item should usually keep the surrounding context visible.

## Capture stays local

Creation and quick-add actions should stay close to the current surface.

Examples:

- add item from within a list
- add plan from planning
- add task from the current operational context

## Counts should be visible

Surfaces should expose relevance before opening deep detail.

Examples:

- unchecked list count
- items today
- open tasks
- upcoming items

## Completed state should compress

Completed or checked items should remain accessible but should not dominate the default view.

## Controls stay compact

Search, tabs, filters, and segmented controls should remain close to the surface header and action context.

---

# Responsive Rules

## Desktop preference

Desktop should prefer:

- split views
- visible inspectors
- dense toolbars
- more state visible at once

## Mobile preference

Mobile should prefer:

- stacked sections
- bottom sheets for contextual detail
- compact headers
- compressed controls
- high-priority content first

The product logic must remain the same across breakpoints.

---

# Surface Pattern Defaults

These are the default interaction choices unless a surface has a strong reason to differ.

## Use an inspector when:

- the user is inspecting one selected item
- edit depth is light
- context should remain visible

## Use a modal when:

- the action is destructive
- the flow is short and interruptive by nature
- the user must finish or cancel before continuing

## Use full-page navigation when:

- the user is moving to a distinct work context
- the flow requires depth or sustained focus
- the item is itself a full operational surface

---

# Anti-Patterns

Avoid these patterns across the product:

- giant centered islands
- page-as-a-pile-of-cards composition
- cards inside cards inside cards
- oversized decorative headers
- modal chaos on desktop
- page-specific visual languages
- excessive empty padding
- chrome heavier than content
- full navigation for simple inspection
- mobile redesigns that change the product model

---

# Success Criteria

The surface system is working when:

- all major surfaces feel like one product
- content dominates chrome
- desktop feels efficient and calm
- mobile feels like the same product in a smaller frame
- inspectors reduce navigation cost
- density improves without visual hostility
- no surface looks like an isolated design experiment

---

# Summary

DomusMind should present one shared surface language across all operational modules.

The system is defined by:

- one shell
- one layout grammar
- one visual tone
- one interaction model
- one responsive logic

Surface specs inherit from this document.
They must not redefine it.