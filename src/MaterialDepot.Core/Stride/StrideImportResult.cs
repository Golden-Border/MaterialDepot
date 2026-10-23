namespace MaterialDepot.Core.Stride;

/// <summary>
/// What an import wrote.
/// </summary>
/// <param name="MaterialPath">The <c>.sdmat</c>.</param>
/// <param name="MaterialId">Its Id.</param>
/// <param name="ReplacedMaterial">Whether a material of that name existed and was rewritten.</param>
/// <param name="WrittenFiles">Every file written, the material last.</param>
public sealed record StrideImportResult(string MaterialPath, Guid MaterialId, bool ReplacedMaterial, IReadOnlyList<string> WrittenFiles);
