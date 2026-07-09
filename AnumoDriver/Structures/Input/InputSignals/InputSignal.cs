using System.Text.Json.Serialization;

namespace AnumoDriver.Structures.INPUT;

public class InputSignal
{
    public enum Type
    {
        Key,
        //MultiKeys,
        Rotary
    }

    public virtual string Title { get; }

    public Input Input { get; set; }
    private protected InputAction? InputAction => Input?.InputAction;

    //public bool BlockNextInput { get; set; } = false;

    public InputSignal(Input input)
    {
        Input = input;
    }

    //NOTE: When a signal is received, ProcessSignal() will be called before Loop() call

    /// <summary>
    /// Called every time when a signal is received from the device.
    /// </summary>
    public virtual void ProcessSignal(byte[] signalData) { }

    /// <summary>
    /// Run every loop of the app.
    /// </summary>
    public virtual void Loop() { }
}

//public class InputSignal_MultiKeys : InputSignal
//{
//    public override string Title => "PRESS A BUNCH OF KEYS";
//    public enum PressType
//    {
//        /// <summary>
//        /// Action will do immediately when the key is pressed
//        /// </summary>
//        Pressed,

//        /// <summary>
//        /// Action will do when user is holding the key.
//        /// </summary>
//        Hold,
//    }
//    public PressType KeyPressType { get; set; }
//    public List<int> Keys { get; set; } = [];

//    private int _keyCheckIndex = 0;
    
//    public InputSignal_MultiKeys(Input input) : base(input)
//    {
//        Input = input;
//        Input.Parameters.Add("IsHolding", false);
//    }

//    public override void ProcessSignal(byte[] signalData)
//    {
//        //If received signal is PRESSED
//        if ((signalData[0] & 0b_1_0000) != 0)
//        {
//            //If received keycode is equal to current checking keycode,
//            //move on to the next keycode to check later
//            if (_keyCheckIndex < Keys.Count)
//            if (signalData[1] == Keys[_keyCheckIndex])
//                _keyCheckIndex++;
//        }
//        //If received signal is RELEASED
//        else if (_keyCheckIndex > 0)
//        {
//            //If one of the key in combo is released, reset the whole checking process
//            for (var i = 0; i < _keyCheckIndex; i++)
//            {
//                if (signalData[1] == Keys[i])
//                    _keyCheckIndex = 0;
//            }
//        }

        
//        if (_keyCheckIndex == Keys.Count && KeyPressType == PressType.Pressed)
//        {
//            if (KeyPressType == PressType.Pressed)
//                _keyCheckIndex = 0;
//        }
//    }
//}
