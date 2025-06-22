using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace robot_arm_mcp.Helpers
{
    public class JointPosition
    {
        public string Joint1 { get; set; }
        public string Joint2 { get; set; }
        public string Joint3 { get; set; }
        public string Joint4 { get; set; }
        public string Gripper { get; set; }
        public JointPosition(string joint1, string joint2, string joint3, string joint4, string gripper)
        {
            Joint1 = joint1;
            Joint2 = joint2;
            Joint3 = joint3;
            Joint4 = joint4;
            Gripper = gripper;
        }

    }
}
