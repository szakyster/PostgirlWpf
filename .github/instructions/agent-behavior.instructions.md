---
applyTo: "**/*"
---

# Agent Behavior Instructions

## Routing
- When the user says `CP`, the request must be handled by the `git-expert` agent.
- For `CP`, prefer using `.github/skills/validated-build-commit-push/skill.md` when that workflow matches the request and its prerequisites are acceptable.
- Release orchestration is handled by `release-manager`.
- In release workflow, git operations (branch switch/check, commit/push, tag/push) must be handled by `git-expert`.
- In release workflow, build verification must be handled by `QM`.

## Typical development flow
- Preferred path for feature delivery: `planner` → `developer` → `QM`.
- `planner` defines the plan and acceptance logic.
- `developer` implements the approved plan.
- `QM` verifies requirement fulfillment, checks regressions, and compares implementation with the planner output when available.

## Skill and prompt feedback
- Always explicitly report which `skill` was used.
- Always explicitly report which `prompt` was used.
- If no `skill` or `prompt` was used, state that explicitly.
- Keep this feedback short and consistent.

## Preferred response footer
- `Felhasznált skill: <path vagy nincs>`
- `Felhasznált prompt: <path vagy nincs>`
