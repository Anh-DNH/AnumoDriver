using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnumoDriver.Controllers;

public class GlobalTick
{
    public const float FrameRate = 24;

    public event Action OnTick;

    private CancellationTokenSource _cts = new();
    private bool _isStarted = false;

    public void Start()
    {
        if (_isStarted) return;
        _isStarted = true;

        // Fire and forget the background loop
        Task.Run(() => RunLoop(_cts.Token));
    }

    private async Task RunLoop(CancellationToken token)
    {
        // 1000ms / ticks
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(1000 / FrameRate));

        while (await timer.WaitForNextTickAsync(token))
        {
            // Execute logic
            OnTick?.Invoke();
        }
    }
}
