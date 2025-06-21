using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BR.ECS.DeviceDriver.Robot.BYRobotSample.models
{
    public class DeviceException : Exception
    {
        public int ErrorCode { get; set; } = 6001;


        public string ErrMessage { get; set; } = "故障";


        public string ErrInnerMessage { get; set; }

        public string Module { get; set; } = "驱动";


        public DeviceException(int errorCode, string message, string errInnerMessage)
            : base(errInnerMessage)
        {
            ErrorCode = errorCode;
            ErrMessage = message;
            ErrInnerMessage = errInnerMessage;
        }

        public DeviceException(int errorCode, string message, string errInnerMessage, Exception innerException)
            : base(errInnerMessage, innerException)
        {
            ErrorCode = errorCode;
            ErrMessage = message;
            ErrInnerMessage = errInnerMessage;
        }

        public DeviceException(Exception ex)
            : base(ex.Message)
        {
            ErrMessage = "故障";
            ErrInnerMessage = ((ex.InnerException == null) ? ex.Message : ex.InnerException.Message);
        }

        public DeviceException(string message, string innerException)
            : base(innerException)
        {
            ErrMessage = message;
            ErrInnerMessage = innerException;
        }

        public override string ToString()
        {
            return ToJson();
        }

        public string ToJson()
        {
            try
            {
                if (!string.IsNullOrEmpty(ErrInnerMessage))
                {
                    return $"{{\"ErrorCode\":{ErrorCode},\"ErrMessage\":\"{EscapeJson(ErrMessage)}\",\"ErrInnerMessage\":\"{EscapeJson(ErrInnerMessage)}\",\"Module\":\"{EscapeJson(Module)}\",\"StackTrace\":\"{EscapeJson(StackTrace)}\"}}";
                }

                return "";
            }
            catch (Exception)
            {
                throw;
            }
        }

        private string EscapeJson(string s)
        {
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r\n", "\\r\\n")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r");
        }
    }
}
