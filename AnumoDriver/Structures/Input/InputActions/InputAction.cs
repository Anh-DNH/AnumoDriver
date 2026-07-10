namespace AnumoDriver.Structures.INPUT;

public class InputAction
{
    public enum Type
    {
        SimulateKeyboardKeys,

        //MoveMouse,
        ClickMouseButton,
        RollMouseWheel,

        WriteText,
        ChangeLayer
    }

    public Input Input { get; set; }

    public InputAction(Input input)
    {
        Input = input;
    }

    /// <summary>
    /// Called when you want to invoke this action.
    /// </summary>
    public virtual void Invoke() { }

    /// <summary>
    /// Called when you want to reset some properties of the action
    /// </summary>
    public virtual void Reset() { }

}
