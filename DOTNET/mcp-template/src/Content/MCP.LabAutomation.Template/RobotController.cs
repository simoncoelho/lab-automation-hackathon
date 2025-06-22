// RobotArmControllerSync.cs
// -----------------------------------------------------------------------------
// Synchronous high-level wrapper built on BYRobotClient.
// Provides: InitializeRobotArm / MoveToPosition / OpenGripper / CloseGripper / GetCurrentPosition
// -----------------------------------------------------------------------------

using ByRobot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace robot_arm_mcp
{
    public enum MoveKind
    {
        Linear        = 1,   // MOVL – Cartesian straight-line
        Joint         = 2,   // MOVJ – Joint-interpolated
        AbsoluteJoint = 3    // MOVABSJ – absolute J1-J6
    }

    public sealed class RobotController : IDisposable
    {
        private readonly BYRobotClient _cli = new();
        private readonly TimeSpan _timeout = TimeSpan.FromSeconds(30);

        // ───────────────────────────── Public API ─────────────────────────────

        public void InitializeRobotArm()
        {
            _cli.ConnectAsync("192.168.3.200", 56788, 5000).GetAwaiter().GetResult();
            _cli.ClearErrorsAsync()          .GetAwaiter().GetResult();
            _cli.ChangeModeAsync(ModeType.Auto).GetAwaiter().GetResult();
            _cli.ControlEnableAsync(true)    .GetAwaiter().GetResult();

            if (_cli.GetLuaStateAsync().GetAwaiter().GetResult() != LuaProgramState.Running)
            {
                _cli.ChangeLuaStateAsync(ProgramControlCmdType.StartOrRecovery)
                    .GetAwaiter().GetResult();
            }
        }

        public void MoveToPosition(
            double xOrJ1, double yOrJ2, double zOrJ3, double rzOrJ4,
            double speed,  double accel,
            MoveKind kind = MoveKind.Linear,
            double extAxis = 0)
        {
            ExecuteSocket2(
                opCode: (int)kind,
                v1: xOrJ1, v2: yOrJ2, v3: zOrJ3, v4: rzOrJ4,
                speed: speed, accel: accel, extAxis: extAxis);
        }

        public void OpenGripper(double extAxisAngle = 90) =>
            ExecuteSocket2(4, 0, 0, 0, 0, 30, 50, extAxisAngle);

        public void CloseGripper(double extAxisAngle = 0) =>
            ExecuteSocket2(5, 0, 0, 0, 0, 30, 50, extAxisAngle);

        public double[] GetCurrentPosition()
        {
            string json = _cli.GetJointInfoAsync().GetAwaiter().GetResult();
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("joints")
                                  .EnumerateArray()
                                  .Select(e => e.GetDouble())
                                  .ToArray();
        }

        public void Dispose() => _cli.DisposeAsync().GetAwaiter().GetResult();

        // ─────────────────────────── Internal glue ───────────────────────────

        private void ExecuteSocket2(
            int opCode,
            double v1, double v2, double v3, double v4,
            double speed, double accel,
            double extAxis)
        {
            // 1) push float payload
            _cli.SetSysParaFAsync(new Dictionary<int, float>
            {
                [0] = (float)v1, [1] = (float)v2, [2] = (float)v3, [3] = (float)v4,
                [4] = (float)speed, [5] = (float)accel, [6] = (float)extAxis
            }).GetAwaiter().GetResult();

            // 2) trigger motion
            _cli.SetSysVarIAsync(new Dictionary<int, int>
            {
                [6] = 1,
                [7] = opCode
            }).GetAwaiter().GetResult();

            // 3) block until Lua clears flag
            var sw = Stopwatch.StartNew();
            while (true)
            {
                var flag = _cli.GetSysVarIAsync(6).GetAwaiter().GetResult();
                if (flag != null && flag.TryGetValue(6, out int f) && f == 0)
                    return;

                if (sw.Elapsed > _timeout)
                    throw new TimeoutException("Robot did not complete the operation within the allotted time.");

                Thread.Sleep(200); // 5 Hz polling
            }
        }
    }
}
