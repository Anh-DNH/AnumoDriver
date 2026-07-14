using AnumoDriver.Controllers;
using AnumoDriver.Structures;
using AnumoDriver.Structures.INPUT;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AnumoDriver.Designs;

/// <summary>
/// Interaction logic for InputEditorWindow.xaml
/// </summary>
public partial class InputEditorWindow : Window
{
    private readonly InputController _inputController;
    private AppLayer _selectedAppLayer;
    private InputLayer _selectedInputLayer;

    private List<InputDataEditorWindow> _inputDataEditors = [];

    public InputEditorWindow(InputController inputController)
    {
        _inputController = inputController;
        InitializeComponent();
        DataContext = _inputController;
    }

    public void Window_Loaded(object sender, RoutedEventArgs e)
    {
        AppLayerItemsControl.ItemsSource = _inputController.AppLayerList;
        SelectAppLayer(_inputController.AppLayerList[0]);
    }



    private void AppIcon_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is AppLayer selectedAppLayer)
            SelectAppLayer(selectedAppLayer);
    }

    private void AppAddButton_Click(object sender, RoutedEventArgs e)
    {
        OpenFileDialog openFileDialog = new OpenFileDialog
        {
            Filter = "Executable Files (*.exe)|*.exe|All Files (*.*)|*.*",
            Title = "Select Application Executable"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            string exePath = openFileDialog.FileName;
            string appName = Path.GetFileNameWithoutExtension(exePath);

            if (_inputController.AppLayerList.FirstOrDefault(e => e.Name == appName) is { })
            {
                MessageBox.Show("This application is already added.", "Duplicated Application");
                return;
            }

            try
            {
                var newAppLayer = new AppLayer(appName, exePath)
                {
                    IconImage = ExtractIconFromFile(exePath),
                };

                var newInputLayer = new InputLayer { Index = 0 };
                newAppLayer.InputLayers.Add(newInputLayer);

                _inputController.AppLayerList.Add(newAppLayer);
                SelectAppLayer(newAppLayer);

                Trace.WriteLine($"Added new app layer: {appName} from {exePath}");
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Failed to extract icon from {exePath}: {ex.Message}");
            }
        }
    }

    private void RemoveAppButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedAppLayer == null)
        {
            MessageBox.Show("Please select an app layer to remove.", "No Selection");
            return;
        }

        if (_selectedAppLayer == _inputController.AppLayerList[0])
        {
            MessageBox.Show("Cannot remove the default layer.", "WARNING");
            return;
        }

        var i = _inputController.AppLayerList.IndexOf(_selectedAppLayer);
        _inputController.AppLayerList.RemoveAt(i);

        SelectAppLayer(_inputController.AppLayerList[i == _inputController.AppLayerList.Count ? i - 1 : i]);

        Trace.WriteLine($"Removed app layer: {_selectedAppLayer.Name}");

    }



    private void InputLayer_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is InputLayer selectedInputLayer)
        {
            SelectInputLayer(selectedInputLayer);
        }
    }

    private void AddInputLayerButton_Click(object sender, RoutedEventArgs e)
    {
        var newInputLayer = new InputLayer
        {
            Index = _selectedAppLayer.InputLayers.Count
        };

        _selectedAppLayer.InputLayers.Add(newInputLayer);
        SelectInputLayer(newInputLayer);

        Trace.WriteLine($"Added new input layer with index: {_selectedInputLayer.Index}");
    }

    private void RemoveInputLayerButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedInputLayer == null)
        {
            MessageBox.Show("Please select an input layer to remove.", "No Selection");
            return;
        }

        if (_selectedAppLayer.InputLayers.Count == 1)
        {
            MessageBox.Show("Cannot remove the last input layer.\nAt least one layer must remain.", "Cannot Remove");
            return;
        }

        var i = _selectedInputLayer.Index;
        _selectedAppLayer.InputLayers.RemoveAt(i);

        // Re-index remaining layers
        if (_selectedAppLayer.InputLayers.Count > 1)
        {
            for (int ii = i; ii < _selectedAppLayer.InputLayers.Count; ii++)
                _selectedAppLayer.InputLayers[ii].Index = ii;
        }
        SelectInputLayer(_selectedAppLayer.InputLayers[i == _selectedAppLayer.InputLayers.Count ? i - 1 : i]);

        Trace.WriteLine($"Removed input layer at index: {i}");
    }



    private void SelectAppLayer(AppLayer selectedAppLayer)
    {
        _selectedAppLayer = selectedAppLayer;
        SelectInputLayer(_selectedAppLayer.InputLayers[0]);

        AppNameText.Text = _selectedAppLayer.Name;

        InputLayerItemsControl.ItemsSource = _selectedAppLayer.InputLayers;
    }

    private void SelectInputLayer(InputLayer selectedInputLayer)
    {
        _selectedInputLayer = selectedInputLayer;

        LayerButtonText.Text = $"Layer: {_selectedInputLayer.Index}";
        
        InputsItemsControl.ItemsSource = _selectedInputLayer.Inputs;
    }

    private void AddInputButton_Click(object sender, RoutedEventArgs e)
    {
        var newInput = new Input();

        _selectedInputLayer.Inputs.Add(newInput);
    }

    private void InputInfoUctrl_MoveUpInput(object sender, RoutedEventArgs e)
    {
        var index = GetIndex(e);
        if (index == 0)
            return;
        _selectedInputLayer.Inputs.Move(index, index - 1);
    }

    private void InputInfoUctrl_MoveDownInput(object sender, RoutedEventArgs e)
    {
        var index = GetIndex(e);
        if (index == _selectedInputLayer.Inputs.Count - 1)
            return;
        _selectedInputLayer.Inputs.Move(index, index + 1);
    }

    private void InputInfoUctrl_RemoveInput(object sender, RoutedEventArgs e)
    {
        var index = GetIndex(e);
        _selectedInputLayer.Inputs.RemoveAt(index);
    }

    private void InputInfoUctrl_OpenInputDataEditor(InputDataEditorWindow obj) =>
        _inputDataEditors.Add(obj);

    private void InputInfoUctrl_CloseInputDataEditor(InputDataEditorWindow obj) =>
        _inputDataEditors.Remove(obj);

    private void SaveButton_Click(object sender, RoutedEventArgs e) =>
        _inputController.SaveFile();

    private void SaveAsButton_Click(object sender, RoutedEventArgs e)
    {
        var openDialog = new SaveFileDialog
        {
            Filter = "JSON file (*.json)|*.json|All Files (*.*)|*.*",
            Title = "Select save location"
        };

        if (openDialog.ShowDialog() == true)
            _inputController.SaveFile(openDialog.FileName);
    }

    private void LoadButton_Click(object sender, RoutedEventArgs e)
    {
        var openDialog = new OpenFileDialog
        {
            Filter = "JSON file (*.json)|*.json|All Files (*.*)|*.*",
            Title = "Select file"
        };

        if (openDialog.ShowDialog() == true)
            _inputController.LoadFile(openDialog.FileName);

        AppLayerItemsControl.ItemsSource = _inputController.AppLayerList;
        SelectAppLayer(_inputController.AppLayerList[0]);
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e) =>
        Close();



    private ImageSource ExtractIconFromFile(string filePath)
    {
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

    private int GetIndex(RoutedEventArgs e)
    {
        //Try to get FrameworkElement from UserControl (if that makes sense)
        FrameworkElement fe = e.OriginalSource as FrameworkElement;
        if (fe == null)
            return -1;

        //Get Input from FrameworkElement's DataContext
        var item = fe.DataContext as Input;
        if (item == null)
            return -1;

        //Get the index
        return _selectedInputLayer.Inputs.IndexOf(item);
    }

    protected override void OnClosed(EventArgs e)
    {
        while (_inputDataEditors.Count > 0)
            _inputDataEditors[0].Close();

        base.OnClosed(e);
    }

}
