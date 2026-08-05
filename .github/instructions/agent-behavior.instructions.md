---
applyTo: "**/*"
---

# Agent Behavior Instructions

## Routing
- When the user says `CP`, the request must be handled by the `git-expert` agent.
- For `CP`, prefer using `.github/skills/validated-build-commit-push/skill.md` when that workflow matches the request and its prerequisites are acceptable.

## Skill and prompt feedback
- Always explicitly report which `skill` was used.
- Always explicitly report which `prompt` was used.
- If no `skill` or `prompt` was used, state that explicitly.
- Keep this feedback short and consistent.

## Preferred response footer
- `Felhasznált skill: <path vagy nincs>`
- `Felhasznált prompt: <path vagy nincs>`
