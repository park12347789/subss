# Project Guidelines

- Unity project work should follow OOP and keep responsibilities separated by component.
- Scripts related to interfaces must start with `I`.
- When something does not work, analyze the root cause first instead of adding reinforcement patches.
- Prefer prefab, hierarchy, and direct scene placement before code-only setup.
- If generated images are needed, create PNG assets under `Assets/` and wire them into prefabs or scene objects.
- Do not add validation rules or validation tooling unless the user explicitly approves that after the feature is finished.
- In this Codex environment, do not claim live Unity Editor validation unless MCP/editor access actually confirms it. Otherwise, report checks as code, reference, prefab, scene, and inspector-connection review.
