using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BR.ECS.DeviceDriver.Robot.BYRobotSample.models
{
    public class FilterInfo : IEquatable<FilterInfo>, IEqualityComparer<FilterInfo>
    {
        public int EndByte { get; set; }

        public FrameFilterType FilterType { get; set; }

        public List<byte> Header { get; set; }

        public int Length { get; set; }

        public int NotFixLengthTailLength { get; set; }

        public int StartByte { get; set; }

        public List<byte> Tail { get; set; }

        public FilterInfo(FrameFilterType filterType, List<byte> header, int length, int endByte)
        {
            FilterType = filterType;
            Header = header;
            Length = length;
            EndByte = endByte;
        }

        public FilterInfo(FrameFilterType filterType, List<byte> header, List<byte> tail)
        {
            FilterType = filterType;
            Tail = tail;
            Header = header;
        }

        public FilterInfo(FrameFilterType filterType, List<byte> tail, int length)
        {
            FilterType = filterType;
            Tail = tail;
            Length = length;
        }

        public FilterInfo(FrameFilterType filterType, int startByte, int endByte, int notFixLengthTailLength)
        {
            FilterType = filterType;
            StartByte = startByte;
            EndByte = endByte;
            NotFixLengthTailLength = notFixLengthTailLength;
        }

        public FilterInfo(FrameFilterType filterType, List<byte> tail)
        {
            FilterType = filterType;
            Tail = tail;
        }

        public FilterInfo()
        {
        }

        public bool Equals(FilterInfo other)
        {
            if (other == null)
            {
                return false;
            }

            if (FilterType != other.FilterType)
            {
                return false;
            }

            if (!object.Equals(Header, other.Header))
            {
                return false;
            }

            if (Length != other.Length)
            {
                return false;
            }

            if (EndByte != other.EndByte)
            {
                return false;
            }

            if (NotFixLengthTailLength != other.NotFixLengthTailLength)
            {
                return false;
            }

            if (StartByte != other.StartByte)
            {
                return false;
            }

            if (!object.Equals(Tail, other.Tail))
            {
                return false;
            }

            return true;
        }

        public bool Equals(FilterInfo x, FilterInfo y)
        {
            return x.Equals(y);
        }

        public int GetHashCode(FilterInfo obj)
        {
            return obj.GetHashCode();
        }
    }
}
