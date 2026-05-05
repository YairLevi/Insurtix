# Commit

Use Conventional Commits. Subject ≤50 chars.

## Format

```
<type>(<scope>): <what changed>

<why, if not obvious>
```

## Types

- `feat` — new feature
- `fix` — bug fix
- `refactor` — restructure, no behavior change
- `test` — add/fix tests
- `chore` — deps, config, tooling
- `docs` — documentation only

## Rules

- Subject: imperative, lowercase, no period. "add user login" not "Added user login."
- Scope: optional, short noun. `feat(auth):`, `fix(policy-list):`
- Body: only when WHY is non-obvious. Skip if subject says it all.
- No "WIP", no "fix stuff", no "changes".

## Examples

```
feat(claims): add bulk export to CSV
fix(auth): redirect to login on 401 instead of blank screen
refactor(policy): extract premium calc into service
test(quotes): cover edge case when coverage limit is zero
```
