---
name: Commit and push
description: Format and build the project, stage all changes, commit with a provided message, and push the current branch. Also use this when the user says short prompts like "commit and push", "git push", or "push it".
---

# Commit and push

Use this skill when the user wants to publish local changes only after a successful format and build.

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
2. Run `dotnet format .\Postgirl.csproj`.
3. Build the project.
4. If formatting or build fails, stop immediately and show the command output, including compiler errors with file and line information when available.
5. If formatting and build succeed, stage all changes in the repository, including tracked files that were deleted from the file system.
6. Commit the staged changes with the provided commit message.
7. Push the current branch. If the branch has no upstream yet, create it on `origin`.
8. Return a summary of how many files were modified, created, and deleted.

## Notes

- Do not continue to commit or push when formatting or build fails.
- All tracked, untracked, and deleted files are staged automatically before commit.
- Deleted tracked files are removed from Git automatically before commit.
