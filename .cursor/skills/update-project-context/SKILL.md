---
name: update-project-context
description: >-
  Updates this backend repo's Cursor rules and skills when product decisions,
  architecture, or conventions change. Use when the user says "update context",
  "update rules", "update skills", or "remember this" for backend/.NET work.
---

# Update project context (backend)

This repo keeps its own `.cursor/rules/` and `.cursor/skills/`. Do not put backend conventions in the Flutter repo’s `.cursor/`.

## When to write

Update files when a decision should still be true in a future session. Skip ephemeral git status, one-off bugs, and chat-only scratch.

## Where to put it

| Kind | Location |
|------|----------|
| Always-on product/architecture/stack | `.cursor/rules/*.mdc` with `alwaysApply: true` |
| File-specific constraints | `.cursor/rules/*.mdc` with `globs` |
| Multi-step workflow | `.cursor/skills/<name>/SKILL.md` |

Always-on: `communication.mdc`, `project.mdc`, `architecture.mdc`, `stack.mdc`. File-specific: `api-controllers.mdc`, `ef-data.mdc`.

Workflow skills: `commit-changes` (phrase: “Commit backend changes”; split by kind; skip local-dev-only; never add agent as contributor).

## How

1. Identify which rule or skill the change belongs to (one concern per file).
2. Edit in place; keep each rule under ~50 lines and actionable.
3. Do not duplicate the same fact across rules and skills. Skills hold procedures; rules hold constraints and facts.
4. Chat replies stay English (see `communication.mdc`).
