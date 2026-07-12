using AnumoDriver.Structures.INPUT;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AnumoDriver;

public class InputLayer : INotifyPropertyChanged
{
    public int Index
    {
        get => _index;
        set => SetProperty(ref _index, value);
    }
    private int _index;

    public ObservableCollection<Input> Inputs { get; set; }

    public InputLayer()
    {
        Inputs = new ObservableCollection<Input>();
    }



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
