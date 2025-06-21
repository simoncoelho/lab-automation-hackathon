using BR.ECS.DeviceDriver.Robot.BYRobotSample.models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BR.ECS.DeviceDriver.Robot.BYRobotSample
{
    public class BYRobotSample
    {
        private const string _BRCAPI = "1.0";                                                       //系统指令版本
        private const int MAXINDEX = int.MaxValue;
        private bool _isInited = false;
        private string _errorInfos = "";
        private string _tempErr = "";
        private int _setSysVarIReturnValue = 1;
        private SocketCommunication _communication = new SocketCommunication();                     //socket信息，包括ip地址及端口
        private SafeList<ReceiveRobotInfo> _receiveMessages = new SafeList<ReceiveRobotInfo>();
        private CancellationTokenSource _tokenSource = new CancellationTokenSource();
        private readonly SequentialNumberGenerator _generator;                                      //用于生成发送消息的id
        private readonly bool _isNeedSendHeart = true;                                              //是否需要发送并接收心跳信息，默认需要
        private DateTime currenttime;
        private readonly object _lockerForSend = new object();                                      //发送消息时所使用，多个线程同时调用造成发送错误
        private ILog _log;
        public BYRobotSample()
        {
            _generator = new SequentialNumberGenerator(MAXINDEX);
            SendHeart(_tokenSource);
            _log = new Log();
        }

        /// <summary>
        /// 连接机器人，传入设置的socket
        /// </summary>
        /// <param name="communication"></param>
        /// <param name="DeviceCfg"></param>
        /// <returns></returns>
        public virtual int Connect(SocketCommunication communication)
        {
            _communication = communication;
            try
            {
                _isInited = true;

                if (_communication.IsConnected)
                {
                    _communication.OnReceive = HandleReceiveData;
                    ChangeLuaProgramState(ProgramControlCmdType.Stop);//停止lua程序
                    PrepareRobot();//准备机器人
                    _log.Info($"机器人 连接成功");
                    return 0;
                }

                _communication.Open();
                DateTime dateTime = DateTime.Now.AddSeconds(5);
                while (dateTime > DateTime.Now)
                {
                    if (!_communication.IsConnected)
                    {
                        Thread.Sleep(100);
                        continue;
                    }
                    else
                    {
                        _communication.OnReceive = HandleReceiveData;
                        ClearErrors();
                        ChangeLuaProgramState(ProgramControlCmdType.Stop);//停止lua程序
                        PrepareRobot();//准备机器人
                        _log.Info($"机器人 连接成功");
                        return 0;
                    }

                }
                _log.Error($"机器人 连接失败");
                return -1;
            }
            catch (Exception e)
            {
                _isInited = false;
                if (_communication != null && _communication.IsConnected)
                {
                    _communication.Close();
                }
                _log.Error($"机器人 连接失败：{e.Message}");
                return -1;
            }
        }

        /// <summary>
        /// 断开与机器人连接
        /// </summary>
        /// <returns></returns>
        public virtual int Disconnect()
        {
            try
            {
                // 检查 _communication 是否已经连接
                if (_communication != null && _communication.IsConnected)
                {
                    int ret = _communication.Close() ? 0 : -1;
                    if (ret == 0)
                    {
                        _log.Info($"机器人 关闭连接");
                    }
                    return ret;
                }
                else
                {
                    _log.Info($"机器人 连接已经关闭，无需重复关闭");
                    return 0;  // 返回0表示连接已经关闭或不需要关闭
                }
            }
            catch (ObjectDisposedException ex)
            {
                _log.Error($"机器人 对象已被释放，无法关闭连接: {ex.Message}");
                return -1;
            }
            catch (Exception ex)
            {
                _log.Error($"机器人 关闭连接时发生错误: {ex.Message}");
                return -1;
            }
        }

        /// <summary>
        /// 机器人清错指令，在连接前会调用一次
        /// </summary>
        /// <param name="timeOut"></param>
        /// <returns></returns>
        public bool ClearErrors(int timeOut = 5000)
        {
            bool res = ClearErrorsAsync(timeOut).Result;
            if (!res)
            {
                for (int i = 0; i < 1; i++)
                {
                    res = ClearErrorsAsync(timeOut).Result;
                    if (res)
                    {
                        break;
                    }
                }
            }

            res = ClearErrorsAsync(timeOut).Result;
            _log.Info($"机器人 清除错误结果{res}");
            return res;
        }

        /// <summary>
        /// 机器人准备工作
        /// </summary>
        /// <returns>0成功 其余失败</returns>
        private int PrepareRobot()
        {
            var temp = GetRobotMode();

            if (ModeType.Auto != temp)
            {
                if (!ChangeMode(ModeType.Auto))
                {
                    return -1;
                }
            }

            bool? v = GetIsRobotEnable();
            if (v == null || v == false)
            {
                if (!ControlRemoteEnable(true))
                {
                    return -1;
                }
            }
            var proState = GetLuaProgramState();
            if (proState != LuaProgramState.Running)
            {
                if (!ChangeLuaProgramState(ProgramControlCmdType.StartOrRecovery))
                {
                    return -1;
                }
                //else
                //{
                //    Task.Delay(5000).Wait();  //lua程序启动需要一定时间,后续需要机器人控制器返回确切信号
                //}
                var r = CheckLuaReadyInPeriodTime();
            }

            return 0;
        }

        /// <summary>
        /// 设置机器人系统变量
        /// </summary>
        /// <param name="sysVarIParams"></param>
        /// <param name="timeOut"></param>
        /// <returns></returns>
        public bool SetSysVarI(Dictionary<string, int> sysVarIParams, int timeOut = 5 * 60 * 1000)
        {
            var resT = SetSysVarIAsync(sysVarIParams, timeOut);
            var res = resT.GetAwaiter().GetResult();
            _log.Info($"机器人 设置系统变量{JsonConvert.SerializeObject(sysVarIParams)},结果{res}");
            return res;
        }

        /// <summary>
        /// 异步任务，设置机器人系统变量
        /// </summary>
        /// <param name="sysVarIDics">系统变量信息</param>
        /// <param name="timeOut">超时时间</param>
        /// <returns></returns>
        private async Task<bool> SetSysVarIAsync(Dictionary<string, int> sysVarIDics, int timeOut = 5 * 60 * 1000)
        {
            if (sysVarIDics == null || sysVarIDics.Count == 0)
            {
                _log.Warn($"机器人 设置系统变量失败,系统变量信息为空");
                return false;
            }

            List<SysVarIParam> sysVarIParams = new List<SysVarIParam>();
            foreach (var item in sysVarIDics)
            {
                if (int.TryParse(item.Key, out int tem))
                {
                    sysVarIParams.Add(new SysVarIParam(tem, item.Value));
                }
                else
                {
                    _log.Warn($"机器人 设置系统变量失败,系统变量ID:{item.Key}不是数字");
                    return false;
                }
            }

            var t = await SendAndWaitResponseAsync(CmdType.setSysVarI, sysVarIParams, timeOut);
            try
            {
                if (t == null)
                {
                    if (!string.IsNullOrEmpty(_errorInfos))
                    {
                        if (_errorInfos.Contains("急停"))
                        {
                            throw new Exception(_errorInfos);

                        }
                        if (_errorInfos.Contains("碰撞"))
                        {
                            throw new Exception(_errorInfos);
                        }
                        else
                        {
                            throw new Exception("机器人执行指令异常");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error($"机器人 捕获到异常：{ex.Message}");
                Console.ReadLine();               
            }

            var jsonMess = JsonConvert.SerializeObject(t);
            if (t != null)
            {
                if ((jsonMess).Contains($"\"value\":{_setSysVarIReturnValue}"))
                {
                    //_log.Info($"机器人 设置系统变量{JsonConvert.SerializeObject(sysVarIParams)},结果 true");
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                _log.Warn($"机器人 发送设置系统变量失败!");
            }
            return false;
        }

        /// <summary>
        /// 获取机器人系统变量
        /// </summary>
        /// <param name="indexs"></param>
        /// <param name="timeOut"></param>
        /// <returns></returns>
        public List<SysVarIParam> GetSysVarI(List<int> indexs, int timeOut = 5 * 60 * 1000)
        {
            var res = GetSysVarIAsync(indexs, timeOut).Result;
            _log.Info($"机器人 获取系统变量结果{JsonConvert.SerializeObject(res)}");
            return res;
        }

        /// <summary>
        /// 异步任务，获取机器人系统变量
        /// </summary>
        /// <param name="indexs"></param>
        /// <param name="timeOut"></param>
        /// <returns></returns>
        private async Task<List<SysVarIParam>> GetSysVarIAsync(List<int> indexs, int timeOut = 5 * 60 * 1000)
        {
            var infos = new List<object>();
            foreach (var item in indexs)
            {
                infos.Add(new { index = item });
            }
            var cmd = CmdType.getSysVarI;
            var t = await SendAndWaitResponseAsync(cmd, infos, timeOut);
            var jsonMess = JsonConvert.SerializeObject(t);
            if (t != null)
            {
                var res = JsonConvert.DeserializeAnonymousType(jsonMess, new { BRCAPI = _BRCAPI, cmd = CmdType.getSysVarI, ret = new List<SysVarIParam>(), id = 0 });
                return res.ret;
            }
            else
            {
                _log.Warn($"机器人 获取系统变量失败");
            }
            return null;
        }

        /// <summary>
        /// 设置机器人整型系统参数
        /// </summary>
        /// <param name="sysParaIDics"></param>
        /// <param name="timeOut"></param>
        /// <returns></returns>
        public bool SetSysParaI(Dictionary<string, int> sysParaIDics, int timeOut = 5 * 60 * 1000)
        {
            var resT = SetSysParaIAsync(sysParaIDics, timeOut);
            var res = resT.GetAwaiter().GetResult();
            _log.Info($"机器人 设置系统变量{JsonConvert.SerializeObject(sysParaIDics)},结果{res}");
            return res;
        }

        /// <summary>
        /// 异步任务，设置机器人整型系统参数
        /// </summary>
        /// <param name="sysVarIDics">系统变量信息</param>
        /// <param name="timeOut">超时时间</param>
        /// <returns></returns>
        private async Task<bool> SetSysParaIAsync(Dictionary<string, int> sysParaIDics, int timeOut = 5 * 60 * 1000)
        {
            if (sysParaIDics == null || sysParaIDics.Count == 0)
            {
                _log.Warn($"机器人 设置系统变量失败,系统变量信息为空");
                return false;
            }

            List<SysVarIParam> sysParaIParams = new List<SysVarIParam>();
            foreach (var item in sysParaIDics)
            {
                if (int.TryParse(item.Key, out int tem))
                {
                    sysParaIParams.Add(new SysVarIParam(tem, item.Value));
                }
                else
                {
                    _log.Warn($"机器人 设置系统变量失败,系统变量ID:{item.Key}不是数字");
                    return false;
                }
            }
            ReceiveRobotInfo t = await SendAndWaitResponseAsync(CmdType.setSysParaI, sysParaIParams, timeOut);

            if (t != null)
            {
                if (t.error == null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                _log.Warn($"机器人 设置整数系统参数变量失败!");
            }
            return false;
        }

        /// <summary>
        /// 获取机器人整型系统参数
        /// </summary>
        /// <param name="indexs"></param>
        /// <param name="timeOut"></param>
        /// <returns></returns>
        public List<SysVarIParam> GetSysParaI(List<int> indexs, int timeOut = 5 * 60 * 1000)
        {
            var res = GetSysParaIAsync(indexs, timeOut).Result;
            _log.Debug($"机器人 获取整型系统参数结果{JsonConvert.SerializeObject(res)}");
            return res;
        }

        /// <summary>
        /// 异步任务，获取机器人整型系统参数
        /// </summary>
        /// <param name="indexs"></param>
        /// <param name="timeOut"></param>
        /// <returns></returns>
        private async Task<List<SysVarIParam>> GetSysParaIAsync(List<int> indexs, int timeOut = 5 * 60 * 1000)
        {
            var infos = new List<object>();
            foreach (var item in indexs)
            {
                infos.Add(new { index = item });
            }
            var cmd = CmdType.getSysParaI;
            var t = await SendAndWaitResponseAsync(cmd, infos, timeOut);
            var jsonMess = JsonConvert.SerializeObject(t);
            if (t != null)
            {
                var res = JsonConvert.DeserializeAnonymousType(jsonMess, new { BRCAPI = _BRCAPI, cmd = CmdType.getSysVarI, ret = new List<SysVarIParam>(), id = 0 });
                return res.ret;
            }
            else
            {
                _log.Warn($"机器人 获取整型系统参数失败");
            }
            return null;
        }

        /// <summary>
        /// 设置机器人浮点型系统参数
        /// </summary>
        /// <param name="sysParaFDics"></param>
        /// <param name="timeOut"></param>
        /// <returns></returns>
        public bool SetSysParaF(Dictionary<string, float> sysParaFDics, int timeOut = 5 * 60 * 1000)
        {
            bool res = SetSysParaFAsync(sysParaFDics, timeOut).Result;
            _log.Info($"机器人 设置浮点型系统参数{JsonConvert.SerializeObject(sysParaFDics)},结果{res}");
            return res;
        }

        /// <summary>
        /// 异步任务，设置机器人浮点型系统参数
        /// </summary>
        /// <param name="sysParaFDics">浮点型系统参数信息</param>
        /// <param name="timeOut">超时时间</param>
        /// <returns></returns>
        private async Task<bool> SetSysParaFAsync(Dictionary<string, float> sysParaFDics, int timeOut = 5 * 60 * 1000)
        {
            if (sysParaFDics == null || sysParaFDics.Count == 0)
            {
                _log.Warn($"机器人 设置浮点型系统参数失败,系统变量信息为空");
                return false;
            }

            List<SysVarIParam> sysParaFParams = new List<SysVarIParam>();
            foreach (var item in sysParaFDics)
            {
                if (int.TryParse(item.Key, out int tem))
                {
                    sysParaFParams.Add(new SysVarIParam(tem, item.Value));
                }
                else
                {
                    _log.Warn($"机器人 设置浮点型系统参数失败,系统参数ID:{item.Key}不是数字");
                    return false;
                }
            }
            ReceiveRobotInfo t = await SendAndWaitResponseAsync(CmdType.setSysParaF, sysParaFParams, timeOut);

            if (t != null)
            {
                if (t.error == null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                _log.Warn($"机器人 设置浮点型系统参数失败!");
            }
            return false;
        }

        /// <summary>
        /// 获取机器人浮点型系统参数
        /// </summary>
        /// <param name="indexs"></param>
        /// <param name="timeOut"></param>
        /// <returns></returns>
        public List<SysVarIParam> GetSysParaF(List<int> indexs, int timeOut = 5 * 60 * 1000)
        {
            var res = GetSysParaFAsync(indexs, timeOut).Result;
            _log.Debug($"机器人 获取浮点型系统参数结果{JsonConvert.SerializeObject(res)}");
            return res;
        }

        /// <summary>
        /// 异步任务，获取机器人浮点型系统参数
        /// </summary>
        /// <param name="indexs"></param>
        /// <param name="timeOut"></param>
        /// <returns></returns>
        private async Task<List<SysVarIParam>> GetSysParaFAsync(List<int> indexs, int timeOut = 5 * 60 * 1000)
        {
            var infos = new List<object>();
            foreach (var item in indexs)
            {
                infos.Add(new { index = item });
            }
            var cmd = CmdType.getSysParaF;
            var t = await SendAndWaitResponseAsync(cmd, infos, timeOut);
            var jsonMess = JsonConvert.SerializeObject(t);
            if (t != null)
            {
                var res = JsonConvert.DeserializeAnonymousType(jsonMess, new { BRCAPI = _BRCAPI, cmd = CmdType.getSysParaF, ret = new List<SysVarIParam>(), id = 0 });
                return res.ret;
            }
            else
            {
                _log.Warn($"机器人 获取浮点型系统参数失败");
            }
            return null;
        }

        /// <summary>
        /// 获取机器人当前位置信息
        /// </summary>
        /// <param name="timeOut"></param>
        /// <returns></returns>
        public string GetJointInfo(int timeOut = 5000)
        {
            var res = GetJointInfoAsync(timeOut).Result;
            _log.Info($"机器人 获取关节信息结果{res}");
            return res;
        }

        /// <summary>
        /// 异步任务，获取机器人当前位置信息
        /// </summary>
        /// <param name="timeOut"></param>
        /// <returns></returns>
        private async Task<string> GetJointInfoAsync(int timeOut = 5000)
        {
            CmdType cmdType = CmdType.getJointInfo;
            var t = await SendAndWaitResponseAsync(cmdType, null, timeOut);
            var jsonMess = JsonConvert.SerializeObject(t.ret);
            if (t != null)
            {
                if ((jsonMess).Contains("joints"))
                {
                    return (string)jsonMess;
                }
                else
                {
                    _log.Warn($"获取关节信息失败,返回字段未包含joints,详细信息为{JsonConvert.SerializeObject(t)}");
                    return "";
                }

            }
            else
            {
                _log.Warn($"机器人 获取关节信息失败");
            }
            return "";
        }

        /// <summary>
        /// 获取机器人手动/自动模式
        /// </summary>
        /// <param name="timeOut"></param>
        /// <returns></returns>
        private ModeType? GetRobotMode(int timeout = 5000)
        {
            var res = GetRobotModeAsync(timeout).Result;
            if (res == null || res != ModeType.Auto)
            {
                _log.Info($"机器人 获取模式结果{res}");
            }
            else
            {
                _log.Debug($"机器人 获取模式结果{res}");
            }
            return res;
        }

        /// <summary>
        /// 异步任务，获取机器人手动/自动模式
        /// </summary>
        /// <param name="timeOut"></param>
        /// <returns></returns>
        private async Task<ModeType?> GetRobotModeAsync(int timeout = 500)
        {
            CmdType cmdType = CmdType.getCtrlMode;
            var t = await SendAndWaitResponseAsync(cmdType, null, timeout);
            var jsonMess = JsonConvert.SerializeObject(t);
            if (t != null)
            {
                var jsonModel1 = new { BRCAPI = _BRCAPI, ret = new { value = 8 }, id = 0 };
                var res = JsonConvert.DeserializeAnonymousType(jsonMess, jsonModel1);
                if (res.ret.value >= 0 && res.ret.value <= 2)
                {
                    return (ModeType)res.ret.value;
                }
                else
                {
                    _log.Warn($"机器人 获取模式失败,详细信息为{JsonConvert.SerializeObject(t)}");
                    return null;
                }
            }
            else
            {
                _log.Warn($"机器人 获取模式失败");
            }
            return null;
        }

        /// <summary>
        /// 设置机器人手动/自动模式
        /// </summary>
        /// <param name="mODEType"></param>
        /// <param name="timeOut"></param>
        /// <returns></returns>
        private bool ChangeMode(ModeType mODEType, int timeOut = 5000)
        {
            var res = ChangeModeAsync(mODEType, timeOut).Result;
            _log.Info($"机器人 设置模式{mODEType},结果{res}");
            return res;
        }

        /// <summary>
        /// 异步任务，设置机器人手动/自动模式
        /// </summary>
        /// <param name="mODEType">浮点型系统参数信息</param>
        /// <param name="timeOut">超时时间</param>
        /// <returns></returns>
        private async Task<bool> ChangeModeAsync(ModeType mODEType, int timeOut = 5000)
        {
            CmdType cmdType = CmdType.getCtrlMode;
            switch (mODEType)
            {
                case ModeType.Auto:
                    cmdType = CmdType.switchToAuto;
                    break;

                case ModeType.Manual:
                    cmdType = CmdType.switchToManual;
                    break;

                default:
                    break;
            }

            var t = await SendAndWaitResponseAsync(cmdType, null, timeOut);
            var jsonMess = JsonConvert.SerializeObject(t);
            if (t != null)
            {
                if ((jsonMess).Contains("CmdDone"))
                {
                    return true;
                }
                else
                {
                    _log.Warn($"机器人 模式切换失败,返回字段未包含CmdDone,详细信息为{JsonConvert.SerializeObject(t)}");
                    return false;
                }
            }
            else
            {
                _log.Warn($"机器人 模式切换{mODEType}失败");
            }
            return false;
        }

        /// <summary>
        /// 获取机器人使能状态
        /// </summary>
        /// <param name="timeOut"></param>
        /// <returns></returns>
        private bool? GetIsRobotEnable(int timeout = 5000)
        {
            bool? res = GetIsRobotEnableAsync(timeout).Result;
            if (res == null || res == false)
            {
                _log.Info($"机器人 获取是否在使能状态结果{res}");
            }
            else
            {
                _log.Debug($"机器人 获取是否在使能状态结果{res}");
            }
            return res;
        }

        /// <summary>
        /// 异步任务，获取机器人使能状态
        /// </summary>
        /// <param name="timeOut"></param>
        /// <returns></returns>
        private async Task<bool?> GetIsRobotEnableAsync(int timeout = 500)
        {
            CmdType cmdType = CmdType.getEnableStatus;
            var t = await SendAndWaitResponseAsync(cmdType, null, timeout);
            var jsonMess = JsonConvert.SerializeObject(t);
            if (t != null)
            {
                var jsonModel1 = new { BRCAPI = _BRCAPI, ret = new { value = 8 }, id = 0 };
                var res = JsonConvert.DeserializeAnonymousType(jsonMess, jsonModel1);
                if (res.ret.value >= 0 && res.ret.value <= 1)
                {
                    return res.ret.value == 1 ? true : false;
                }
                else
                {
                    _log.Warn($"机器人 获取使能情况失败,详细信息为{JsonConvert.SerializeObject(t)}");
                    return null;
                }
            }
            else
            {
                _log.Warn($"机器人 获取使能情况失败");
            }
            return null;
        }

        /// <summary>
        /// 控制机器人使能
        /// </summary>
        /// <param name="isEnable"></param>
        /// <param name="timeOut"></param>
        /// <returns></returns>
        public bool ControlRemoteEnable(bool isEnable, int timeOut = 5000)
        {
            bool res = RemoteEnableAsync(isEnable, timeOut).Result;
            _log.Info($"机器人 设置使能{isEnable},结果{res}");
            return res;
        }

        /// <summary>
        /// 异步任务，控制机器人使能
        /// </summary>
        /// <param name="isEnable"></param>
        /// <param name="timeOut"></param>
        /// <returns></returns>
        private async Task<bool> RemoteEnableAsync(bool isEnable, int timeOut = 5000)
        {
            CmdType cmdType;
            if (isEnable)
            {
                cmdType = CmdType.remoteEnable;
            }
            else
            {
                cmdType = CmdType.remoteDisable;
            }
            var t = await SendAndWaitResponseAsync(cmdType, null, timeOut);
            var jsonMess = JsonConvert.SerializeObject(t);
            if (t != null)
            {

                if ((jsonMess).Contains("CmdDone"))
                {
                    return true;
                }
                else
                {
                    _log.Warn($"机器人 使能切换失败,返回字段未包含CmdDone,详细信息为{JsonConvert.SerializeObject(t)}");
                    return false;
                }

            }
            else
            {
                _log.Warn($"机器人 使能切换失败");
            }
            return false;
        }

        /// <summary>
        /// 获取机器人lua运行状态
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        private LuaProgramState? GetLuaProgramState(int timeout = 5000)
        {
            var res = GetLuaProgramStateAsync(timeout).Result;
            if (res == null || res != LuaProgramState.Running)
            {
                _log.Info($"机器人 获取lua运行状态结果{res}");
            }
            else
            {
                _log.Debug($"机器人 获取lua运行状态结果{res}");
            }
            return res;
        }

        /// <summary>
        /// 异步任务，获取机器人lua运行状态
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        private async Task<LuaProgramState?> GetLuaProgramStateAsync(int timeout = 500)
        {
            CmdType cmdType = CmdType.getProgState;
            var t = await SendAndWaitResponseAsync(cmdType, null, timeout);
            var jsonMess = JsonConvert.SerializeObject(t);
            if (t != null)
            {

                var jsonModel1 = new { BRCAPI = _BRCAPI, ret = new { value = 8 }, id = 0 };
                var res = JsonConvert.DeserializeAnonymousType(jsonMess, jsonModel1);
                if (res.ret.value >= 0 && res.ret.value <= 3)
                {
                    return (LuaProgramState)res.ret.value;
                }
                else
                {
                    _log.Warn($"机器人 获取lua运行状态失败,详细信息为{JsonConvert.SerializeObject(t)}");
                    return null;
                }

            }
            return null;
        }

        /// <summary>
        /// 设置机器人lua程序运行
        /// </summary>
        /// <param name="programControlCMDType"></param>
        /// <param name="timeOut"></param>
        /// <returns></returns>
        private bool ChangeLuaProgramState(ProgramControlCmdType programControlCMDType, int timeOut = 5000)
        {
            bool res = ChangeLuaProgramstateAsync(programControlCMDType, timeOut).Result;
            _log.Info($"机器人 设置lua程序运行情况{programControlCMDType},结果{res}");
            return res;
        }

        /// <summary>
        /// 异步任务，设置机器人lua程序运行
        /// </summary>
        /// <param name="programControlCMDType"></param>
        /// <param name="timeOut"></param>
        /// <returns></returns>
        private async Task<bool> ChangeLuaProgramstateAsync(ProgramControlCmdType programControlCMDType, int timeOut = 5000)
        {
            _log.Info($"机器人 设置lua程序运行状态为{programControlCMDType}");
            CmdType cmdType = CmdType.getCtrlMode;
            switch (programControlCMDType)
            {
                case ProgramControlCmdType.StartOrRecovery:
                    cmdType = CmdType.remoteStart;
                    break;

                case ProgramControlCmdType.Pause:
                    cmdType = CmdType.remotePause;
                    break;

                case ProgramControlCmdType.Stop:
                    cmdType = CmdType.remoteStop;
                    break;

                default:
                    break;
            }
            ReceiveRobotInfo t = await SendAndWaitResponseAsync(cmdType, null, timeOut);
            var jsonMess = JsonConvert.SerializeObject(t);
            if (t != null)
            {

                if ((jsonMess).Contains("CmdDone"))
                {
                    return true;
                }
                else
                {
                    _log.Warn($"机器人 发送lua程序状态失败,返回字段未包含CmdDone,详细信息为{JsonConvert.SerializeObject(t)}");
                    return false;
                }

            }
            else
            {
                _log.Warn($"机器人 发送lua程序状态{programControlCMDType}失败");
            }
            return false;
        }

        /// <summary>
        /// 检查Lua是否准备完毕
        /// </summary>
        /// <param name="periodTime"></param>
        /// <returns></returns>
        private bool CheckLuaReadyInPeriodTime(int periodTime = 5 * 1000)
        {
            bool res = false;
            var startTime = DateTime.Now;
            while (true)
            {
                if (GetIsLuaReady(500))
                {
                    return true;
                }
                if ((DateTime.Now - startTime).TotalMilliseconds > periodTime)
                {
                    res = false;
                    break;
                }
                Thread.Sleep(100);

            }
            return res;
        }

        /// <summary>
        /// 获取Lua是否准备完毕
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public bool GetIsLuaReady(int timeout = 5000)
        {
            var res = GetIsLuaReadyAsync(timeout).Result;
            if (res == false)
            {
                _log.Info($"机器人 获取Lua是否准备完毕结果{res}");
            }
            else
            {
                _log.Debug($"机器人 获取Lua是否准备完毕结果{res}");
            }
            return res;
        }

        /// <summary>
        /// 异步任务，获取Lua是否准备完毕
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        private async Task<bool> GetIsLuaReadyAsync(int timeout = 500)
        {
            CmdType cmdType = CmdType.getLuaAppStatus;
            var t = await SendAndWaitResponseAsync(cmdType, null, timeout);
            var jsonMess = JsonConvert.SerializeObject(t);
            if (t != null)
            {
                var jsonModel1 = new { BRCAPI = _BRCAPI, ret = new { value = 8 }, id = 0 };
                var res = JsonConvert.DeserializeAnonymousType(jsonMess, jsonModel1);
                if (res.ret.value >= 0 && res.ret.value <= 1)
                {
                    return res.ret.value == 1 ? true : false;
                }
                else
                {
                    _log.Warn($"[{currenttime:yyyy-MM-dd HH:mm:ss:ff}] 机器人 获取Lua是否准备完毕失败,详细信息为{JsonConvert.SerializeObject(t)}");
                    return false;
                }
            }
            else
            {
                _log.Warn($"[{currenttime:yyyy-MM-dd HH:mm:ss:ff}] 机器人 获取Lua是否准备完毕失败");
            }
            return false;
        }

        /// <summary>
        /// 接收消息，只要有新消息就接收
        /// </summary>
        /// <param name="list"></param>
        private void HandleReceiveData(List<byte> list)
        {
            try
            {
                string info = Encoding.UTF8.GetString(list.ToArray());
                info = info.Remove(info.Length - 1);
                if (!info.Contains("\"ret\":[{\"errCode\":null,\"errMsg\":null}"))
                {
                    _log.Debug($"机器人 接收到的信息 {info}");

                }
                var model = new { BRCAPI = _BRCAPI, id = 1, ret = new object(), error = new { code = ErrorType.ParseError, message = "" }, reply = new object() };
                var receivemess = JsonConvert.DeserializeAnonymousType(info, model);
                if (receivemess.BRCAPI != _BRCAPI)
                {
                    _log.Warn($"机器人 接收到的信息{info},BRCAPI与当前驱动支持的不相同,接收到的BRCAPI:{receivemess.BRCAPI},驱动定义的{_BRCAPI}");
                    return;
                }
                if (receivemess.id == -1)
                {
                    _log.Warn($"错误的消息{info}");
                    return;
                }
                ReceiveRobotInfo receiveRobotInfo = JsonConvert.DeserializeObject<ReceiveRobotInfo>(info);
                _receiveMessages.Add(receiveRobotInfo);
                return;
            }
            catch (Exception ex)
            {
                _log.Error($"机器人 接收到的信息{Encoding.UTF8.GetString(list.ToArray())}出错：{ex.Message}");
            }
        }

        /// <summary>
        /// 请求机器人心跳信息
        /// </summary>
        /// <param name="tokenSource"></param>
        private void SendHeart(CancellationTokenSource tokenSource)
        {
            Task.Factory.StartNew(async () =>
            {
                await Task.Delay(3000, tokenSource.Token);
                while (true)
                {
                    try
                    {
                        if (_communication != null && _communication.IsConnected && _isNeedSendHeart)
                        {
                            var res = GetErrorInfoAsync(5000).Result;
                        }
                    }
                    catch (Exception ex)
                    {
                        _log.Error($"机器人 发送心跳出错：{ex.Message}");
                    }
                    await Task.Delay(1 * 800, tokenSource.Token);
                }
            }, tokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        /// <summary>
        /// 异步任务，获取机器人心跳信息
        /// </summary>
        /// <param name="timeOut"></param>
        /// <returns></returns>
        private async Task<List<ErrorInfo>> GetErrorInfoAsync(int timeOut = 5000)
        {
            CmdType cmdType = CmdType.getRobotErr;
            ReceiveRobotInfo temp = null;
            try
            {
                temp = await SendAndWaitResponseAsync(cmdType, null, timeOut);
            }
            catch (Exception ex)
            {
                _log.Error($"机器人 获取错误信息失败：{ex.Message}");
                return null;
            }
            var jsonMess = JsonConvert.SerializeObject(temp);
            if (temp != null)
            {

                if (jsonMess.Contains("{\"errCode\":null,\"errMsg\":null}"))
                {
                    _errorInfos = "";
                    return null;
                }
                else if (jsonMess.Contains("\"error\":{"))
                {
                    _log.Warn($"发送的指令错误,获取错误信息不成功，发送的指令类别为{cmdType}，错误信息为：{jsonMess}");
                    return null;
                }
                else
                {
                    List<ErrorInfo> errs = JsonConvert.DeserializeObject<List<ErrorInfo>>(JsonConvert.SerializeObject(temp.ret));
                    _errorInfos = "";
                    var temperrs = "";
                    foreach (var item in errs)
                    {
                        temperrs += $"错误编号:{item.errCode}.错误描述:{item.errMsg}" + "\r\n";
                    }
                    if (errs.Count(x => (x.errCode == 3611)||(x.errCode == 3608)) > 0)
                    {
                        _errorInfos = "急停按钮已触发";
                    }
                    else if (errs.Count(x => x.errCode == 3873) > 0)
                    {
                        _errorInfos = "机器人发生碰撞";
                    }
                    else
                    {
                        _errorInfos = "未知异常";
                    }
                    if (_errorInfos == _tempErr) //避免多次重复打印错误日志
                    {
                        return errs;
                    }
                    _tempErr = _errorInfos;
                    _log.Error($"获取到错误信息{temperrs} {_errorInfos}");
                    return errs;
                }
            }
            else
            {
                _log.Warn($"获取错误信息失败!{JsonConvert.SerializeObject(temp)}");
            }
            return null;
        }

        /// <summary>
        /// 发送消息之后等待回应消息
        /// </summary>
        /// <param name="cmdType"></param>
        /// <param name="param"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private async Task<ReceiveRobotInfo> SendAndWaitResponseAsync(CmdType cmdType, object param, int timeout = 5 * 60 * 1000)
        {
            GenerateCommand(cmdType, param, out int id, out string jsonStr);
            bool result = Send(jsonStr);
            if (!result)
            {
                return null;
            }
            ReceiveRobotInfo receiveRobotInfo = await FindResponseAsync(id, timeout, cmdType, GetRequestNeedReturnTimes(cmdType));
            return receiveRobotInfo;
        }

        /// <summary>
        /// 找到对应cmdtype的回复消息，注意有些命令比如setSysVarI需要收到状态回发消息才算成功，也就是returnTimes=2
        /// </summary>
        /// <param name="id"></param>
        /// <param name="timeout"></param>
        /// <param name="cmdType"></param>
        /// <param name="returnTimes"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private async Task<ReceiveRobotInfo> FindResponseAsync(int id, int timeout, CmdType cmdType, int returnTimes = 1)
        {
            return await Task.Run(() =>
            {
                var timeNow = DateTime.Now;
                while (true)
                {
                    if (_communication.IsConnected)
                    {
                        if (DateTime.Now.Subtract(timeNow).TotalMilliseconds > timeout)
                        {
                            _log.Warn($"等待{id}消息超时");
                            return null;
                        }

                        if (_receiveMessages.Count > 0 && (_receiveMessages.CountAll(x => x.id == id) == returnTimes || _receiveMessages.FindLast(x => x.id == id && x.error != null) != default))
                        {
                            var temp = _receiveMessages.FindLast(x => x.id == id);
                            if (temp != null)
                            {
                                _receiveMessages.RemoveAll(x => x.id == id);
                                return temp;
                            }
                        }
                        if (cmdType == CmdType.setSysVarI)
                        {
                            if (!string.IsNullOrEmpty(_errorInfos))
                            {
                                return null;
                            }
                            var temp = _receiveMessages.FindLast(x => x.id == id);
                            if (temp != null)
                            {
                                continue;
                            }
                            else
                            {
                                return null;
                            }
                        }

                        Thread.Sleep(100);
                    }
                    else
                    {
                        return null;
                    }
                }
            });
        }

        /// <summary>
        /// 生成需要发送的消息
        /// </summary>
        /// <param name="cmdType"></param>
        /// <param name="param"></param>
        /// <param name="id"></param>
        /// <param name="jsonStr"></param>
        private void GenerateCommand(CmdType cmdType, object param, out int id, out string jsonStr)
        {
            id = _generator.GetNextNumber();
            var jsonModel = new { BRCAPI = _BRCAPI, cmd = cmdType, @params = param, id = id };
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new StringEnumConverter());
            jsonStr = JsonConvert.SerializeObject(jsonModel, settings);
        }

        /// <summary>
        /// 发送消息，目前暂不支持并发消息，因此设置消息与消息之间发送间隔为200ms，避免粘包；使用锁机制避免多个线程同时发送出错
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        private bool Send(string info)
        {
            if (_communication != null && _communication.IsConnected)
            {
                lock (_lockerForSend)
                {
                    if (!info.Contains("{\"BRCAPI\":\"1.0\",\"cmd\":\"getRobotErr\""))
                    {
                        _log.Debug($"发送信息{info}");
                    }
                    var res = _communication.Send(Encoding.UTF8.GetBytes(info + "\n"));
                    Thread.Sleep(200);
                    return res;
                }
            }
            _log.Error($"机器人 通信对象为空,或者链接状态不对,communication={_communication},发送失败");
            return false;
        }

        /// <summary>
        /// 不同的命令对应不同的状态回发次数，需要等待接收的消息次数不同，如果次数为2，那么需要等待接收到第二次消息即状态回发消息以判断消息发送成功与否
        /// </summary>
        /// <param name="cmdType"></param>
        /// <returns></returns>
        private int GetRequestNeedReturnTimes(CmdType cmdType)
        {
            int res = 0;
            switch (cmdType)
            {
                case CmdType.setSysVarI:
                case CmdType.switchToAuto:
                case CmdType.switchToManual:
                case CmdType.remoteEnable:
                case CmdType.remoteDisable:
                case CmdType.remoteStart:
                case CmdType.remotePause:
                case CmdType.remoteStop:
                case CmdType.remoteClearError:
                case CmdType.moveBackwards:
                    return 2;


                case CmdType.getCtrlMode://无状态回发
                case CmdType.getEnableStatus://无状态回发
                case CmdType.getProgState://无状态回发
                case CmdType.getSysVarI://无状态回发
                case CmdType.getRobotErr://无状态回发
                case CmdType.getJointInfo://无状态回发
                case CmdType.getZeroConfig://无状态回发
                default:
                    return 1;
            }
        }

        /// <summary>
        /// 异步任务，机器人清错
        /// </summary>
        /// <param name="timeOut"></param>
        /// <returns></returns>
        private async Task<bool> ClearErrorsAsync(int timeOut = 5000)
        {
            CmdType cmdType = CmdType.remoteClearError;

            var t = await SendAndWaitResponseAsync(cmdType, null, timeOut);
            var jsonMess = JsonConvert.SerializeObject(t);
            if (t != null)
            {
                if ((jsonMess).Contains("CmdDone"))
                {
                    return true;
                }
                else
                {
                    _log.Warn($"远程清错失败,返回字段未包含CmdDone,详细信息为{JsonConvert.SerializeObject(t)}");
                    return false;
                }
            }
            else
            {
                _log.Warn($"机器人 发送远程清错失败");
            }
            return false;
        }
    }
    public interface ILog 
    {
        void Debug(string message);
        void Info(string message);
        void Warn(string message);
        void Error(string message);
    }

    public class Log : ILog
    {
        public void Debug(string message)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss:ff}] {message}");
            Console.ResetColor();
        }

        public void Info(string message)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss:ff}] {message}");
            Console.ResetColor();
        }

        public void Warn(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss:ff}] {message}");
            Console.ResetColor();
        }
        public void Error(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss:ff}] {message}");
            Console.ResetColor();
        }
    }
}
