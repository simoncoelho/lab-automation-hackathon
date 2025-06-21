using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BR.ECS.DeviceDriver.Robot.BYRobotSample.models
{
    public class SocketCommunication : ICommunication
    {
        private Socket _socket;  // Socket对象
        private IPEndPoint _endPoint;  // 远程服务器的IP和端口
        private bool _isConnected = false;  // 连接状态

        // 是否已连接
        public bool IsConnected => _isConnected;

        // 连接状态变化的回调
        public Action<bool> IsConnectedAction { get; set; }

        // 接收数据的回调
        public Action<List<byte>> OnReceive { get; set; }

        // 初始化Socket连接的参数
        public bool Init(string ipAddress, int port, IReceiveFilter receiveFilter = null)
        {
            try
            {
                // 设置服务器的IP和端口
                _endPoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);
                // 创建Socket对象
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"初始化失败: {ex.Message}");
                return false;
            }
        }

        // 打开Socket连接，包含可选的超时时间
        public bool Open(int communicationTimeOut = 0)
        {
            try
            {
                
                // 尝试连接服务器
                _socket.Connect(_endPoint);

                if (_socket.Connected)
                {
                    _isConnected = true;
                    // 触发连接成功的回调
                    IsConnectedAction?.Invoke(true);
                    Task.Run(() =>
                    {
                        while (true)
                        {
                            Receive();
                        }
                    });
                }

                return _isConnected;
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"连接失败: {ex.Message} 错误代码: {ex.SocketErrorCode} {_endPoint.Address} {_endPoint.Port}");
                return false;
            }
        }

        // 发送数据到服务器
        public bool Send(byte[] data)
        {
            try
            {
                if (_isConnected)
                {
                    // 发送字节数据
                    _socket.Send(data);
                    return true;
                }
                return false;
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"发送失败: {ex.Message}");
                return false;
            }
        }

        // 接收来自服务器的数据
        public List<byte> Receive(int span = 0)
        {
            List<byte> receivedData = new List<byte>();

            try
            {
                if (_isConnected)
                {
                    byte[] buffer = new byte[1024];  // 数据缓冲区
                    int receivedBytes = _socket.Receive(buffer);  // 接收数据字节数

                    // 将接收到的字节数据存入列表
                    for (int i = 0; i < receivedBytes; i++)
                    {
                        receivedData.Add(buffer[i]);
                    }

                    // 触发接收回调
                    OnReceive?.Invoke(receivedData);
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"接收失败: {ex.Message}");
            }

            return receivedData;
        }

        // 关闭Socket连接
        public bool Close()
        {
            try
            {
                if (_socket != null && _isConnected)
                {
                    if (_socket.Poll(1000, SelectMode.SelectRead) && _socket.Available == 0)
                    {
                        // 没有待处理数据，正常关闭
                        _socket.Shutdown(SocketShutdown.Both);
                    }
                    //_socket.Shutdown(SocketShutdown.Both);  // 关闭发送和接收
                    _socket.Close();  // 关闭Socket
                    _isConnected = false;
                    // 触发断开连接回调
                    IsConnectedAction?.Invoke(false);
                }
                return true;
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"关闭失败: {ex.Message}");
                return false;
            }
        }

        // 暂未实现的SendTo方法
        public bool SendTo(object target, byte[] data)
        {
            throw new NotImplementedException();
        }
    }
}
