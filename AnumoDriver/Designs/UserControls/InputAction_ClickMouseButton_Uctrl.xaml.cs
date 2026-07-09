using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace AnumoDriver.Designs.UserControls;

/// <summary>
/// Interaction logic for InputAction_ClickMouseButton_Uctrl.xaml
/// </summary>
public partial class InputAction_ClickMouseButton_Uctrl : UserControl
{
    public ObservableCollection<string> ButtonName { get; set; } = [
        "Left click",
        "Right click",
        "Middle click",
        "MOUSE4",
        "MOUSE5"
    ];

    public InputAction_ClickMouseButton_Uctrl()
    {
        InitializeComponent();
    }
}
