using AnumoDriver.Structures.INPUT;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AnumoDriver.Structures;

[JsonConverter(typeof(AppLayerJsonConverter))]
public class AppLayer : INotifyPropertyChanged
{
    public AppLayer() { }

    public AppLayer(string name, string exePath)
    {
        _name = name;
        _exePath = exePath;
    }

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }
    private string _name;

    public string ExePath
    {
        get => _exePath;
        set => SetProperty(ref _exePath, value);
    }
    private string _exePath;

    public ImageSource IconImage
    {
        get => _iconImage;
        set => SetProperty(ref _iconImage, value);
    }
    private ImageSource _iconImage;

    public int InputLayerIndex
    {
        get => _inputLayerIndex;
        set => SetProperty(ref _inputLayerIndex, value);
    }
    private int _inputLayerIndex = 0;

    public ObservableCollection<InputLayer> InputLayers { get; } = [];

    #region INotifyPropertyChanged stuff
    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
    #endregion
}

public class AppLayerJsonConverter : JsonConverter<AppLayer>
{
    public override AppLayer? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using (JsonDocument doc = JsonDocument.ParseValue(ref reader))
        {
            var root = doc.RootElement;
            var appLayer = new AppLayer();

            if (root.TryGetProperty("Name", out var name))
                appLayer.Name = name.ToString();

            if (root.TryGetProperty("Path", out var exePart))
            {
                appLayer.ExePath = exePart.ToString();
                appLayer.IconImage = ExtractIconFromFile(appLayer.ExePath);
            }

            //if (root.TryGetProperty("InputLayerIndex", out var inputLayerIndex))
            //    appLayer.InputLayerIndex = inputLayerIndex.GetInt32();

            if (root.TryGetProperty("InputLayers", out var inputLayers))
            {
                var list = inputLayers.Deserialize<ObservableCollection<InputLayer>>();
                if (list == null)
                    throw new InvalidOperationException("Something went wrong while trying to convert InputLayer list");

                foreach (var inputLayer in list)
                    appLayer.InputLayers.Add(inputLayer);
            }

            return appLayer;
        }

    }

    public override void Write(Utf8JsonWriter writer, AppLayer value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WriteString("Name", value.Name);
        writer.WriteString("Path", value.ExePath);
        //writer.WriteNumber("InputLayerIndex", value.InputLayerIndex);

        writer.WritePropertyName("InputLayers");
        writer.WriteRawValue(JsonSerializer.Serialize(value.InputLayers, options));

        writer.WriteEndObject();
    }

    private ImageSource ExtractIconFromFile(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return new BitmapImage(new Uri("pack://application:,,,/AnumoDriver;component/Resources/default.png"));

        using (System.Drawing.Icon icon = System.Drawing.Icon.ExtractAssociatedIcon(filePath))
        {
            if (icon != null)
            {
                return System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                    icon.Handle,
                    System.Windows.Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
        }

        return null;
    }
}