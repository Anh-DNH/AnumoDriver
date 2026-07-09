using AnumoDriver.Structures;
using AnumoDriver.Structures.INPUT;
using AnumoDriver.Utilities;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows.Media.Imaging;

namespace AnumoDriver.Controllers;

public class InputController : INotifyPropertyChanged
{
    public enum InputType
    {
        Key,
        RotaryEncoder,
        //Joystick
    }

    private ObservableCollection<AppLayer> _appLayerList;

    public ObservableCollection<AppLayer> AppLayerList
    {
        get => _appLayerList;
        private set => SetProperty(ref _appLayerList, value);
    }

    private AppLayer _currentAppLayer;

    public AppLayer CurrentAppLayer
    {
        get => _currentAppLayer;
        private set => SetProperty(ref _currentAppLayer, value);
    }

    public InputLayer? CurrentInputLayer =>
        CurrentAppLayer?.InputLayers.Count > 0 ? CurrentAppLayer.InputLayers[CurrentAppLayer.InputLayerIndex] : null;

    public int InputLayerCount => CurrentAppLayer?.InputLayers.Count ?? 0;

    private string _fileLocation = "";

    public InputController()
    {
        AppLayerList = new ObservableCollection<AppLayer>();

        // Initialize file location to %localappdata%/AnumoDriver/set.json
        string appName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
        string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        _fileLocation = Path.Combine(localAppData, appName, "set.json");

        if (File.Exists(_fileLocation))
            LoadFile();
        else
            SetupDefault();
    }

