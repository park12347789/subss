# MCP Setup

- Unity MCP package: `com.anklebreaker.unity-mcp`.
- Package source: `https://github.com/AnkleBreaker-Studio/unity-mcp-plugin.git`.
- Codex local MCP server already exists as `unityAnkleBreaker` and runs `anklebreaker-unity-mcp@latest`.
- This project reserves Unity MCP bridge port `7894`.
- `Assets/_Project/Editor/CodexMcpPortBootstrap.cs` keeps the Unity-side bridge on `7894`.
- After this package is resolved by Unity, reopen or refresh the project if Codex still reports zero Unity instances.
