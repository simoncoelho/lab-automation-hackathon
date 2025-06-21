using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BR.ECS.DeviceDriver.Robot.BYRobotSample.models
{
    public class SafeList<T>
    {
        private readonly object _locker = new object();
        private readonly List<T> list = new List<T>();

        public void Add(T t)
        {
            lock (_locker)
            {
                list.Add(t);
            }
        }

        public void Remove(T t)
        {
            lock (_locker)
            {
                list.Remove(t);
            }
        }

        public void Clear()
        {
            lock (_locker)
            {
                list.Clear();
            }
        }

        //获取符合条件的元素个数
        public int CountAll(Predicate<T> match)
        {
            lock (_locker)
            {
                return list.FindAll(match).Count;
            }
        }

        public int Count
        {
            get
            {
                lock (_locker)
                {
                    return list.Count;
                }
            }
        }

        public T this[int index]
        {
            get
            {
                lock (_locker)
                {
                    return list[index];
                }
            }
        }

        //查找最后一个符合条件的元素
        public T FindLast(Predicate<T> match)
        {
            lock (_locker)
            {
                return list.FindLast(match);
            }
        }

        //按条件查询
        public List<T> FindAll(Predicate<T> match)
        {
            lock (_locker)
            {
                return list.FindAll(match);
            }
        }

        //按条件删除
        public int RemoveAll(Predicate<T> match)
        {
            lock (_locker)
            {
                return list.RemoveAll(match);
            }
        }
    }
}
