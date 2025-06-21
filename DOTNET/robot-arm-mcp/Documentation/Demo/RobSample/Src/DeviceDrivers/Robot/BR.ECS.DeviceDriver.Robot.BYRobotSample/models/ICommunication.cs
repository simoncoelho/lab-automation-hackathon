using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BR.ECS.DeviceDriver.Robot.BYRobotSample.models
{
    public interface ICommunication
    {
        bool IsConnected { get; }

        Action<bool> IsConnectedAction { get; set; }

        Action<List<byte>> OnReceive { get; set; }

        bool Close();

        bool Init(string ipAddress, int port, IReceiveFilter receiveFilter = null);

        bool Open(int communicationTimeOut = 0);

        List<byte> Receive(int span = 0);

        bool Send(byte[] data);

        bool SendTo(object target, byte[] data);
    }
}
