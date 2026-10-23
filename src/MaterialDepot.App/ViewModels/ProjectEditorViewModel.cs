using CommunityToolkit.Mvvm.ComponentModel;
using MaterialDepot.Core.Stride;

namespace MaterialDepot.App.ViewModels;

/// <summary>
/// One Stride project in the settings, editable.
/// </summary>
public sealed partial class ProjectEditorViewModel(StrideProject project) : ObservableObject
{
    [ObservableProperty]
    public partial string Name { get; set; } = project.Name;

    [ObservableProperty]
    public partial string RootDirectory { get; set; } = project.RootDirectory;

    [ObservableProperty]
    public partial string SourceTextureDirectory { get; set; } = project.SourceTextureDirectory;

    [ObservableProperty]
    public partial string AssetsDirectory { get; set; } = project.AssetsDirectory;

    [ObservableProperty]
    public partial string TextureAssetDirectory { get; set; } = project.TextureAssetDirectory;

    [ObservableProperty]
    public partial string MaterialDirectory { get; set; } = project.MaterialDirectory;

    public StrideProject ToProject() =>
        new(Name.Trim(), RootDirectory.Trim(), SourceTextureDirectory.Trim(), AssetsDirectory.Trim(), TextureAssetDirectory.Trim(), MaterialDirectory.Trim());
}
