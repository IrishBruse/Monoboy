namespace Monoboy.Dap;

using System.Text.Json.Nodes;

using Monoboy;
using Monoboy.Debug;

/// <summary>Debug Adapter Protocol session for Monoboy.</summary>
public sealed class DapAdapter
{
    readonly DebugSession _session = new();
    int _seq;
    bool _linesStartAt1 = true;

    public bool ExitRequested { get; private set; }

    public Emulator? Emulator => _session.Emulator;

    public List<JsonObject> Handle(JsonObject request, Func<bool> interrupt)
    {
        string command = request["command"]?.GetValue<string>() ?? "";
        int requestSeq = request["seq"]?.GetValue<int>() ?? 0;
        JsonObject args = request["arguments"] as JsonObject ?? new JsonObject();

        switch (command)
        {
            case "initialize":
            return Initialize(requestSeq, args);
            case "launch":
            return Launch(requestSeq, args);
            case "configurationDone":
            return ConfigurationDone(requestSeq, interrupt);
            case "setBreakpoints":
            return SetBreakpoints(requestSeq, args);
            case "setExceptionBreakpoints":
            return [Ok(requestSeq, command, new JsonObject { ["breakpoints"] = new JsonArray() })];
            case "threads":
            return Threads(requestSeq);
            case "stackTrace":
            return StackTrace(requestSeq, args);
            case "scopes":
            return Scopes(requestSeq);
            case "variables":
            return Variables(requestSeq, args);
            case "source":
            return Source(requestSeq, args);
            case "gblFrame":
            return Frame(requestSeq);
            case "gblInput":
            _session.SetButton(args["button"]?.GetValue<string>(), args["pressed"]?.GetValue<bool>() ?? false);
            return [Ok(requestSeq, command)];
            case "continue":
            return Run(requestSeq, command, () => _session.Continue(interrupt), interrupt);
            case "next":
            return Run(requestSeq, command, () => _session.StepOver(interrupt), interrupt);
            case "stepIn":
            return Run(requestSeq, command, () => _session.StepInto(interrupt), interrupt);
            case "stepOut":
            return Run(requestSeq, command, () => _session.StepOut(interrupt), interrupt);
            case "pause":
            return [Ok(requestSeq, command)];
            case "disconnect":
            case "terminate":
            ExitRequested = true;
            return [Ok(requestSeq, command), Event("terminated")];
            default:
            return [Ok(requestSeq, command)];
        }
    }

    List<JsonObject> Initialize(int requestSeq, JsonObject args)
    {
        if (args["linesStartAt1"] is JsonValue lines && lines.TryGetValue(out bool startAt1))
        {
            _linesStartAt1 = startAt1;
        }

        var body = new JsonObject
        {
            ["supportsConfigurationDoneRequest"] = true,
            ["supportsTerminateRequest"] = true,
        };
        return [Ok(requestSeq, "initialize", body), Event("initialized")];
    }

    List<JsonObject> Launch(int requestSeq, JsonObject args)
    {
        string? program = args["program"]?.GetValue<string>();
        if (string.IsNullOrWhiteSpace(program))
        {
            return [Fail(requestSeq, "launch", "launch requires program")];
        }

        bool stopOnEntry = true;
        if (args["stopOnEntry"] is JsonValue stop && stop.TryGetValue(out bool parsedStop))
        {
            stopOnEntry = parsedStop;
        }

        string? cwd = args["cwd"]?.GetValue<string>();
        if (!_session.Launch(program, cwd, stopOnEntry, out string error))
        {
            var failed = new List<JsonObject>();
            if (_session.Log.Length > 0)
            {
                failed.Add(Output(_session.Log + "\n"));
            }

            failed.Add(Fail(requestSeq, "launch", error));
            return failed;
        }

        var messages = new List<JsonObject> { Ok(requestSeq, "launch") };
        if (_session.Log.Length > 0)
        {
            messages.Add(Output(_session.Log + "\n"));
        }

        return messages;
    }

