using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BR.ECS.DeviceDriver.Robot.BYRobotSample.models
{
    public enum CmdType
    {

        /// <summary>
        /// 设置整型系统变量
        /// </summary>
        setSysVarI,

        /// <summary>
        /// 获取整型系统变量
        /// </summary>
        getSysVarI,

        /// <summary>
        /// 切换到自动操作模式
        /// </summary>
        switchToAuto,

        /// <summary>
        /// 切换到手动操作模式
        /// </summary>
        switchToManual,

        /// <summary>
        /// 远程使能机器人
        /// </summary>
        remoteEnable,

        /// <summary>
        /// 远程禁能机器人
        /// </summary>
        remoteDisable,

        /// <summary>
        /// 远程启动程序/恢复运行程序
        /// </summary>
        remoteStart,

        /// <summary>
        /// 远程暂停程序
        /// </summary>
        remotePause,

        /// <summary>
        /// 远程停止程序
        /// </summary>
        remoteStop,

        /// <summary>
        /// 远程清除错误
        /// </summary>
        remoteClearError,

        /// <summary>
        /// 获取当前机器人控制模式
        /// </summary>
        getCtrlMode,

        /// <summary>
        /// 获取机器人的使能状态。
        /// </summary>
        getEnableStatus,

        /// <summary>
        /// 获取当前机器人程序的运行状态。
        /// </summary>
        getProgState,

        /// <summary>
        /// 命令机器人对刚才执行过的动作进行回退动作。
        /// </summary>
        moveBackwards,

        /// <summary>
        /// 获取机器人当前位置信息
        /// </summary>
        getJointInfo,

        /// <summary>
        /// 获取机器人参考零位
        /// </summary>
        getZeroConfig,

        /// <summary>
        /// 获取错误信息
        /// </summary>
        getRobotErr,
        /// <summary>
        /// 设置系统参数
        /// </summary>
        setSysParaI,
        /// <summary>
        /// 获取系统参数
        /// </summary>
        getSysParaI,
        /// <summary>
        /// 设置浮点参数
        /// </summary>
        setSysParaF,
        /// <summary>
        /// 获取浮点参数
        /// </summary>
        getSysParaF,
        /// <summary>
        /// 获取lua是否初始化
        /// </summary>
        getLuaAppStatus,
        /// <summary>
        /// 获取位姿
        /// </summary>
        getFrameWithU0T0
    }
}
