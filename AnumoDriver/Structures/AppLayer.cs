using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Windows.Media;

namespace AnumoDriver.Structures;

public class AppLayer : INotifyPropertyChanged
{

    public AppLayer(string name, string exePath)
    {
        _name = name;
        _exePath = exePath;
        InputLayers = [];
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

    [JsonIgnore]
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
    private int _inputLayerIndex;

    [JsonIgnore]
    public ObservableCollection<InputLayer> InputLayers { get; }


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
