
namespace AnumoDriver.Structures.INPUT;

public class InputSignal_Rotary : InputSignal
{
    public InputSignal_Rotary(Input input) : base(input) { }

    public int RotaryIndex { get; set; }
    public enum RollType
    {
        Clockwise,
        CounterClockwise
    }
    public RollType RotaryRollType { get; set; }
    public override void ProcessSignal(byte[] signalData)
    {
        var getIndex = signalData[0] >> 4;
        if (getIndex != RotaryIndex)
            return;
        if ((RollType)signalData[1] == RotaryRollType)
            InputAction?.Invoke();
    }
}
