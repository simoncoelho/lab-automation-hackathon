using BR.ECS.DeviceDriver.Robot.BYRobotSample;
using BR.ECS.DeviceDriver.Robot.BYRobotSample.models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BR.ECS.DeviceDriver.Tests
{
    public class RobotTestMainPointInBRTeach
    {
        static async void Main()
        {
            // Initialize, set the socket connection IP address, port, etc., and connect
            BYRobotSample robotDemo = new BYRobotSample();
            SocketCommunication socketCommunication = new SocketCommunication();
            socketCommunication.Init("192.168.3.200", 56788);
            robotDemo.Connect(socketCommunication);
            ILog _log = new Log();
            Thread.Sleep(5000);

            // The following example corresponds to the Lua sample program mod001Socket1. Each time the system variable is set, it corresponds to the condition for entering mod_Pick1(), mod_Place1(), mod_Pick2(), or mod_Place2().
            // When the robot Lua program obtains these system variables, it can perform different movements according to different variable values. The API will enter a waiting state and send a message back. After the robot completes the movement, the Lua program needs to send related instructions before it can determine whether the system variable was set successfully.
            while (true)
            {
                // The following function sets system variables: SYSINVAR0=1, SYSINVAR1=1, SYSINVAR2=1, SYSINVAR3=1, SYSINVAR4=1, SYSINVAR5=2
                Dictionary<string, int> setSysValParamsModels1 = new Dictionary<string, int>
                {
                        { "0", 1 },
                        { "1", 1 },
                        { "2", 1 },
                        { "3", 1 },
                        { "4", 1 },
                        { "5", 2 },
                };
                // Set system variables, need to pass in the corresponding system variable data
                try
                {
                    bool result = robotDemo.SetSysVarI(setSysValParamsModels1, 5 * 60 * 1000);
                    if (!result)
                    {
                        _log.Info($"Robot failed to set system variable");
                    }
                }
                catch (Exception ex)
                {
                    _log.Info($"Robot failed to set system variable, reason: {ex.Message}");
                    Console.ReadLine();
                }

                // The following function sets system variables: SYSINVAR0=1, SYSINVAR1=2, SYSINVAR2=2, SYSINVAR3=2, SYSINVAR4=2, SYSINVAR5=2
                Dictionary<string, int> setSysValParamsModels2 = new Dictionary<string, int>
                {
                        { "0", 1 },
                        { "1", 2 },
                        { "2", 2 },
                        { "3", 2 },
                        { "4", 2 },
                        { "5", 2 },
                };
                try
                {
                    bool result = robotDemo.SetSysVarI(setSysValParamsModels2, 5 * 60 * 1000);
                    var currenttime = DateTime.Now;
                    if (!result)
                    {
                        _log.Info($"Robot failed to set system variable");
                    }
                }
                catch (Exception ex)
                {
                    _log.Info($"Robot failed to set system variable, reason: {ex.Message}");
                    Console.ReadLine();
                }

                // The following function sets system variables: SYSINVAR0=1, SYSINVAR1=2, SYSINVAR2=2, SYSINVAR3=2, SYSINVAR4=1, SYSINVAR5=2
                Dictionary<string, int> setSysValParamsModels3 = new Dictionary<string, int>
                {
                        { "0", 1 },
                        { "1", 2 },
                        { "2", 2 },
                        { "3", 2 },
                        { "4", 1 },
                        { "5", 2 },
                };
                try
                {
                    bool result = robotDemo.SetSysVarI(setSysValParamsModels3, 5 * 60 * 1000);
                    var currenttime = DateTime.Now;
                    if (!result)
                    {
                        _log.Info($"Robot failed to set system variable");
                    }
                }
                catch (Exception ex)
                {
                    _log.Info($"Robot failed to set system variable, reason: {ex.Message}");
                    Console.ReadLine();
                }

                // The following function sets system variables: SYSINVAR0=1, SYSINVAR1=1, SYSINVAR2=1, SYSINVAR3=1, SYSINVAR4=2, SYSINVAR5=2
                Dictionary<string, int> setSysValParamsModels4 = new Dictionary<string, int>
                {
                        { "0", 1 },
                        { "1", 1 },
                        { "2", 1 },
                        { "3", 1 },
                        { "4", 2 },
                        { "5", 2 },
                };
                try
                {
                    bool result = robotDemo.SetSysVarI(setSysValParamsModels4, 5 * 60 * 1000);
                    var currenttime = DateTime.Now;
                    if (!result)
                    {
                        _log.Info($"Robot failed to set system variable");
                    }
                }
                catch (Exception ex)
                {
                    _log.Info($"Robot failed to set system variable, reason: {ex.Message}");
                    Console.ReadLine();
                }
            }

            Thread.Sleep(20000);

            // Close the connection to the robot
            robotDemo.Disconnect();
            Console.ReadLine();
        }
    }


    public class RobotTestMainPointInUpperComputer
    {
        static async Task Main()
        {
            BYRobotSample robotDemo = new BYRobotSample();
            SocketCommunication socketCommunication = new SocketCommunication();
            socketCommunication.Init("192.168.3.200", 56788);
            robotDemo.Connect(socketCommunication);
            ILog _log = new Log();
            Thread.Sleep(5000);

            // The following example corresponds to the Lua sample program mod001Socket2. After setting the system variable, it will enter mod_InputPoint().
            // When the robot Lua program obtains these system variables, it can perform different movements according to different variable values. The API will enter a waiting state and send a message back. After the robot completes the movement, the Lua program needs to send related instructions before it can determine whether the system variable was set successfully.
            while (true)
            {
                // The following are system parameters
                // index0: Target position of joint 1
                // index1: Target position of joint 2
                // index2: Target position of joint 3
                // index3: Target position of joint 4
                // index4: For MOVL, this is the robot joint max speed (mm/s); for MOVJ, MOVABSJ, MOVEXTJ, this is the ratio of joint speed to max speed (%)
                // index5: For MOVL, this is the trajectory length of the blending area; for MOVJ, MOVABSJ, MOVEXTJ, this is the joint angle of the blending area
                // index6: For MOVEXTJ command, controls external axis 1 (e.g., gripper) open/close
                Dictionary<string, float> setSysParaF1 = new Dictionary<string, float>
                {
                        { "0", 271.83f },
                        { "1", 44.08f },
                        { "2", -120.67f },
                        { "3", 80.16f },
                        { "4", 50f },
                        { "5", 20f },
                        { "6", 0f }
                 };

                // The following are system variables
                // SYSINVAR0 to SYSINVAR5 are used in the first example scenario, set to 0 when not used in this scenario
                // SYSINVAR6: Enable point transfer function in the upper computer
                // SYSINVAR7: Control motion type, 1=MOVL, 2=MOVJ, 3=MOVABSJ, 4=MOVEXTJ gripper open, 5=MOVEXTJ gripper close
                Dictionary<string, int> setSysVal1 = new Dictionary<string, int>
                {
                        { "0", 0 },
                        { "1", 0 },
                        { "2", 0 },
                        { "3", 0 },
                        { "4", 0 },
                        { "5", 0 },
                        { "6", 1 },
                        { "7", 3 },
                };
                robotDemo.SetSysParaF(setSysParaF1, 5 * 60 * 1000);
                // MOVABSJ joint moves to the first point
                try
                {
                    bool result = robotDemo.SetSysVarI(setSysVal1, 5 * 60 * 1000);
                    var currenttime = DateTime.Now;
                    if (!result)
                    {
                        _log.Info($"Robot failed to set system variable");
                    }
                }
                catch (Exception ex)
                {
                    _log.Info($"Robot failed to set system variable, reason: {ex.Message}");
                    Console.ReadLine();
                }

                // MOVL linear move to the second point
                Dictionary<string, float> setSysParaF2 = new Dictionary<string, float>
                {
                        { "0", 276.53f },
                        { "1", 337.21f },
                        { "2", 310f },
                        { "3", -0.98f },
                        { "4", 50f },
                        { "5", 20f },
                        { "6", 0f }
                 };
                Dictionary<string, int> setSysVal2 = new Dictionary<string, int>
                {
                        { "0", 0 },
                        { "1", 0 },
                        { "2", 0 },
                        { "3", 0 },
                        { "4", 0 },
                        { "5", 0 },
                        { "6", 1 },
                        { "7", 1 },
                };
                robotDemo.SetSysParaF(setSysParaF2, 5 * 60 * 1000);
                try
                {
                    bool result = robotDemo.SetSysVarI(setSysVal2, 5 * 60 * 1000);
                    var currenttime = DateTime.Now;
                    if (!result)
                    {
                        _log.Info($"Robot failed to set system variable");
                    }
                }
                catch (Exception ex)
                {
                    _log.Info($"Robot failed to set system variable, reason: {ex.Message}");
                    Console.ReadLine();
                }

                // MOVJ joint moves to the third point
                Dictionary<string, float> setSysParaF3 = new Dictionary<string, float>
                {
                        { "0", 426.93f },
                        { "1", -6.87f },
                        { "2", 167.80f },
                        { "3", 16.91f },
                        { "4", 50f },
                        { "5", 20f },
                        { "6", 0f }
                 };
                Dictionary<string, int> setSysVal3 = new Dictionary<string, int>
                {
                        { "0", 0 },
                        { "1", 0 },
                        { "2", 0 },
                        { "3", 0 },
                        { "4", 0 },
                        { "5", 0 },
                        { "6", 1 },
                        { "7", 2 },
                };
                robotDemo.SetSysParaF(setSysParaF3, 5 * 60 * 1000);
                try
                {
                    bool result = robotDemo.SetSysVarI(setSysVal3, 5 * 60 * 1000);
                    var currenttime = DateTime.Now;
                    if (!result)
                    {
                        _log.Info($"Robot failed to set system variable");
                    }
                }
                catch (Exception ex)
                {
                    _log.Info($"Robot failed to set system variable, reason: {ex.Message}");
                    Console.ReadLine();
                }

                // MOVABSJ joint moves to the fourth point
                Dictionary<string, float> setSysParaF4 = new Dictionary<string, float>
                {
                        { "0", 268.53f },
                        { "1", -65.81f },
                        { "2", -65.6f },
                        { "3", 127.95f },
                        { "4", 50f },
                        { "5", 20f },
                        { "6", 0f }
                 };
                Dictionary<string, int> setSysVal4 = new Dictionary<string, int>
                {
                        { "0", 0 },
                        { "1", 0 },
                        { "2", 0 },
                        { "3", 0 },
                        { "4", 0 },
                        { "5", 0 },
                        { "6", 1 },
                        { "7", 3 },
                };
                robotDemo.SetSysParaF(setSysParaF4, 5 * 60 * 1000);
                try
                {
                    bool result = robotDemo.SetSysVarI(setSysVal4, 5 * 60 * 1000);
                    var currenttime = DateTime.Now;
                    if (!result)
                    {
                        _log.Info($"Robot failed to set system variable");
                    }
                }
                catch (Exception ex)
                {
                    _log.Info($"Robot failed to set system variable, reason: {ex.Message}");
                    Console.ReadLine();
                }
            }
            Thread.Sleep(20000);

            // Close the connection to the robot           
            robotDemo.Disconnect();
            Console.ReadLine();
        }
    }
}
