---
name: Commit and push
description: Build the project, stage all changes, commit with a provided message, and push the current branch. Also use this when the user says short prompts like "commit and push", "git push", or "push it".
---

# Commit and push

Use this skill when the user wants to publish local changes only after a successful build.

Typical short trigger phrases:

- `commit and push`
- `git push`
- `push it`
- `commitold és pushold`
- `toljad fel gitre`

## Required input

- `commit_message`: the commit message to use.

## Steps

1. Run `.github/skills/validated-build-commit-push/build-commit-push.ps1 -CommitMessage "<commit_message>"` from the repository root.
2. If the build fails, stop immediately and show the compiler errors from the command output, including file and line information.
3. If the build succeeds, stage all changes in the repository.
4. Commit the staged changes with the provided commit message.
5. Push the current branch. If the branch has no upstream yet, create it on `origin`.

## Notes

- Do not continue to commit or push when the build fails.
- All tracked, untracked, and deleted files are staged automatically before commit.
