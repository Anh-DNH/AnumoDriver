using AnumoDriver.Controllers;

namespace AnumoDriver.Structures.INPUT;

public class Input
{
    public string Name { get; set; } = "New Input";
    public InputSignal.Type SignalType
    {
        get => _signalType;
        set
        {
            InputSignal = value switch
            {
                InputSignal.Type.Key => new InputSignal_Key(this),
                InputSignal.Type.Rotary => new InputSignal_Rotary(this),
                _ => throw new NotImplementedException($"InputSignal type {value} is not implemented.")
            };
            _signalType = value;
        }
    }
    private InputSignal.Type _signalType;
    public InputAction.Type ActionType
    {
        get => _actionType;
        set
        {
            InputAction = value switch
            {
                InputAction.Type.SimulateKeyboardKeys => new InputAction_SimulateKeyboardKeys(this),
                InputAction.Type.ClickMouseButton => new InputAction_ClickMouseButton(this),
                InputAction.Type.RollMouseWheel => new InputAction_RollMouseWheel(this),
                InputAction.Type.ChangeLayer => new InputAction_ChangeLayer(this),
                InputAction.Type.WriteText => new InputAction_WriteText(this),
                _ => throw new NotImplementedException($"InputAction type {value} is not implemented.")
            };
            _actionType = value;
        }
    }
    private InputAction.Type _actionType;

    public InputSignal InputSignal { get; set; }
    public InputAction InputAction { get; set; }
    public InputController InputController { get; private set; }
    public Dictionary<string, object> Parameters { get; set; } = [];

    public Input()
    {
        SignalType = InputSignal.Type.Key;
        ActionType = InputAction.Type.SimulateKeyboardKeys;
    }

    public Input(string name, InputSignal.Type signalType, InputAction.Type actionType)
    {
        Name = name;
        SignalType = signalType;
        ActionType = actionType;
    }

    public void Loop() =>
        InputSignal.Loop();

    /// <summary>
    /// Fires the input action. The fire method will depends on what type of input InputSignal is set to.
    /// </summary>
    public void ProcessToInvoke(byte[] signalData)
    {
        if ((InputSignal.Type)(signalData[0] & 0b_1111) != SignalType)
            return;

        InputSignal.ProcessSignal(signalData);
    }
}
