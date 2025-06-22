using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using robot_arm_mcp.Helpers;

namespace robot_arm_mcp
{
    public class RobotController
    {
        public RobotController() { }

        public bool IsConnected { get; private set; } = false;

        public void ConnectToRobot()
        {
            if (!IsConnected)
            {
                Console.WriteLine("Connecting to robot...");
                // Logic to connect to the robot
                IsConnected = true;
            }
        }

        public void ShutDownRobot() 
        {
            IsConnected = false;
        }

        public void MoveToSafeLocation()
        {
            ConnectToRobot();
            // Logic to move the robot arm to a safe location
            Console.WriteLine("Moving to safe location...");

        }

        public void MoveToJointPositions(JointPosition jointPositions)
        {
            ConnectToRobot();
            // Logic to move the robot arm to specified joint positions
            Console.WriteLine($"Moving to joint positions: {jointPositions.Joint1}, {jointPositions.Joint2}, {jointPositions.Joint3}, {jointPositions.Joint4}, {jointPositions.Gripper}");
        }

        public void CloseGripper()
        {
            ConnectToRobot();
            // Logic to close the gripper
            Console.WriteLine("Closing gripper...");
        }

        public void OpenGripper()
        {
            ConnectToRobot();
            // Logic to open the gripper
            Console.WriteLine("Opening gripper...");
        }
    }
}
