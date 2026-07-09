using AnumoDriver.Controllers;
using Microsoft.Extensions.DependencyInjection;

namespace AnumoDriver.Structures.INPUT;

public class InputAction_ChangeLayer : InputAction
{
    private InputController? _inputCtrler => App.Service?.GetService<InputController>();

    public int LayerId { get; set; }

    public InputAction_ChangeLayer(Input input) : base(input) { }

    public override void Invoke()
    {
        _inputCtrler?.ChangeInputLayer(LayerId);
    }
}
