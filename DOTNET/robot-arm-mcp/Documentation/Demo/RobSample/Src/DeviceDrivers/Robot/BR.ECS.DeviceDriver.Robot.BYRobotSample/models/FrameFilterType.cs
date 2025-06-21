using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BR.ECS.DeviceDriver.Robot.BYRobotSample.models
{
    public enum FrameFilterType
    {
        FixTail,
        FixHeaderAndTail,
        FixLength,
        FixHeaderAndLength,
        FixTailAndLength,
        FixedHeaderReceiveLengthFilter
    }
}