    List<JsonObject> ConfigurationDone(int requestSeq, Func<bool> interrupt)
    {
        var messages = new List<JsonObject> { Ok(requestSeq, "configurationDone") };
        if (!_session.IsLaunched)
        {
            messages.Add(Event("terminated"));
            ExitRequested = true;
            return messages;
        }

        if (_session.StopOnEntry)
        {
            messages.Add(Stopped(StopReason.Entry));
            return messages;
        }

        StopReason reason = _session.Continue(interrupt);
        messages.Add(Stopped(reason));
        return messages;
    }

    List<JsonObject> SetBreakpoints(int requestSeq, JsonObject args)
    {
        JsonObject source = args["source"] as JsonObject ?? new JsonObject();
        string? path = source["path"]?.GetValue<string>();
        int sourceReference = 0;
        if (source["sourceReference"] is JsonValue reference && reference.TryGetValue(out int parsedReference))
        {
            sourceReference = parsedReference;
        }

        var lines = new List<int>();
        if (args["breakpoints"] is JsonArray breakpoints)
        {
            foreach (JsonNode? item in breakpoints)
            {
                if (item is JsonObject obj && obj["line"] is JsonValue line && line.TryGetValue(out int parsedLine))
                {
                    lines.Add(FromClient(parsedLine));
                }
            }
        }

        IReadOnlyList<LineBreakpoint> bound = _session.SetBreakpoints(path, sourceReference, lines);
        var body = new JsonArray();
        foreach (LineBreakpoint breakpoint in bound)
        {
            var entry = new JsonObject
            {
                ["verified"] = breakpoint.Verified,
                ["line"] = ToClient(breakpoint.Line),
            };
            body.Add(entry);
        }

        return [Ok(requestSeq, "setBreakpoints", new JsonObject { ["breakpoints"] = body })];
    }

    List<JsonObject> Threads(int requestSeq)
    {
        var threads = new JsonArray
        {
            new JsonObject { ["id"] = DebugSession.ThreadId, ["name"] = "main" },
        };
        return [Ok(requestSeq, "threads", new JsonObject { ["threads"] = threads })];
    }

    List<JsonObject> StackTrace(int requestSeq, JsonObject args)
    {
        int startFrame = 0;
        if (args["startFrame"] is JsonValue start && start.TryGetValue(out int parsedStart))
        {
            startFrame = parsedStart;
        }

        var frames = new JsonArray();
        if (startFrame == 0 && _session.IsLaunched)
        {
            SourceFrame frame = _session.CurrentFrame();
            var entry = new JsonObject
            {
                ["id"] = 1,
                ["name"] = frame.Name,
                ["line"] = ToClient(frame.Line),
                ["column"] = 1,
            };
            JsonObject? source = SourceObject(frame);
            if (source != null)
            {
                entry["source"] = source;
            }

            frames.Add(entry);
        }

        return [Ok(requestSeq, "stackTrace", new JsonObject
        {
            ["stackFrames"] = frames,
            ["totalFrames"] = frames.Count,
        })];
    }

    List<JsonObject> Scopes(int requestSeq)
    {
        var scopes = new JsonArray
        {
            new JsonObject
            {
                ["name"] = "Registers",
                ["presentationHint"] = "registers",
                ["variablesReference"] = 1,
                ["expensive"] = false,
            },
        };
        return [Ok(requestSeq, "scopes", new JsonObject { ["scopes"] = scopes })];
    }

    List<JsonObject> Variables(int requestSeq, JsonObject args)
    {
        int reference = 0;
        if (args["variablesReference"] is JsonValue value && value.TryGetValue(out int parsed))
        {
            reference = parsed;
        }

        var variables = new JsonArray();
        if (reference == 1)
        {
            foreach (DebugVariable variable in _session.Registers())
            {
                variables.Add(new JsonObject
                {
                    ["name"] = variable.Name,
                    ["value"] = variable.Value,
                    ["variablesReference"] = 0,
                });
            }
        }

        return [Ok(requestSeq, "variables", new JsonObject { ["variables"] = variables })];
    }