    /// <summary>
    /// If no configuration file is found, this method will set up a default layout with one AppLayer and four InputLayer,
    /// along with predefined Inputs and their associated signals and actions.
    /// </summary>
    public void SetupDefault()
    {
        var defAppLayer = new AppLayer("Default", "")
        {
            IconImage = new BitmapImage(new Uri("pack://application:,,,/AnumoDriver;component/Resources/default.png")),
        };

        defAppLayer.InputLayers.Add(new InputLayer { Index = 0 });
        defAppLayer.InputLayers.Add(new InputLayer { Index = 1 });
        defAppLayer.InputLayers.Add(new InputLayer { Index = 2 });

        #region Layer 0
        var defInputLayer = defAppLayer.InputLayers[0];

        ObservableCollection<Input> defInput = [

            new Input {
                Name = "Layer 0",
                ActionType = InputAction.Type.ChangeLayer
            },

            new Input {
                Name = "Layer 1",
                ActionType = InputAction.Type.ChangeLayer
            },

            new Input {
                Name = "Layer 2",
                ActionType = InputAction.Type.ChangeLayer
            },

            //  --  --  --  --  --  --  --  --  --  --  --  --  --  //

            new Input {
                Name = "CONTROL"
            },

            new Input {
                Name = "SPACE"
            },

            new Input {
                Name = "SHIFT"
            },

            //  --  --  --  --  --  --  --  --  --  --  --  --  --  //

            new Input {
                Name = "CUT"
            },

            new Input {
                Name = "COPY"
            },

            new Input {
                Name = "PASTE"
            },

            //  --  --  --  --  --  --  --  --  --  --  --  --  --  //

            new Input {
                Name = "UNDO"
            },

            new Input {
                Name = "REDO"
            },

            new Input {
                Name = "SAVE"
            },

            ////  --  --  --  --  --  --  --  --  --  --  --  --  --  //

            new Input {
                Name = "MIDDLE MOUSE",
                ActionType = InputAction.Type.ClickMouseButton
            },

            new Input {
                Name = "SCROLL MOUSE UP",
                SignalType = InputSignal.Type.Rotary,
                ActionType = InputAction.Type.RollMouseWheel
            },

            new Input {
                Name = "SCROLL MOUSE DOWN",
                SignalType = InputSignal.Type.Rotary,
                ActionType = InputAction.Type.RollMouseWheel
            },

        ];
        defInputLayer.Inputs = defInput;

        //Layer 0
        defInput[0].InputSignal = new InputSignal_Key(defInput[0])
        {
            KeyPressType = InputSignal_Key.PressType.Press,
            Key = 0
        };
        defInput[0].InputAction = new InputAction_ChangeLayer(defInput[0])
        {
            LayerId = 0
        };
        //  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --
        //Layer 1
        defInput[1].InputSignal = new InputSignal_Key(defInput[1])
        {
            KeyPressType = InputSignal_Key.PressType.Press,
            Key = 1
        };
        defInput[1].InputAction = new InputAction_ChangeLayer(defInput[1])
        {
            LayerId = 1
        };
        //  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --
        //Layer 2
        defInput[2].InputSignal = new InputSignal_Key(defInput[2])
        {
            KeyPressType = InputSignal_Key.PressType.Press,
            Key = 2
        };
        defInput[2].InputAction = new InputAction_ChangeLayer(defInput[2])
        {
            LayerId = 2
        };
        //  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --
        //  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --
        //  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --
        //SHIFT Hold
        defInput[3].InputSignal = new InputSignal_Key(defInput[3])
        {
            KeyPressType = InputSignal_Key.PressType.Hold,
            Key = 3
        };
        defInput[3].InputAction = new InputAction_SimulateKeyboardKeys(defInput[3])
        {
            Keys = [VirtualKeyCode.SHIFT]
        };
        //  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --
        //SPACE Hold
        defInput[4].InputSignal = new InputSignal_Key(defInput[4])
        {
            KeyPressType = InputSignal_Key.PressType.Hold,
            Key = 6
        };
        defInput[4].InputAction = new InputAction_SimulateKeyboardKeys(defInput[4])
        {
            Keys = [VirtualKeyCode.SPACE]
        };
        //  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --
        //CTRL Hold
        defInput[5].InputSignal = new InputSignal_Key(defInput[5])
        {
            KeyPressType = InputSignal_Key.PressType.Hold,
            Key = 9
        };
        defInput[5].InputAction = new InputAction_SimulateKeyboardKeys(defInput[5])
        {
            Keys = [VirtualKeyCode.CONTROL]
        };
        //  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --
        //  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --
        //  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --
        //CTRL-X (Cut) Press
        defInput[6].InputSignal = new InputSignal_Key(defInput[6])
        {
            KeyPressType = InputSignal_Key.PressType.Press,
            Key = 4
        };
        defInput[6].InputAction = new InputAction_SimulateKeyboardKeys(defInput[6])
        {
            Keys = [VirtualKeyCode.CONTROL, VirtualKeyCode.VK_X]
        };
        //  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --
        //CTRL-C (Copy) Press
        defInput[7].InputSignal = new InputSignal_Key(defInput[7])
        {
            KeyPressType = InputSignal_Key.PressType.Hold,
            Key = 7
        };
        defInput[7].InputAction = new InputAction_SimulateKeyboardKeys(defInput[7])
        {
            Keys = [VirtualKeyCode.CONTROL, VirtualKeyCode.VK_C]
        };
        //  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --
        //CTRL-V (Paste) TapHold
        defInput[8].InputSignal = new InputSignal_Key(defInput[8])
        {
            KeyPressType = InputSignal_Key.PressType.TapHold,
            Key = 10
        };
        defInput[8].InputAction = new InputAction_SimulateKeyboardKeys(defInput[8])
        {
            Keys = [VirtualKeyCode.CONTROL, VirtualKeyCode.VK_V]
        };
        //  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --
        //  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --
        //  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --
        //CTRL-Z (Undo) TapHold
        defInput[9].InputSignal = new InputSignal_Key(defInput[9])
        {
            KeyPressType = InputSignal_Key.PressType.TapHold,
            Key = 5
        };
        defInput[9].InputAction = new InputAction_SimulateKeyboardKeys(defInput[9])
        {
            Keys = [VirtualKeyCode.CONTROL, VirtualKeyCode.VK_Z]
        };
        //  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --
        //CTRL-Y (Redo) TapHold
        defInput[10].InputSignal = new InputSignal_Key(defInput[10])
        {
            KeyPressType = InputSignal_Key.PressType.TapHold,
            Key = 8
        };
        defInput[10].InputAction = new InputAction_SimulateKeyboardKeys(defInput[10])
        {
            Keys = [VirtualKeyCode.CONTROL, VirtualKeyCode.VK_Y]
        };
        //  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --
        //CTRL-S (Save) Press
        defInput[11].InputSignal = new InputSignal_Key(defInput[11])
        {
            KeyPressType = InputSignal_Key.PressType.Press,
            Key = 11
        };
        defInput[11].InputAction = new InputAction_SimulateKeyboardKeys(defInput[11])
        {
            Keys = [VirtualKeyCode.CONTROL, VirtualKeyCode.VK_S]
        };
        //  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --
        //  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --
        //  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --
        //MOUSE3 Press
        defInput[12].InputSignal = new InputSignal_Key(defInput[12])
        {
            KeyPressType = InputSignal_Key.PressType.Hold,
            Key = 12
        };
        defInput[12].InputAction = new InputAction_ClickMouseButton(defInput[12])
        {
            ButtonId = 3,
        };
        //  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --
        //MOUSE WHEEL UP
        defInput[13].InputSignal = new InputSignal_Rotary(defInput[13])
        {
            RotaryIndex = 0,
            RotaryRollType = InputSignal_Rotary.RollType.Clockwise
        };
        defInput[13].InputAction = new InputAction_RollMouseWheel(defInput[13]) {
            Delta = 120
        };
        //  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --
        //MOUSE WHEEL DOWN
        defInput[14].InputSignal = new InputSignal_Rotary(defInput[14])
        {
            RotaryIndex = 0,
            RotaryRollType = InputSignal_Rotary.RollType.CounterClockwise
        };
        defInput[14].InputAction = new InputAction_RollMouseWheel(defInput[14]) {
            Delta = -120
        };
        #endregion

        #region Layer 1
        defInputLayer = defAppLayer.InputLayers[1];

        defInput = [

            new Input {
                Name = "Layer 0",
                ActionType = InputAction.Type.ChangeLayer
            },

            new Input {
                Name = "Layer 1",
                ActionType = InputAction.Type.ChangeLayer
            },

            new Input {
                Name = "Layer 2",
                ActionType = InputAction.Type.ChangeLayer
            },

            //  --  --  --  --  --  --  --  --  --  --  --  --  --  //

            new Input {
                Name = "Q"
            },

            new Input {
                Name = "W"
            },

            new Input {
                Name = "E"
            },

            //  --  --  --  --  --  --  --  --  --  --  --  --  --  //

            new Input {
                Name = "A"
            },

            new Input {
                Name = "S"
            },

            new Input {
                Name = "D"
            },

            //  --  --  --  --  --  --  --  --  --  --  --  --  --  //

            new Input {
                Name = "Z"
            },

            new Input {
                Name = "X"
            },

            new Input {
                Name = "C"
            },

            ////  --  --  --  --  --  --  --  --  --  --  --  --  --  //

            new Input {
                Name = "MIDDLE MOUSE",
                ActionType = InputAction.Type.ClickMouseButton
            },

            new Input {
                Name = "SCROLL MOUSE UP",
                SignalType = InputSignal.Type.Rotary,
                ActionType = InputAction.Type.RollMouseWheel
            },

            new Input {
                Name = "SCROLL MOUSE DOWN",
                SignalType = InputSignal.Type.Rotary,
                ActionType = InputAction.Type.RollMouseWheel
            },

        ];
        defInputLayer.Inputs = defInput;

        //Layer 0
        defInput[0].InputSignal = new InputSignal_Key(defInput[0])
        {
            KeyPressType = InputSignal_Key.PressType.Press,
            Key = 0
        };
        defInput[0].InputAction = new InputAction_ChangeLayer(defInput[0])
        {
            LayerId = 0
        };
        //  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --
        //Layer 1
        defInput[1].InputSignal = new InputSignal_Key(defInput[1])
        {
            KeyPressType = InputSignal_Key.PressType.Press,
            Key = 1
        };
        defInput[1].InputAction = new InputAction_ChangeLayer(defInput[1])
        {
            LayerId = 1
        };
        //  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --
        //Layer 2
        defInput[2].InputSignal = new InputSignal_Key(defInput[2])
        {
            KeyPressType = InputSignal_Key.PressType.Press,
            Key = 2
        };
        defInput[2].InputAction = new InputAction_ChangeLayer(defInput[2])
        {
            LayerId = 2
        };
        //  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --
        //  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --
        //  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --
        //Q TapHold
        defInput[3].InputSignal = new InputSignal_Key(defInput[3])
        {
            KeyPressType = InputSignal_Key.PressType.TapHold,
            Key = 3
        };
        defInput[3].InputAction = new InputAction_SimulateKeyboardKeys(defInput[3])
        {
            Keys = [VirtualKeyCode.VK_Q]
        };
        //  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --
        //W TapHold
        defInput[4].InputSignal = new InputSignal_Key(defInput[4])
        {
            KeyPressType = InputSignal_Key.PressType.TapHold,
            Key = 6
        };
        defInput[4].InputAction = new InputAction_SimulateKeyboardKeys(defInput[4])
        {
            Keys = [VirtualKeyCode.VK_W]
        };
        //  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --
        //E TapHold
        defInput[5].InputSignal = new InputSignal_Key(defInput[5])
        {
            KeyPressType = InputSignal_Key.PressType.TapHold,
            Key = 9
        };
        defInput[5].InputAction = new InputAction_SimulateKeyboardKeys(defInput[5])
        {
            Keys = [VirtualKeyCode.VK_E]
        };
        //  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --
        //  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --
        //  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --
        //A TapHold
        defInput[6].InputSignal = new InputSignal_Key(defInput[6])
        {
            KeyPressType = InputSignal_Key.PressType.TapHold,
            Key = 4
        };
        defInput[6].InputAction = new InputAction_SimulateKeyboardKeys(defInput[6])
        {
            Keys = [VirtualKeyCode.VK_A]
        };
        //  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --
        //S TapHold
        defInput[7].InputSignal = new InputSignal_Key(defInput[7])
        {
            KeyPressType = InputSignal_Key.PressType.TapHold,
            Key = 7
        };
        defInput[7].InputAction = new InputAction_SimulateKeyboardKeys(defInput[7])
        {
            Keys = [VirtualKeyCode.VK_S]
        };
        //  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --
        //D TapHold
        defInput[8].InputSignal = new InputSignal_Key(defInput[8])
        {
            KeyPressType = InputSignal_Key.PressType.TapHold,
            Key = 10
        };
        defInput[8].InputAction = new InputAction_SimulateKeyboardKeys(defInput[8])
        {
            Keys = [VirtualKeyCode.VK_D]
        };
        //  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --
        //  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --
        //  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --
        //Z TapHold
        defInput[9].InputSignal = new InputSignal_Key(defInput[9])
        {
            KeyPressType = InputSignal_Key.PressType.TapHold,
            Key = 5
        };
        defInput[9].InputAction = new InputAction_SimulateKeyboardKeys(defInput[9])
        {
            Keys = [VirtualKeyCode.VK_Z]
        };
        //  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --
        //X TapHold
        defInput[10].InputSignal = new InputSignal_Key(defInput[10])
        {
            KeyPressType = InputSignal_Key.PressType.TapHold,
            Key = 8
        };
        defInput[10].InputAction = new InputAction_SimulateKeyboardKeys(defInput[10])
        {
            Keys = [VirtualKeyCode.VK_X]
        };
        //  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --
        //C TapHold
        defInput[11].InputSignal = new InputSignal_Key(defInput[11])
        {
            KeyPressType = InputSignal_Key.PressType.TapHold,
            Key = 11
        };
        defInput[11].InputAction = new InputAction_SimulateKeyboardKeys(defInput[11])
        {
            Keys = [VirtualKeyCode.VK_C]
        };
        //  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --
        //  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --
        //  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --
        //MOUSE3 Press
        defInput[12].InputSignal = new InputSignal_Key(defInput[12])
        {
            KeyPressType = InputSignal_Key.PressType.Hold,
            Key = 12
        };
        defInput[12].InputAction = new InputAction_ClickMouseButton(defInput[12])
        {
            ButtonId = 3,
        };
        //  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --
        //MOUSE WHEEL UP
        defInput[13].InputSignal = new InputSignal_Rotary(defInput[13])
        {
            RotaryIndex = 0,
            RotaryRollType = InputSignal_Rotary.RollType.Clockwise
        };
        defInput[13].InputAction = new InputAction_RollMouseWheel(defInput[13])
        {
            Delta = 120
        };
        //  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --
        //MOUSE WHEEL DOWN
        defInput[14].InputSignal = new InputSignal_Rotary(defInput[14])
        {
            RotaryIndex = 0,
            RotaryRollType = InputSignal_Rotary.RollType.CounterClockwise
        };
        defInput[14].InputAction = new InputAction_RollMouseWheel(defInput[14])
        {
            Delta = -120
        };
        #endregion

        #region Layer 2
        defInputLayer = defAppLayer.InputLayers[2];

        defInput = [

            new Input {
                Name = "Layer 0",
                ActionType = InputAction.Type.ChangeLayer
            },

            new Input {
                Name = "Layer 1",
                ActionType = InputAction.Type.ChangeLayer
            },

            new Input {
                Name = "Layer 2",
                ActionType = InputAction.Type.ChangeLayer
            },

            //  --  --  --  --  --  --  --  --  --  --  --  --  --  //

            new Input {
                Name = "HELLO ABYSS"
            },

            new Input {
                Name = "ABC DEF GHI"
            },

            new Input {
                Name = "NERV"
            },

            //  --  --  --  --  --  --  --  --  --  --  --  --  --  //

            new Input {
                Name = "MIDDLE MOUSE",
                ActionType = InputAction.Type.ClickMouseButton
            },

            new Input {
                Name = "SCROLL MOUSE UP",
                SignalType = InputSignal.Type.Rotary,
                ActionType = InputAction.Type.RollMouseWheel
            },

            new Input {
                Name = "SCROLL MOUSE DOWN",
                SignalType = InputSignal.Type.Rotary,
                ActionType = InputAction.Type.RollMouseWheel
            },

        ];
        defInputLayer.Inputs = defInput;

        //Layer 0
        defInput[0].InputSignal = new InputSignal_Key(defInput[0])
        {
            KeyPressType = InputSignal_Key.PressType.Press,
            Key = 0
        };
        defInput[0].InputAction = new InputAction_ChangeLayer(defInput[0])
        {
            LayerId = 0
        };
        //  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --
        //Layer 1
        defInput[1].InputSignal = new InputSignal_Key(defInput[1])
        {
            KeyPressType = InputSignal_Key.PressType.Press,
            Key = 1
        };
        defInput[1].InputAction = new InputAction_ChangeLayer(defInput[1])
        {
            LayerId = 1
        };
        //  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --
        //Layer 2
        defInput[2].InputSignal = new InputSignal_Key(defInput[2])
        {
            KeyPressType = InputSignal_Key.PressType.Press,
            Key = 2
        };
        defInput[2].InputAction = new InputAction_ChangeLayer(defInput[2])
        {
            LayerId = 2
        };
        //  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --
        //  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --
        //  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --
        //HELLO ABYSS
        defInput[3].InputSignal = new InputSignal_Key(defInput[3])
        {
            KeyPressType = InputSignal_Key.PressType.Press,
            Key = 3
        };
        defInput[3].InputAction = new InputAction_WriteText(defInput[3])
        {
            Text = "HELLO ABYSS"
        };
        //  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --
        //ABC DEF GHI
        defInput[4].InputSignal = new InputSignal_Key(defInput[4])
        {
            KeyPressType = InputSignal_Key.PressType.Press,
            Key = 6
        };
        defInput[4].InputAction = new InputAction_WriteText(defInput[4])
        {
            Text = "ABC DEF GHI"
        };
        //  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --
        //NERV
        defInput[5].InputSignal = new InputSignal_Key(defInput[5])
        {
            KeyPressType = InputSignal_Key.PressType.Press,
            Key = 9
        };
        defInput[5].InputAction = new InputAction_WriteText(defInput[5])
        {
            Text = "God's in his heaven, all's right with the world!"
        };
        //  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --
        //  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --
        //  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --
        //MOUSE3 Press
        defInput[6].InputSignal = new InputSignal_Key(defInput[6])
        {
            KeyPressType = InputSignal_Key.PressType.Hold,
            Key = 12
        };
        defInput[6].InputAction = new InputAction_ClickMouseButton(defInput[6])
        {
            ButtonId = 3,
        };
        //  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --
        //MOUSE WHEEL UP
        defInput[7].InputSignal = new InputSignal_Rotary(defInput[7])
        {
            RotaryIndex = 0,
            RotaryRollType = InputSignal_Rotary.RollType.Clockwise
        };
        defInput[7].InputAction = new InputAction_RollMouseWheel(defInput[7])
        {
            Delta = 120
        };
        //  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --
        //MOUSE WHEEL DOWN
        defInput[8].InputSignal = new InputSignal_Rotary(defInput[8])
        {
            RotaryIndex = 0,
            RotaryRollType = InputSignal_Rotary.RollType.CounterClockwise
        };
        defInput[8].InputAction = new InputAction_RollMouseWheel(defInput[8])
        {
            Delta = -120
        };
        #endregion

        AppLayerList = [defAppLayer];
        CurrentAppLayer = defAppLayer;
    }

