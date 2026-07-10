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

    public InputEditorWindow(InputController inputController)
    {
        _inputController = inputController;
        InitializeComponent();
        DataContext = _inputController;
    }

    public void Window_Loaded(object sender, RoutedEventArgs e)
    {
        _selectedAppLayer = _inputController.AppLayerList[0];
        _selectedInputLayer = _selectedAppLayer.InputLayers[0];

        SelectAppLayer(_inputController.AppLayerList[0]);

        AppLayerItemsControl.ItemsSource = _inputController.AppLayerList;
        InputLayerItemsControl.ItemsSource = _selectedAppLayer.InputLayers;
        InputsItemsControl.ItemsSource = _inputController.CurrentInputLayer?.Inputs;
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
        try
        {
            var newInputLayer = new InputLayer
            {
                Index = _selectedAppLayer.InputLayers.Count
            };

            _selectedAppLayer.InputLayers.Add(newInputLayer);
            SelectInputLayer(newInputLayer);

            Trace.WriteLine($"Added new input layer with index: {_selectedInputLayer.Index}");
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"Failed to add input layer: {ex.Message}");
            MessageBox.Show($"Failed to add input layer: {ex.Message}", "Error");
        }
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

        try
        {
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
        catch (Exception ex)
        {
            Trace.WriteLine($"Failed to remove input layer: {ex.Message}");
            MessageBox.Show($"Failed to remove input layer: {ex.Message}", "Error");
        }
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

    private void ConnectButton_Copy1_Click(object sender, RoutedEventArgs e)
    {

    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
    {

    }

}