    List<JsonObject> Source(int requestSeq, JsonObject args)
    {
        int reference = 0;
        if (args["sourceReference"] is JsonValue value && value.TryGetValue(out int parsed))
        {
            reference = parsed;
        }

        if (!_session.TryReadSource(reference, out _, out string text))
        {
            return [Fail(requestSeq, "source", "Source is not available")];
        }

        return [Ok(requestSeq, "source", new JsonObject
        {
            ["content"] = text,
            ["mimeType"] = "text/x-gbl",
        })];
    }

    List<JsonObject> Frame(int requestSeq)
    {
        if (!_session.TryGetFrame(out int width, out int height, out string data))
        {
            return [Fail(requestSeq, "gblFrame", "No emulator is running")];
        }

        return [Ok(requestSeq, "gblFrame", new JsonObject
        {
            ["width"] = width,
            ["height"] = height,
            ["data"] = data,
        })];
    }

    List<JsonObject> Run(int requestSeq, string command, Func<StopReason> run, Func<bool> interrupt)
    {
        if (!_session.IsLaunched)
        {
            return [Fail(requestSeq, command, "No program is launched")];
        }

        if (interrupt())
        {
            return [Ok(requestSeq, command), Stopped(StopReason.Pause)];
        }

        StopReason reason = run();
        JsonObject? body = command == "continue"
            ? new JsonObject { ["allThreadsContinued"] = true }
            : null;
        return [Ok(requestSeq, command, body), Stopped(reason)];
    }

    JsonObject? SourceObject(SourceFrame frame)
    {
        if (frame.SourceReference > 0)
        {
            return new JsonObject
            {
                ["name"] = frame.Path ?? "source",
                ["sourceReference"] = frame.SourceReference,
            };
        }

        if (frame.Path == null)
        {
            return null;
        }

        return new JsonObject
        {
            ["name"] = Path.GetFileName(frame.Path),
            ["path"] = frame.Path,
        };
    }

    JsonObject Stopped(StopReason reason)
    {
        string name = reason switch
        {
            StopReason.Entry => "entry",
            StopReason.Breakpoint => "breakpoint",
            StopReason.Step => "step",
            _ => "pause",
        };
        return Event("stopped", new JsonObject
        {
            ["reason"] = name,
            ["threadId"] = DebugSession.ThreadId,
            ["allThreadsStopped"] = true,
        });
    }

    JsonObject Output(string text)
    {
        return Event("output", new JsonObject
        {
            ["category"] = "console",
            ["output"] = text,
        });
    }

    JsonObject Event(string name, JsonObject? body = null)
    {
        var message = new JsonObject
        {
            ["seq"] = ++_seq,
            ["type"] = "event",
            ["event"] = name,
        };
        if (body != null)
        {
            message["body"] = body;
        }

        return message;
    }

    JsonObject Ok(int requestSeq, string command, JsonObject? body = null)
    {
        var message = new JsonObject
        {
            ["seq"] = ++_seq,
            ["type"] = "response",
            ["request_seq"] = requestSeq,
            ["success"] = true,
            ["command"] = command,
        };
        if (body != null)
        {
            message["body"] = body;
        }

        return message;
    }

    JsonObject Fail(int requestSeq, string command, string message)
    {
        return new JsonObject
        {
            ["seq"] = ++_seq,
            ["type"] = "response",
            ["request_seq"] = requestSeq,
            ["success"] = false,
            ["command"] = command,
            ["message"] = message,
        };
    }

    int ToClient(int line) => _linesStartAt1 ? line : Math.Max(0, line - 1);

    int FromClient(int line) => _linesStartAt1 ? line : line + 1;
}
