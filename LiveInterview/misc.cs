using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveInterview
{
    class FormKeyboard : GlobalKeyListener
    {
        private readonly HashSet<int> _pressedKeys = new();
        private Task pressRun;
        private CancellationTokenSource _cts;

        private async Task BeforeTriggerAsync(int key, CancellationToken token)
        {
            try
            {
                await Task.Delay(1000, token); // Delay can be adjusted
                Trace.WriteLine(string.Join(",", _pressedKeys));

                if (_pressedKeys.SetEquals(new[] { (int)Keys.LControlKey, (int)Keys.LShiftKey }))
                {
                    _onTrigger?.Invoke(key); // 🔥 Trigger
                }
                else
                {
                    _pressedKeys.Clear(); // Reset if other keys are pressed
                }
            }
            catch (TaskCanceledException)
            {
                // Ignore cancellation
            }
            finally
            {
                pressRun = null;
            }
        }

        protected override void OnKeyDownPress(int key)
        {
            _pressedKeys.Add(key);

            // Cancel previous task if running
            if (pressRun != null && !pressRun.IsCompleted)
            {
                _cts?.Cancel();
            }

            _cts = new CancellationTokenSource();
            pressRun = BeforeTriggerAsync(key, _cts.Token);
        }
        protected override void OnKeyUp(int key)
        {
        }


    }
}