    /// <summary>
    /// Load data from a local saved json file and apply it into AppLayerList;
    /// </summary>
    public void LoadFile()
    {
        try
        {
            if (string.IsNullOrEmpty(_fileLocation) || !File.Exists(_fileLocation))
            {
                Trace.WriteLine($"File not found: {_fileLocation}. We start with default setup!");
                SetupDefault();
                return;
            }

            string jsonContent = File.ReadAllText(_fileLocation);
            var loadedLayers = JsonSerializer.Deserialize<List<AppLayer>>(jsonContent) ?? [];

            AppLayerList = new ObservableCollection<AppLayer>(loadedLayers);

            if (AppLayerList.Count > 0)
            {
                CurrentAppLayer = AppLayerList[0];
            }

            Trace.WriteLine($"Successfully loaded configuration from: {_fileLocation}");
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"Error loading file: {ex.Message}");
            AppLayerList = new ObservableCollection<AppLayer>();
        }
    }

    /// <summary>
    /// Save AppLayerList data into a json file
    /// </summary>
    public void SaveFile()
    {
        try
        {
            if (string.IsNullOrEmpty(_fileLocation))
            {
                Trace.WriteLine("File location not set. Cannot save file.");
                return;
            }

            string directory = Path.GetDirectoryName(_fileLocation);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            string jsonContent = JsonSerializer.Serialize(AppLayerList, options);
            File.WriteAllText(_fileLocation, jsonContent);

            Trace.WriteLine($"Successfully saved configuration to: {_fileLocation}");
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"Error saving file: {ex.Message}");
        }
    }

    /// <summary>
    /// Change the current AppLayer base on name
    /// </summary>
    public void ChangeAppLayer(string name)
    {
        if (AppLayerList.FirstOrDefault(e => e.Name == name) is { } a)
        {
            Trace.WriteLine($"Changed AppLayer to {a.Name}.");
            CurrentAppLayer = a;
        }
        else
        {
            //Trace.WriteLine($"AppLayer {name} not found in AppLayerList, use default layer");
            CurrentAppLayer = AppLayerList[0];
        }
    }

    public void ChangeInputLayer(int index)
    {
        if (CurrentAppLayer == null)
        {
            Trace.WriteLine("No current AppLayer selected.");
            return;
        }
        if (index < 0 || index >= CurrentAppLayer.InputLayers.Count)
        {
            Trace.WriteLine($"Invalid InputLayer index: {index}. Must be between 0 and {CurrentAppLayer.InputLayers.Count - 1}.");
            return;
        }

        CurrentAppLayer.InputLayerIndex = index;
        //Trace.WriteLine($"Changed InputLayer to index {index} for AppLayer {CurrentAppLayer.Name}.");
    }

    public void GetInput(byte[] data)
    {
        if (CurrentInputLayer == null)
            return;

        foreach (var input in CurrentInputLayer.Inputs)
        {
            input.ProcessToInvoke(data);
            //if (input.BlockNextInput)
            //    break;
        }
    }

    public void Loop()
    {
        if (CurrentInputLayer == null)
            return;

        foreach (var input in CurrentInputLayer.Inputs)
        {
            input.Loop();
        }
    }

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
}
