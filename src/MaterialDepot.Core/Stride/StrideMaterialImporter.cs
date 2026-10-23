using System.Text;
using MaterialDepot.Core.Imaging;
using MaterialDepot.Core.Textures;

namespace MaterialDepot.Core.Stride;

public sealed class StrideMaterialImporter(IImageConverter imageConverter) : IStrideMaterialImporter
{
    private static readonly PbrMap[] importedMaps = [PbrMap.Albedo, PbrMap.Normal, PbrMap.Roughness, PbrMap.Metallic, PbrMap.AmbientOcclusion];
    private static readonly PbrMap[] requiredMaps = [PbrMap.Albedo, PbrMap.Normal, PbrMap.Roughness];
    private static readonly Encoding utf8WithoutBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    public StrideImportResult Import(StrideImportRequest request)
    {
        var missing = requiredMaps.Where(map => !request.Set.Maps.ContainsKey(map)).ToArray();
        if (missing.Length > 0)
        {
            throw new InvalidOperationException($"{request.Set.Name} lacks the maps {string.Join(", ", missing)}.");
        }

        var project = request.Project;
        var sourceDirectory = project.SourceTexturePath(request.TextureFolderName);
        var textureAssetDirectory = project.TextureAssetPath(request.TextureFolderName);
        Directory.CreateDirectory(sourceDirectory);
        Directory.CreateDirectory(textureAssetDirectory);

        var written = new List<string>();
        var textures = new Dictionary<PbrMap, StrideAssetReference>();
        foreach (var map in importedMaps)
        {
            if (!request.Set.Maps.TryGetValue(map, out var mapFile))
            {
                continue;
            }

            var pngPath = Path.Combine(sourceDirectory, mapFile.Stem + ".png");
            imageConverter.ConvertToPng(mapFile.Path, pngPath);
            written.Add(pngPath);

            var texturePath = Path.Combine(textureAssetDirectory, mapFile.Stem + ".sdtex");
            var textureId = ExistingId(texturePath) ?? Guid.NewGuid();
            var sourcePath = Path.GetRelativePath(textureAssetDirectory, pngPath);
            WriteAsset(texturePath, StrideAssetText.Texture(textureId, sourcePath, map));
            written.Add(texturePath);

            textures[map] = new StrideAssetReference(textureId, AssetUrl(project, texturePath));
        }

        var materialPath = project.MaterialPath(request.MaterialDirectory, request.MaterialName);
        var existingMaterialId = ExistingId(materialPath);
        var materialId = request.MaterialId ?? existingMaterialId ?? Guid.NewGuid();
        Directory.CreateDirectory(Path.GetDirectoryName(materialPath)!);
        WriteAsset(materialPath, StrideAssetText.Material(materialId, textures, request.UvScale));
        written.Add(materialPath);

        return new StrideImportResult(materialPath, materialId, existingMaterialId.HasValue, written);
    }

    private static Guid? ExistingId(string assetPath) =>
        File.Exists(assetPath) ? StrideAssetText.ReadId(File.ReadAllText(assetPath)) : null;

    private static string AssetUrl(StrideProject project, string assetPath)
    {
        var relative = Path.GetRelativePath(project.AssetsPath, assetPath).Replace('\\', '/');
        return relative[..^Path.GetExtension(relative).Length];
    }

    private static void WriteAsset(string path, string text) => File.WriteAllText(path, text, utf8WithoutBom);
}
