# CLAUDE.md

Material Depot is a Windows desktop tool (WPF, .NET 10, `net10.0-windows`) for PBR texture sets. It does three things:

1. It finds new texture sets in the download folder and copies or moves their original files into the library folder (one folder per set).
2. It shows the library as a grid of sets with their albedo as preview.
3. It imports a set into a Stride project: 8-bit PNGs next to the project's other source textures, one `.sdtex` per map and a ready `.sdmat`.

The sets come from textures.com (`TCom_<name>_<res>_<map>.tif`), some from Poly Haven (`<name>_<map>_<res>.<ext>`). Which files count as maps is not hard-coded: `DepotSettings` holds file name regexes (groups `name`, `map`, optional `res`; matched against the name without extension) and the map name aliases, `MapFileParser` applies them, and `DefaultTextureNaming` holds the shipped defaults that an old `settings.json` without them falls back to. The files are licensed downloads. **Never commit a texture from textures.com or any other store to this repo**, not even as a test fixture: tests create their images themselves.

## Build and Test

```bash
dotnet build ./MaterialDepot.slnx
dotnet test ./MaterialDepot.slnx --no-build
```

## Layout

- `src/MaterialDepot.Core`: everything that is not UI (scanning, archiving, the Stride import, settings). No WPF reference.
- `src/MaterialDepot.App`: the WPF UI, MVVM with CommunityToolkit.Mvvm, services wired in `App.xaml.cs`.
- `tests/MaterialDepot.Core.Tests`: xUnit, Shouldly, Moq.

## Conventions

- C# conventions: `.editorconfig` plus `Directory.Build.props` (`TreatWarningsAsErrors`, `EnforceCodeStyleInBuild`) make style violations build errors. Private fields are `camelCase` without underscore.
- Tests: AAA with `// Arrange` / `// Act` / `// Assert`, the SUT and mocks as `private readonly` fields set in the constructor.
- Code, comments and commits in English; the UI text is German.
- Conventional Commits, `type: short summary`, max 50 characters.

## The Stride import

The output has to match what the existing materials in existing Stride projects look like, byte for byte where Stride itself would write the same:

- A PNG is written with depth 8 (Stride has no sRGB variant for 16-bit formats, so a 16-bit albedo renders too bright) and without date and time chunks, so the same source always gives the same bytes.
- `.sdtex`: albedo `!ColorTextureType` with the magenta `ColorKeyColor`, normal `!NormalMapTextureType {}`, roughness, metallic and ao `!GrayscaleTextureType {}`. Compression and mipmaps stay at Stride's defaults.
- `.sdmat`: normal map, glossiness from roughness with `Invert: true`, diffuse, metalness (only with a metallic map), occlusion with ambient and cavity map from ao and `DirectLightingFactor` 0 (only with an ao map), no emissive. `UVScale` defaults to 1; the real-world tiling (metres per UV unit ÷ tile size) is only an opt-in in the dialog, because it can be too fine to read from the game camera.
- An asset that already exists keeps its `Id`, so models that reference it stay intact. The `.sdmat` is rewritten as a whole.
- Stride YAML assets are ASCII with CRLF and a trailing newline.

Game Studio does not reload asset files changed on disk; the project has to be reloaded after an import.
