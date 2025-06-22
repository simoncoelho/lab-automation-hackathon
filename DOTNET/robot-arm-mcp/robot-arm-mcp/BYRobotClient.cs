// BYRobotClient.cs
// -----------------------------------------------------------------------------
// Lightweight client for the robot-controller JSON API (Lua Socket-2 compatible)
// Dependencies: .NET 8 (no external NuGet packages required)
// -----------------------------------------------------------------------------

using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace ByRobot
{
    /// <summary>
    /// Thin async client that mirrors the Python sample:
    ///   • sends JSON lines over TCP
    ///   • matches replies by monotonically-increasing <c>id</c>
    ///   • exposes convenience wrappers for common commands
    /// </summary>
    public sealed class BYRobotClient : IAsyncDisposable
    {
        private const string ApiVersion = "1.0";

        private readonly TcpClient _tcp = new();
        private readonly CancellationTokenSource _cts = new();
        private readonly ConcurrentDictionary<int, TaskCompletionSource<JsonElement>> _awaiting = new();
        private int _nextId;

        // ─────────────────────────────── Public API ────────────────────────────

        /// <summary>Connects to the controller and starts the background receive loop.</summary>
        public async Task ConnectAsync(string host, int port, int timeoutMs = 5000)
        {
            using var cts = new CancellationTokenSource(timeoutMs);
            await _tcp.ConnectAsync(host, port, cts.Token).ConfigureAwait(false);
            _ = Task.Run(ReceiveLoop, _cts.Token);
        }

        public ValueTask DisposeAsync()
        {
            _cts.Cancel();
            _tcp.Close();
            return ValueTask.CompletedTask;
        }

        // ───── Controller-specific convenience wrappers (add more as required) ─────

        public Task<bool> ClearErrorsAsync()                 => InvokeCmdAsync<bool>(CmdType.remoteClearError);
        public Task<bool> ControlEnableAsync(bool on)        => InvokeCmdAsync<bool>(on ? CmdType.remoteEnable : CmdType.remoteDisable);
        public Task<bool> ChangeModeAsync(ModeType m)        => InvokeCmdAsync<bool>(m == ModeType.Auto ? CmdType.switchToAuto : CmdType.switchToManual);
        public Task<LuaProgramState?> GetLuaStateAsync()     => InvokeCmdAsync<LuaProgramState?>(CmdType.getProgState);
        public Task<bool> ChangeLuaStateAsync(ProgramControlCmdType c)
            => InvokeCmdAsync<bool>(c switch
            {
                ProgramControlCmdType.StartOrRecovery => CmdType.remoteStart,
                ProgramControlCmdType.Pause           => CmdType.remotePause,
                ProgramControlCmdType.Stop            => CmdType.remoteStop,
                _ => throw new ArgumentOutOfRangeException(nameof(c))
            });

        public Task<bool> SetSysVarIAsync(IDictionary<int, int> values)
            => InvokeCmdAsync<bool>(CmdType.setSysVarI,
                   values.Select(kv => new { index = kv.Key, value = kv.Value }));

        public async Task<Dictionary<int, int>?> GetSysVarIAsync(params int[] indexes)
        {
            var ret = await InvokeCmdAsync<JsonElement?>(CmdType.getSysVarI,
                        indexes.Select(i => new { index = i })).ConfigureAwait(false);

            return ret?.EnumerateArray()
                       .ToDictionary(e => e.GetProperty("index").GetInt32(),
                                     e => e.GetProperty("value").GetInt32());
        }

        public Task<bool> SetSysParaFAsync(IDictionary<int, float> values)
            => InvokeCmdAsync<bool>(CmdType.setSysParaF,
                   values.Select(kv => new { index = kv.Key, value = kv.Value }));

        public Task<string> GetJointInfoAsync()
            => InvokeCmdAsync<string>(CmdType.getJointInfo);

        // ───────────────────────────── Core transport ──────────────────────────

        private async Task<T?> InvokeCmdAsync<T>(CmdType cmd, object? param = null, int timeoutMs = 5000)
        {
            var id  = Interlocked.Increment(ref _nextId);
            var tcs = new TaskCompletionSource<JsonElement>(TaskCreationOptions.RunContinuationsAsynchronously);
            _awaiting[id] = tcs;

            var json = JsonSerializer.Serialize(new
            {
                BRCAPI  = ApiVersion,
                cmd     = cmd.ToString(),
                @params = param,
                id
            });
            await SendAsync(json + "\n").ConfigureAwait(false);

            using var cts = new CancellationTokenSource(timeoutMs);
            await using var _ = cts.Token.Register(() => tcs.TrySetCanceled());

            JsonElement reply = await tcs.Task.ConfigureAwait(false);

            // Error branch?
            if (reply.TryGetProperty("error", out var err) && err.ValueKind != JsonValueKind.Null)
                throw new InvalidOperationException($"Robot returned error: {err.GetRawText()}");

            if (typeof(T) == typeof(bool))
                return (T)(object)true;                       // Ack only

            if (reply.TryGetProperty("ret", out var retElem))
            {
                // T may be primitive or JSON string
                if (typeof(T) == typeof(string))
                    return (T)(object)retElem.GetRawText();
                return JsonSerializer.Deserialize<T>(retElem.GetRawText());
            }
            return default;
        }

        private async Task SendAsync(string text)
        {
            byte[] buf = Encoding.UTF8.GetBytes(text);
            await _tcp.GetStream().WriteAsync(buf.AsMemory(), _cts.Token).ConfigureAwait(false);
        }

        private async Task ReceiveLoop()
        {
            NetworkStream ns = _tcp.GetStream();
            byte[] buffer = ArrayPool<byte>.Shared.Rent(1024);
            var sb = new StringBuilder();

            try
            {
                while (!_cts.IsCancellationRequested)
                {
                    int read = await ns.ReadAsync(buffer.AsMemory(), _cts.Token).ConfigureAwait(false);
                    if (read == 0) break;                     // Socket closed

                    sb.Append(Encoding.UTF8.GetString(buffer, 0, read));

                    while (true)
                    {
                        int nl = sb.ToString().IndexOf('\n');
                        if (nl < 0) break;

                        string line = sb.ToString(0, nl);
                        sb.Remove(0, nl + 1);
                        HandleIncoming(line);
                    }
                }
            }
            catch (OperationCanceledException) { /* normal shutdown */ }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        private void HandleIncoming(string jsonLine)
        {
            try
            {
                JsonElement doc = JsonDocument.Parse(jsonLine).RootElement;
                if (!doc.TryGetProperty("id", out var idProp)) return;

                int id = idProp.GetInt32();
                if (_awaiting.TryRemove(id, out var tcs))
                    tcs.TrySetResult(doc);
            }
            catch
            {
                // swallow malformed lines to keep loop alive
            }
        }
    }

    // ────────────────────────────── Supporting enums ───────────────────────────

    public enum CmdType
    {
        setSysVarI, getSysVarI,
        setSysParaI, getSysParaI,
        setSysParaF, getSysParaF,
        getJointInfo, getCtrlMode,
        switchToAuto, switchToManual,
        getEnableStatus, remoteEnable, remoteDisable,
        getProgState, remoteStart, remotePause, remoteStop,
        remoteClearError, getRobotErr,
        getLuaAppStatus, getZeroConfig, moveBackwards
    }

    public enum ModeType                 { Auto = 0, Manual = 1 }
    public enum LuaProgramState          { Stopped = 0, Running = 1, Paused = 2 }
    public enum ProgramControlCmdType    { StartOrRecovery = 0, Pause = 1, Stop = 2 }
}
