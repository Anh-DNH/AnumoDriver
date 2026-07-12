using AnumoDriver.Controllers;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AnumoDriver.Structures.INPUT;

[JsonConverter(typeof(InputJsonConverter))]
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

    //public Dictionary<string, object> Parameters { get; set; } = [];

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

public class InputJsonConverter : JsonConverter<Input>
{

#pragma warning disable CS8601 // Possible null reference assignment.
    public override Input? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using (JsonDocument doc = JsonDocument.ParseValue(ref reader))
        {
            var root = doc.RootElement;
            var input = new Input();

            if (root.TryGetProperty("Name", out var nameElement))
                input.Name = nameElement.ToString();

            if (root.TryGetProperty("SignalType", out var sTypeElement))
                input.SignalType = (InputSignal.Type)sTypeElement.GetInt32();

            if (root.TryGetProperty("ActionType", out var aTypeElement))
                input.ActionType = (InputAction.Type)aTypeElement.GetInt32();

            if (root.TryGetProperty("InputSignal", out var iSignalElement))
            {
                string productJson = iSignalElement.GetRawText();

                input.InputSignal = input.SignalType switch
                {
                    InputSignal.Type.Key => JsonSerializer.Deserialize<InputSignal_Key>(productJson, options),
                    InputSignal.Type.Rotary => JsonSerializer.Deserialize<InputSignal_Rotary>(productJson, options),
                    _ => throw new JsonException("Unknown InputSignal type occured")
                };
            }

            if (root.TryGetProperty("InputAction", out var iActionElement))
            {
                string productJson = iActionElement.GetRawText();

                input.InputAction = input.ActionType switch
                {
                    InputAction.Type.SimulateKeyboardKeys => JsonSerializer.Deserialize<InputAction_SimulateKeyboardKeys>(productJson, options),
                    InputAction.Type.ClickMouseButton => JsonSerializer.Deserialize<InputAction_ClickMouseButton>(productJson, options),
                    InputAction.Type.RollMouseWheel => JsonSerializer.Deserialize<InputAction_RollMouseWheel>(productJson, options),
                    InputAction.Type.ChangeLayer => JsonSerializer.Deserialize<InputAction_ChangeLayer>(productJson, options),
                    InputAction.Type.WriteText => JsonSerializer.Deserialize<InputAction_WriteText>(productJson, options),
                    _ => throw new JsonException("Unknown InputAction type occured")
                };
            }

            return input;
        }
    }
#pragma warning restore CS8601 // Possible null reference assignment.

    public override void Write(Utf8JsonWriter writer, Input value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WriteString("Name", value.Name);
        writer.WriteNumber("SignalType", (int)value.SignalType);
        writer.WriteNumber("ActionType", (int)value.ActionType);

        writer.WritePropertyName("InputSignal");
        writer.WriteRawValue(
            value.SignalType switch
            {
                InputSignal.Type.Key => JsonSerializer.Serialize((InputSignal_Key)value.InputSignal, options),
                InputSignal.Type.Rotary => JsonSerializer.Serialize((InputSignal_Rotary)value.InputSignal, options),
                _ => throw new JsonException("Unknown InputSignal type occured")
            }
        );

        writer.WritePropertyName("InputAction");
        writer.WriteRawValue(
            value.ActionType switch
            {
                InputAction.Type.SimulateKeyboardKeys => JsonSerializer.Serialize((InputAction_SimulateKeyboardKeys)value.InputAction, options),
                InputAction.Type.ClickMouseButton => JsonSerializer.Serialize((InputAction_ClickMouseButton)value.InputAction, options),
                InputAction.Type.RollMouseWheel => JsonSerializer.Serialize((InputAction_RollMouseWheel)value.InputAction, options),
                InputAction.Type.ChangeLayer => JsonSerializer.Serialize((InputAction_ChangeLayer)value.InputAction, options),
                InputAction.Type.WriteText => JsonSerializer.Serialize((InputAction_WriteText)value.InputAction, options),
                _ => throw new JsonException("Unknown InputAction type occured")
            }
        );

        writer.WriteEndObject();
        //writer.WriteStringValue(value.Name);
    }
}