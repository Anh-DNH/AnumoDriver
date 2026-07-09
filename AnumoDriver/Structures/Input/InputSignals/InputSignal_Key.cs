using System.Text.Json.Serialization;

namespace AnumoDriver.Structures.INPUT;

public class InputSignal_Key : InputSignal
{
    public override string Title => "PRESS A KEY";
    public enum PressType
    {
        /// <summary>
        /// Action will do immediately when the key is pressed
        /// </summary>
        Press,

        /// <summary>
        /// Action will do immediately when the key is released
        /// </summary>
        Release,

        /// <summary>
        /// Action will do when user is holding the key.
        /// </summary>
        Hold,

        /// <summary>
        /// When user holds the key, the action will do right after pressed,
        /// then wait for a delay, then do the action until the key is released.
        /// </summary>
        TapHold,

        /// <summary>
        /// Action will do when user repeatedly press in a short moment.
        /// </summary>
        Multiple,

        /// <summary>
        /// Action will do when user press the key,
        /// and stop doing action when user press it again.
        /// </summary>
        Toggle
    }
    public PressType KeyPressType { get; set; }
    public int Key { get; set; }
    public int ClickCount { get; set; } = 1;

    [JsonIgnore]
    private bool _pressDown;

    [JsonIgnore]
    private int _pressDownCounter;
    [JsonIgnore]
    private int _pressClickCounter;
    [JsonIgnore]
    private int _pressClickDelay;

    [JsonIgnore]
    private int _invokeDelayer = 0;

    [JsonIgnore]
    private bool _toggleState = false;

    public InputSignal_Key(Input input) : base(input)
    {
        Input = input;
    }

    public override void ProcessSignal(byte[] signalData)
    {
        if (signalData[1] != Key)
            return;

        _pressDown = (signalData[0] & 0b_1_0000) != 0;

        switch (KeyPressType)
        {
            case PressType.Press:
                if (_pressDown)
                {
                    InputAction?.Invoke();
                    InputAction?.Reset();
                }
                return;

            case PressType.Release:
                if (!_pressDown)
                {
                    InputAction?.Invoke();
                    InputAction?.Reset();
                }
                return;

            case PressType.Hold:
            case PressType.TapHold:
                if (!_pressDown)
                    InputAction?.Reset();
                return;

            case PressType.Multiple:
                if (_pressDown && (_pressClickCounter == 0 || _pressClickDelay > 0))
                {
                    //Increase click counter until reached then
                    //invoke input action and reset click counter.
                    _pressClickCounter++;
                    if (_pressClickCounter >= ClickCount)
                    {
                        InputAction?.Invoke();
                        _pressClickCounter = 0;
                    }

                    _pressClickDelay = 1100;
                }
                return;

            case PressType.Toggle:
                if (_pressDown)
                    _toggleState = !_toggleState;
                if (!_toggleState)
                    InputAction?.Reset();
                return;

            default:
                return;
        }
    }

    public override void Loop()
    {
        //Counter for Tap-Hold feature.
        _pressDownCounter = _pressDown ? _pressDownCounter + 1 : 0;

        //Countdown for Multiple feature. If delaye counter is zero,
        //reset the click counter to zero as well.
        if (_pressClickDelay > 0)
        {
            _pressClickDelay--;
            if (_pressClickDelay == 0)
                _pressClickCounter = 0;
        }

        _invokeDelayer -= _invokeDelayer < 0 ? -101 : 1;

        switch (KeyPressType)
        {
            case PressType.Hold:
                if (_pressDown)
                {
                    if (_invokeDelayer == 0)
                        InputAction?.Invoke();
                }
                //else
                //    InputAction?.Reset();
                return;

            case PressType.TapHold:
                if (_pressDownCounter == 1 || (_pressDownCounter >= 1700 && _invokeDelayer == 0))
                    InputAction?.Invoke();
                return;

            case PressType.Toggle:
                if (_toggleState && _invokeDelayer == 0)
                    InputAction?.Invoke();
                return;
        }
    }
}

