using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace robot_arm_mcp.Helpers
{
    public class JointPosition
    {
        string Joint1 { get; set; }
        string Joint2 { get; set; }
        string Joint3 { get; set; }
        string Joint4 { get; set; }
        string Joint5 { get; set; }
        string Joint6 { get; set; }
        public JointPosition(string joint1, string joint2, string joint3, string joint4, string joint5, string joint6)
        {
            Joint1 = joint1;
            Joint2 = joint2;
            Joint3 = joint3;
            Joint4 = joint4;
            Joint5 = joint5;
            Joint6 = joint6;
        }

    }
}
