# Material Depot

A Windows desktop tool for PBR texture sets. It does three things:

1. Finds new texture sets in your download folder and copies or moves their original files into a library folder, one folder per set.
2. Shows the library as a grid of sets with the albedo as preview.
3. Imports a set into a [Stride](https://www.stride3d.net/) project: 8-bit PNGs next to the project's other source textures, one `.sdtex` per map and a ready `.sdmat`.

Which files are texture maps is decided by file name patterns in the settings: regular expressions with the named groups `name` (the set) and `map`, and optionally `res`, plus the names each map goes by (`Diffuse`, `Color` → albedo and so on). The defaults recognise textures.com (`TCom_<name>_<res>_<map>.tif`), Poly Haven (`<name>_<map>_<res>.<ext>`), ambientCG and Substance exports; the settings tab has a field to try a file name against your patterns.

The import is deterministic: the same source always gives the same bytes, and an asset that already exists keeps its `Id`, so models that reference it stay intact. Game Studio does not reload asset files changed on disk, so reload the project after an import.

## Build

```bash
dotnet build ./MaterialDepot.slnx
dotnet test ./MaterialDepot.slnx --no-build
dotnet run --project src/MaterialDepot.App
```

Requires the .NET 10 SDK on Windows. The UI is in German.

## Notes

- This is a personal project, shared as is, without support.
- Textures are not part of this repository and must not be added to it. Tests create their images themselves.
- The code was written with the help of an AI assistant.

## License

[MIT](LICENSE)
