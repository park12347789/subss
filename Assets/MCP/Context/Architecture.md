# Architecture Notes

- Main project documentation lives under `Documentation/`.
- Existing gameplay scripts are split between `Assets/02.Scripts/Gameplay/` and `Assets/Scripts/`.
- Interface examples already follow the `I*` naming rule under `Assets/02.Scripts/Gameplay/Combat/`.
- Existing scene and prefab surfaces include `Assets/01.Scenes/`, `Assets/Scenes/`, `Assets/03.Prefab/`, and `Assets/Prefabs/`.
- Prefer serialized inspector references, prefab links, and scene hierarchy ownership for Unity-facing wiring.
- Starter Assets and third-party content should remain isolated under their existing folders unless a task explicitly requires reorganizing them.
