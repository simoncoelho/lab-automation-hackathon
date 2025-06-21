using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BR.ECS.DeviceDriver.Robot.BYRobotSample.models
{
    public class SequentialNumberGenerator
    {
        #region Private Fields

        private readonly object lockObj = new object();
        private int currentNumber;
        private int maxNum;

        #endregion Private Fields

        #region Public Constructors

        public SequentialNumberGenerator(int maxNumber)
        {
            if (maxNumber <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxNumber), "maxNumber must be greater than zero.");
            currentNumber = 0;
            maxNum = maxNumber;
        }

        #endregion Public Constructors



        #region Public Properties

        public int CurrentNumber
        { get { return currentNumber; } }

        #endregion Public Properties



        #region Public Methods

        public int GetNextNumber()
        {
            lock (lockObj)
            {
                if (currentNumber >= int.MaxValue)
                {
                    currentNumber = 0;
                }
                if (currentNumber >= maxNum)
                {
                    currentNumber = 0;
                }
                currentNumber++;

                return currentNumber;
            }
        }

        #endregion Public Methods
    }
}
