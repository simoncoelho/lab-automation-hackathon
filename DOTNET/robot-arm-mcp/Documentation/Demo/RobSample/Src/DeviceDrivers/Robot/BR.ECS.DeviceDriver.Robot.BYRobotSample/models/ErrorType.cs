using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BR.ECS.DeviceDriver.Robot.BYRobotSample.models
{
    public enum ErrorType
    {
        /*
         错误码	消息	描述
    01	Invalid request	无效请求，协议版本错误
    02	cmd not valid	该命令无效命令，服务端不支持
    03	index exceeded allowed range	设置或获取系统变量时的索引值超出允许的范围
    04	Internal error	服务端发生内部错误
    05	Parse error: xxxxxx	收到不符合规范的消息，导致解析错误。
    06	Operation failed, controller is in safty mode	控制器处于安全模式，不允许此操作
    07	Operation failed, internal error	控制器内部故障
    08	Operation failed, can not move in disable state	控制器处于非使能状态，不支持机器人运动
    09	Operation failed, can not clear error in enabled status	控制器处于使能状态，不支持清错操作
    10	Operation failed, Motion enable/disable must be in auto mode	使能/禁能操作必须在自动模式下进行。
    11	Operation failed, controller is in Rec-Play state	控制器处于Rec-Play状态，不允许执行指令回退命令
    12	Operation failed, lua program is running	机器人Lua程序正在运行，不允许执行指令回退命令
    13	Operation canceled, move ins buffer is empty	机器人运动指令缓存为空，取消执行指令回退命令
    14	Operation failed, robot is moving now	机器人正在运动中，不允许执行指令回退命令
    15	invalid xxx params	内部读取的某参数非法
    16	Command does not support this type of robot	指令不支持该类型机器人
    17	Operation failed,can not stop moving backwards in other mode	非回退模式下，不允许执行暂停回退指令
         */

        // 无效请求，协议版本错误
        InvalidRequest = 1,

        // 该命令无效，服务端不支持
        CmdNotValid = 2,

        // 设置或获取系统变量时的索引值超出允许的范围
        IndexExceededAllowedRange = 3,

        // 服务端发生内部错误
        InternalError = 4,

        // 收到不符合规范的消息，导致解析错误
        ParseError = 5,

        // 控制器处于安全模式，不允许此操作
        ControllerSafeMode = 6,

        // 控制器内部故障
        ControllerInternalError = 7,

        // 控制器处于非使能状态，不支持机器人运动
        ControllerDisabled = 8,

        // 控制器处于使能状态，不支持清错操作
        ClearErrorNotSupportedInEnabledState = 9,

        // 使能/禁能操作必须在自动模式下进行
        MotionEnableDisableAutoMode = 10,

        // 控制器处于Rec-Play状态，不允许执行指令回退命令
        ControllerRecPlayState = 11,

        // 机器人Lua程序正在运行，不允许执行指令回退命令
        RobotLuaProgramRunning = 12,

        // 机器人运动指令缓存为空，取消执行指令回退命令
        RobotBufferEmpty = 13,

        // 机器人正在运动中，不允许执行指令回退命令
        RobotMoving = 14,

        // 内部读取的某参数非法
        InvalidParams = 15,

        // 指令不支持该类型机器人
        CommandNotSupportRobotType = 16,

        // 非回退模式下，不允许执行暂停回退指令
        CannotStopMovingBackwardsInOtherMode = 17
    }
}
