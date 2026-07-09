using AnumoDriver.Utilities;

namespace AnumoDriver.Structures.INPUT;

public class InputAction_RollMouseWheel : InputAction
{
    private InputSimulator _inputSimulator = new();

    public InputAction_RollMouseWheel(Input input) : base(input) { }

    public int Side { get; set; }
    public int Delta { get; set; }

    public override void Invoke()
    {
        _inputSimulator.MouseScroll((Side > 0 ? 1 : -1) * Delta);
    }
}

