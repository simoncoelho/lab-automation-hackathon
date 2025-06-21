using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BR.ECS.DeviceDriver.Robot.BYRobotSample.models
{
    public class ReceiveRobotInfo : IEquatable<ReceiveRobotInfo>
    {
        // var model = new { BRCAPI = _BRCAPI, id = 1, ret = new object(), error = new { code = ErrorType.ParseError, message = "" }, reply = new object() };
        public string BRCAPI;
        public int id;
        public object ret;
        public ErrorInfoForMessage error;
        public object reply;

        public override bool Equals(object obj)
        {
            return Equals(obj as ReceiveRobotInfo);
        }

        public bool Equals(ReceiveRobotInfo other)
        {
            return !(other is null) &&
                   BRCAPI == other.BRCAPI &&
                   id == other.id &&
                   EqualityComparer<object>.Default.Equals(ret, other.ret) &&
                   EqualityComparer<ErrorInfoForMessage>.Default.Equals(error, other.error) &&
                   EqualityComparer<object>.Default.Equals(reply, other.reply);
        }

        public override int GetHashCode()
        {
            int hashCode = 191131794;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(BRCAPI);
            hashCode = hashCode * -1521134295 + id.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<object>.Default.GetHashCode(ret);
            hashCode = hashCode * -1521134295 + EqualityComparer<ErrorInfoForMessage>.Default.GetHashCode(error);
            hashCode = hashCode * -1521134295 + EqualityComparer<object>.Default.GetHashCode(reply);
            return hashCode;
        }

        public static bool operator ==(ReceiveRobotInfo left, ReceiveRobotInfo right)
        {
            return EqualityComparer<ReceiveRobotInfo>.Default.Equals(left, right);
        }

        public static bool operator !=(ReceiveRobotInfo left, ReceiveRobotInfo right)
        {
            return !(left == right);
        }
    }
}
