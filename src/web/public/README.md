# DomusMind public site

This directory contains the Astro-based public website for DomusMind.
It is part of the main repository (not a submodule).

## Node version

- Required: Node `>=22.12.0`
- Local version pin: `.nvmrc`

## Run locally

```bash
cd src/web/public
npm ci
npm run dev
```

## Build

```bash
cd src/web/public
npm run build
```

## Content and i18n structure

- Long-form page content is stored as Markdown in `src/content/<locale>/*.md`
- UI translation fragments are stored in `src/content/<locale>/ui.json`
- Supported locales: `en`, `es`, `fr`, `de`, `it`, `zh`, `ja`
- English (`en`) is canonical
- If a localized page is missing, the route falls back to the English page content for that full page

## Deployment target

- Azure Static Web Apps
- Build output: `dist`
- Workflow files:
  - `.github/workflows/public-site-ci.yml`
  - `.github/workflows/public-site-deploy.yml`
