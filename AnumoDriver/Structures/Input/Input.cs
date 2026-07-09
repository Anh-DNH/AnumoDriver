using AnumoDriver.Controllers;

namespace AnumoDriver.Structures.INPUT;

public class Input
{
    public string Name { get; set; } = "";
    public InputSignal.Type SignalType { get; set; } = InputSignal.Type.Key;
    public InputAction.Type ActionType { get; set; } = InputAction.Type.SimulateKeyboardKeys;
    public InputSignal InputSignal { get; set; }
    public InputAction InputAction { get; set; }
    public InputController InputController { get; private set; }
    public Dictionary<string, object> Parameters { get; set; } = [];

    public Input() { }

    public Input(string name, InputSignal.Type signalType, InputAction.Type actionType)
    {
        Name = name;
        SignalType = signalType;
        ActionType = actionType;

        InputSignal = SignalType switch
        {
            InputSignal.Type.Key => new InputSignal_Key(this),
            InputSignal.Type.Rotary => new InputSignal_Rotary(this),
            _ => throw new NotImplementedException($"InputSignal type {signalType} is not implemented.")
        };
        InputAction = ActionType switch
        {
            InputAction.Type.SimulateKeyboardKeys => new InputAction_SimulateKeyboardKeys(this),
            InputAction.Type.RollMouseWheel => new InputAction_RollMouseWheel(this),
            InputAction.Type.ChangeLayer => new InputAction_ChangeLayer(this),
            InputAction.Type.WriteText => new InputAction_WriteText(this),
            _ => throw new NotImplementedException($"InputAction type {actionType} is not implemented.")
        };
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
