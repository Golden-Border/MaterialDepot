using System.Globalization;
using System.IO;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaterialDepot.App.Services;
using MaterialDepot.Core.Stride;
using MaterialDepot.Core.Textures;

namespace MaterialDepot.App.ViewModels;

/// <summary>
/// The import dialog: names, tiling and an optional material Id for one set and one project.
/// </summary>
public sealed partial class ImportViewModel : ObservableObject
{
    private readonly IStrideMaterialImporter importer;
    private readonly IDialogService dialogs;
    private bool updatingUvScale;

    public ImportViewModel(TextureSet set, StrideProject project, IStrideMaterialImporter importer, IDialogService dialogs)
    {
        this.importer = importer;
        this.dialogs = dialogs;
        Set = set;
        Project = project;
        MaterialName = set.Name;
        TextureFolderName = set.Name;
        MaterialDirectory = project.MaterialDirectory;
        var tileSize = TextureSetNaming.TileSizeMeters(set.Albedo?.BaseName ?? string.Empty);
        TileSizeText = tileSize is { } size ? Format(size) : string.Empty;
        MetersPerUvUnitText = "1";
        Maps = MapLabels.Of(set.Maps.Keys.Where(map => map != PbrMap.Height));
        UpdateUvScale();
    }

    public TextureSet Set { get; }

    public StrideProject Project { get; }

    public IReadOnlyList<string> Maps { get; }

    public ImageSource? Thumbnail { get; init; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MaterialNote))]
    [NotifyCanExecuteChangedFor(nameof(ImportCommand))]
    public partial string MaterialName { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ImportCommand))]
    public partial string TextureFolderName { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MaterialNote))]
    public partial string MaterialDirectory { get; set; }

    /// <summary>
    /// The edge length of one texture tile in metres, as textures.com states it (20 x 20 cm gives 0.2).
    /// </summary>
    [ObservableProperty]
    public partial string TileSizeText { get; set; }

    /// <summary>
    /// How many metres one UV unit covers on the meshes the material goes on.
    /// </summary>
    [ObservableProperty]
    public partial string MetersPerUvUnitText { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ImportCommand))]
    public partial string UvScaleText { get; set; } = "1";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ImportCommand))]
    public partial string MaterialIdText { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ImportCommand))]
    public partial bool IsImporting { get; set; }

    [ObservableProperty]
    public partial StrideImportResult? Result { get; set; }

    /// <summary>
    /// Set when the dialog should close; <see langword="true"/> after an import.
    /// </summary>
    [ObservableProperty]
    public partial bool? DialogResult { get; set; }

    public string MaterialNote
    {
        get
        {
            var path = MaterialPath();
            return path is not null && File.Exists(path)
                ? $"{MaterialName}.sdmat gibt es schon: Die Id bleibt, der Inhalt wird neu geschrieben."
                : string.Empty;
        }
    }

    [RelayCommand(CanExecute = nameof(CanImport))]
    private async Task ImportAsync()
    {
        var request = new StrideImportRequest(
            Set,
            Project,
            TextureFolderName.Trim(),
            MaterialName.Trim(),
            MaterialDirectory.Trim(),
            ParseNumber(UvScaleText)!.Value,
            ParseMaterialId());
        IsImporting = true;
        try
        {
            Result = await Task.Run(() => importer.Import(request));
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or InvalidOperationException or ImageMagick.MagickException)
        {
            dialogs.ShowError($"Die Übernahme ist fehlgeschlagen:\n{exception.Message}");
        }
        finally
        {
            IsImporting = false;
        }
    }

    [RelayCommand]
    private void Close() => DialogResult = Result is not null;

    private bool CanImport() =>
        !IsImporting
        && IsName(MaterialName)
        && IsName(TextureFolderName)
        && ParseNumber(UvScaleText) is > 0
        && (string.IsNullOrWhiteSpace(MaterialIdText) || ParseMaterialId() is not null);

    partial void OnTileSizeTextChanged(string value) => UpdateUvScale();

    partial void OnMetersPerUvUnitTextChanged(string value) => UpdateUvScale();

    private void UpdateUvScale()
    {
        if (updatingUvScale || ParseNumber(TileSizeText) is not > 0 || ParseNumber(MetersPerUvUnitText) is not > 0)
        {
            return;
        }

        updatingUvScale = true;
        UvScaleText = Format(StrideImportRequest.UvScaleFor(ParseNumber(TileSizeText)!.Value, ParseNumber(MetersPerUvUnitText)!.Value));
        updatingUvScale = false;
    }

    private Guid? ParseMaterialId() => Guid.TryParse(MaterialIdText.Trim(), out var id) ? id : null;

    private string? MaterialPath() =>
        IsName(MaterialName) ? Project.MaterialPath(MaterialDirectory ?? string.Empty, MaterialName.Trim()) : null;

    private static bool IsName(string? name) =>
        !string.IsNullOrWhiteSpace(name) && name.IndexOfAny(Path.GetInvalidFileNameChars()) < 0;

    /// <summary>
    /// Accepts a decimal comma as well as a point, since the user types German numbers.
    /// </summary>
    private static double? ParseNumber(string? text) =>
        double.TryParse(text?.Trim().Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out var value) ? value : null;

    private static string Format(double value) => value.ToString("0.####", CultureInfo.CurrentCulture);
}
