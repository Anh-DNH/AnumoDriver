using AnumoDriver.Controllers;
using AnumoDriver.Structures;
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
    private InputLayer _selectedInputLayer;

    public InputEditorWindow(InputController inputController)
    {
        _inputController = inputController;
        InitializeComponent();
        DataContext = _inputController;
    }

    public void Window_Loaded(object sender, RoutedEventArgs e)
    {
        AppLayerItemsControl.ItemsSource = _inputController.AppLayerList;
        InputLayerItemsControl.ItemsSource = _inputController.CurrentAppLayer.InputLayers;
        InputsItemsControl.ItemsSource = _inputController.CurrentInputLayer?.Inputs;
    }

    private void AppIcon_Click(object sender, RoutedEventArgs e)
    {
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

                if (newAppLayer.IconImage != null)
                    AppIcon.Source = newAppLayer.IconImage;

                Trace.WriteLine($"Added new app layer: {appName} from {exePath}");
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Failed to extract icon from {exePath}: {ex.Message}");
            }
        }
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

    private void RemoveAppButton_Click(object sender, RoutedEventArgs e)
    {
        if (_inputController.CurrentAppLayer == null)
        {
            MessageBox.Show("Please select an app layer to remove.", "No Selection");
            return;
        }

        if (_inputController.CurrentAppLayer == _inputController.AppLayerList[0])
        {
            MessageBox.Show("Cannot remove the default layer.", "WARNING");
            return;
        }

        _inputController.AppLayerList.Remove(_inputController.CurrentAppLayer);
        Trace.WriteLine($"Removed app layer: {_inputController.CurrentAppLayer.Name}");
    }

    private void InputLayer_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is InputLayer selectedInputLayer)
        {
            SelectInputLayer(selectedInputLayer);
        }
    }

    private void SelectInputLayer(InputLayer inputLayer)
    {
        if (inputLayer == null)
            return;

        _selectedInputLayer = inputLayer;

        // Find the index of the input layer in the current app layer
        int layerIndex = _inputController.CurrentAppLayer.InputLayers.IndexOf(inputLayer);
        if (layerIndex >= 0)
        {
            _inputController.ChangeInputLayer(layerIndex);
            LayerButtonText.Text = $"Layer: {inputLayer.Index + 1}";
            Trace.WriteLine($"Selected input layer: {inputLayer.Index}");
        }
    }

    private void AddInputLayerButton_Click(object sender, RoutedEventArgs e)
    {
        if (_inputController.CurrentAppLayer == null)
        {
            MessageBox.Show("Please select an app layer first.", "No App Selected");
            return;
        }

        try
        {
            int newIndex = _inputController.CurrentAppLayer.InputLayers.Count;
            var newInputLayer = new InputLayer { Index = newIndex };

            _inputController.CurrentAppLayer.InputLayers.Add(newInputLayer);

            SelectInputLayer(newInputLayer);

            Trace.WriteLine($"Added new input layer with index: {newIndex}");
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

        if (_inputController.CurrentAppLayer.InputLayers.Count <= 1)
        {
            MessageBox.Show("Cannot remove the last input layer. At least one layer must remain.", "Cannot Remove");
            return;
        }

        try
        {
            int removedIndex = _selectedInputLayer.Index;
            _inputController.CurrentAppLayer.InputLayers.Remove(_selectedInputLayer);

            // Re-index remaining layers
            for (int i = 0; i < _inputController.CurrentAppLayer.InputLayers.Count; i++)
            {
                _inputController.CurrentAppLayer.InputLayers[i].Index = i;
            }

            // Select the first remaining layer
            if (_inputController.CurrentAppLayer.InputLayers.Count > 0)
            {
                SelectInputLayer(_inputController.CurrentAppLayer.InputLayers[0]);
            }

            Trace.WriteLine($"Removed input layer at index: {removedIndex}");
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"Failed to remove input layer: {ex.Message}");
            MessageBox.Show($"Failed to remove input layer: {ex.Message}", "Error");
        }
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
