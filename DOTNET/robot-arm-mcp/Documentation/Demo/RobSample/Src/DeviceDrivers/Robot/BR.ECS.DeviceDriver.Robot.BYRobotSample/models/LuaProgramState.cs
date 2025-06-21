namespace BR.ECS.DeviceDriver.Robot.BYRobotSample.models
{

    #region Public Enums

    public enum LuaProgramState
    {/*
      当前机器人程序运行状态。
0：程序停止
1：程序运行中
2：程序暂停
3：程序出错
其他值为非法值。
      */
        Stop = 0,
        Running = 1,
        Pause = 2,
        Error = 3
    }

    #endregion Public Enums

}