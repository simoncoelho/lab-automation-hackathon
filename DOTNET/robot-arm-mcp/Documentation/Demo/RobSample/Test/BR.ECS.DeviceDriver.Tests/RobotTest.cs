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
            //初始化，设置socket连接的ip地址、端口号等，并进行连接
            BYRobotSample robotDemo = new BYRobotSample();
            SocketCommunication socketCommunication = new SocketCommunication();
            socketCommunication.Init("192.168.3.200", 56788);
            robotDemo.Connect(socketCommunication);
            ILog _log = new Log();
            Thread.Sleep(5000);

            //以下示例与Lua示例程序mod001Socket1相对应，每次设置系统变量分别对应进入mod_Pick1()、mod_Place1()、mod_Pick2()、mod_Place2()的判断条件
            //那么当机器人Lua程序获取到这些系统变量时，可以根据不同变量值进行不同的运动，且Api此时会进入等待状态回发消息中，机器人完成运动后需要在Lua程序中发送相关指令，之后才可判断是否设置系统变量成功
            while (true)
            {
                //下面函数为设置系统变量SYSINVAR0为1，SYSINVAR1为1，SYSINVAR2为1，SYSINVAR3为1，SYSINVAR4为1，SYSINVAR5为2
                Dictionary<string, int> setSysValParamsModels1 = new Dictionary<string, int>
                {
                        { "0", 1 },
                        { "1", 1 },
                        { "2", 1 },
                        { "3", 1 },
                        { "4", 1 },
                        { "5", 2 },
                };
                // 设置系统变量，需要传入相应的系统变量数据
                try
                {
                    bool result = robotDemo.SetSysVarI(setSysValParamsModels1, 5 * 60 * 1000);
                    if (!result)
                    {
                        _log.Info($"机器人 设置系统变量失败");
                    }
                }
                catch (Exception ex)
                {
                    _log.Info($"机器人 设置系统变量失败,失败原因：{ex.Message}");
                    Console.ReadLine();
                }

                //下面函数为设置系统变量SYSINVAR0为1，SYSINVAR1为2，SYSINVAR2为2，SYSINVAR3为2，SYSINVAR4为2，SYSINVAR5为2
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
                        _log.Info($"机器人 设置系统变量失败");
                    }
                }
                catch (Exception ex)
                {
                    _log.Info($"机器人 设置系统变量失败,失败原因：{ex.Message}");
                    Console.ReadLine();
                }

                //下面函数为设置系统变量SYSINVAR0为1，SYSINVAR1为2，SYSINVAR2为2，SYSINVAR3为2，SYSINVAR4为1，SYSINVAR5为2
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
                        _log.Info($"机器人 设置系统变量失败");
                    }
                }
                catch (Exception ex)
                {
                    _log.Info($"机器人 设置系统变量失败,失败原因：{ex.Message}");
                    Console.ReadLine();
                }

                //下面函数为设置系统变量SYSINVAR0为1，SYSINVAR1为1，SYSINVAR2为1，SYSINVAR3为1，SYSINVAR4为2，SYSINVAR5为2
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
                        _log.Info($"机器人 设置系统变量失败");
                    }
                }
                catch (Exception ex)
                {
                    _log.Info($"机器人 设置系统变量失败,失败原因：{ex.Message}");
                    Console.ReadLine();
                }
            }

            Thread.Sleep(20000);

            //关闭与机器人的连接
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

            //以下示例与Lua示例程序mod001Socket2相对应，设置系统变量后会进入mod_InputPoint()
            //那么当机器人Lua程序获取到这些系统变量时，可以根据不同变量值进行不同的运动，且Api此时会进入等待状态回发消息中，机器人完成运动后需要在Lua程序中发送相关指令，之后才可判断是否设置系统变量成功
            while (true)
            {
                // 以下为系统参数
                // index0 关节1的目标位置
                // index1 关节2的目标位置
                // index2 关节3的目标位置
                // index3 关节4的目标位置
                // index4 MOVL时是机器人关节最大运动速度，单位为mm/s；MOVJ、MOVABSJ、MOVEXTJ时是机器人关节运动速度与关节最大速度的比率，单位为%
                // index5 MOVL时是融合区域轨迹长度，MOVJ、MOVABSJ、MOVEXTJ时是融合区域关节角度
                // index6 在MOVEXTJ指令时控制外轴1（如夹爪）开合
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

                // 以下为系统变量
                // SYSINVAR0 至 SYSINVAR5 在第一个示例场景中已使用，该示例场景中不用时置为0
                // SYSINVAR6 启用在上位机中传输点位功能
                // SYSINVAR6 控制运动类型，1是MOVL，2是MOVJ，3是MOVABSJ，4是MOVEXTJ夹爪开，5是MOVEXTJ夹爪关
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
                // MOVABSJ关节运动到第一个点位
                try
                {
                    bool result = robotDemo.SetSysVarI(setSysVal1, 5 * 60 * 1000);
                    var currenttime = DateTime.Now;
                    if (!result)
                    {
                        _log.Info($"机器人 设置系统变量失败");
                    }
                }
                catch (Exception ex)
                {
                    _log.Info($"机器人 设置系统变量失败,失败原因：{ex.Message}");
                    Console.ReadLine();
                }

                // MOVL直线运动到第二个点位
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
                        _log.Info($"机器人 设置系统变量失败");
                    }
                }
                catch (Exception ex)
                {
                    _log.Info($"机器人 设置系统变量失败,失败原因：{ex.Message}");
                    Console.ReadLine();
                }

                // MOVJ关节运动到第三个点位
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
                        _log.Info($"机器人 设置系统变量失败");
                    }
                }
                catch (Exception ex)
                {
                    _log.Info($"机器人 设置系统变量失败,失败原因：{ex.Message}");
                    Console.ReadLine();
                }

                // MOVABSJ关节运动到第四个点位
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
                        _log.Info($"机器人 设置系统变量失败");
                    }
                }
                catch (Exception ex)
                {
                    _log.Info($"机器人 设置系统变量失败,失败原因：{ex.Message}");
                    Console.ReadLine();
                }
            }
            Thread.Sleep(20000);

            //关闭与机器人的连接           
            robotDemo.Disconnect();
            Console.ReadLine();
        }
    }
}
