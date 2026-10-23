namespace MaterialDepot.Core.Stride;

/// <summary>
/// How a Stride asset refers to another one: its <c>Id</c> and its URL, the path inside the asset folder without extension.
/// </summary>
public sealed record StrideAssetReference(Guid Id, string Url)
{
    public override string ToString() => $"{Id}:{Url}";
}
