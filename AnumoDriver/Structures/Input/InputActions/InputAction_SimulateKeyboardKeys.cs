using AnumoDriver.Utilities;

namespace AnumoDriver.Structures.INPUT;

public class InputAction_SimulateKeyboardKeys : InputAction
{
    private InputSimulator _inputSimulator = new();

    public InputAction_SimulateKeyboardKeys(Input input) : base(input) { }

    public List<VirtualKeyCode> Keys { get; set; } = [];

    public override void Invoke()
    {
        if (Keys == null || Keys.Count == 0)
            return;

        //PressKey();
        foreach (VirtualKeyCode key in Keys)
            _inputSimulator.KeyDown(key);
    }

    public override void Reset()
    {
        foreach (VirtualKeyCode key in Keys)
            _inputSimulator.KeyUp(key);
    }
}

