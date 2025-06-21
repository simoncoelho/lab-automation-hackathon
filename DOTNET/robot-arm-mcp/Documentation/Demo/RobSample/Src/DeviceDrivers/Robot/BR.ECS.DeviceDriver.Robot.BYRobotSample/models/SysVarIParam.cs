using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BR.ECS.DeviceDriver.Robot.BYRobotSample.models
{
    public class SysVarIParam
    {
        #region Public Constructors

        public SysVarIParam(int index, object value)
        {
            this.index = index;
            this.value = value;
        }

        #endregion Public Constructors



        #region Public Properties

        public int index { set; get; }
        public object value { set; get; }

        #endregion Public Properties
    }
}
