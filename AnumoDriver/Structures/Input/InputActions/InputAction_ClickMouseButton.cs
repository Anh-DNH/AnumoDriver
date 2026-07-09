using AnumoDriver.Utilities;

namespace AnumoDriver.Structures.INPUT;

public class InputAction_ClickMouseButton : InputAction
{
    private InputSimulator _inputSimulator = new();

    public InputAction_ClickMouseButton(Input input) : base(input) { }

    public int ButtonId { get; set; } = 1;

    public override void Invoke()
    {
        _inputSimulator.MouseDown(ButtonId);
    }

    public override void Reset()
    {
        _inputSimulator.MouseUp(ButtonId);
    }
}

