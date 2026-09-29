using System.Collections.Concurrent;
using System.Text.Json.Nodes;

using Monoboy.Dap;

Console.SetOut(Console.Error);
Stream stdout = Console.OpenStandardOutput();
DapServer.Run(Console.OpenStandardInput(), stdout);

static class DapServer
{
    public static void Run(Stream input, Stream output)
    {
        var inbox = new BlockingCollection<JsonObject>();
        var reader = new Thread(() =>
        {
            try
            {
                while (DapIO.TryRead(input, out JsonObject message))
                {
                    inbox.Add(message);
                }
            }
            catch (IOException)
            {
            }
            finally
            {
                inbox.CompleteAdding();
            }
        })
        {
            IsBackground = true,
            Name = "dap-read",
        };
        reader.Start();

        var adapter = new DapAdapter();
        while (!adapter.ExitRequested)
        {
            int wait = ScreenWindow.IsOpen ? 16 : Timeout.Infinite;
            if (!inbox.TryTake(out JsonObject? request, wait))
            {
                if (inbox.IsCompleted)
                {
                    break;
                }

                ScreenWindow.Pump(adapter.Emulator);
                continue;
            }

            if (request == null)
            {
                continue;
            }

            Dispatch(adapter, output, inbox, request);
            if (adapter.Emulator != null)
            {
                ScreenWindow.EnsureOpen();
                ScreenWindow.Pump(adapter.Emulator);
            }
        }

        ScreenWindow.Close();
    }

    static void Dispatch(DapAdapter adapter, Stream output, BlockingCollection<JsonObject> inbox, JsonObject request)
    {
        var deferred = new Queue<JsonObject>();
        var control = new ControlBox();
        foreach (JsonObject message in adapter.Handle(request, () => TakeControl(adapter, output, inbox, deferred, control)))
        {
            DapIO.Write(output, message);
        }

        if (control.Message != null && !adapter.ExitRequested)
        {
            foreach (JsonObject message in adapter.Handle(control.Message, () => false))
            {
                DapIO.Write(output, message);
            }
        }

        while (deferred.Count > 0 && !adapter.ExitRequested)
        {
            Dispatch(adapter, output, inbox, deferred.Dequeue());
        }
    }

    static bool TakeControl(
        DapAdapter adapter,
        Stream output,
        BlockingCollection<JsonObject> inbox,
        Queue<JsonObject> deferred,
        ControlBox control)
    {
        if (control.Message != null)
        {
            return true;
        }

        while (inbox.TryTake(out JsonObject? next))
        {
            if (next == null)
            {
                continue;
            }

            string command = next["command"]?.GetValue<string>() ?? "";
            if (command is "pause" or "disconnect" or "terminate")
            {
                control.Message = next;
                return true;
            }

            if (command is "gblFrame" or "gblInput")
            {
                foreach (JsonObject message in adapter.Handle(next, () => false))
                {
                    DapIO.Write(output, message);
                }

                continue;
            }

            deferred.Enqueue(next);
        }

        ScreenWindow.Pump(adapter.Emulator);
        return false;
    }

    sealed class ControlBox
    {
        public JsonObject? Message;
    }
}
