# Material Depot

A Windows desktop tool for PBR texture sets: it picks up new downloads, keeps them in a library folder, shows that library with albedo previews, and imports a set into a Stride project as ready texture and material assets.

## Build

```bash
dotnet build ./MaterialDepot.slnx
dotnet run --project src/MaterialDepot.App
```

Requires the .NET 10 SDK on Windows.
