using AnumoDriver.Utilities;

namespace AnumoDriver.Structures.INPUT;

public class InputAction_WriteText : InputAction
{
    private InputSimulator _inputSimulator = new();
    public InputAction_WriteText(Input input) : base(input) { }
    public string Text { get; set; } = "";
    public override void Invoke()
    {
        _inputSimulator.WriteText(Text);
    }
}
