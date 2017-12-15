#if NET20

using System;
using System.Runtime.InteropServices;
using System.Threading;
using JointCode.Common.Helpers;

namespace JointCode.Common.Threading
{
    public struct SpinWait
    {
        const int YieldFrequency = 4000;
        volatile int _count;

        public void SpinOnce()
        {
            if (SystemHelper.HasMultiProcessors)
            {
                _count = ++_count % Int32.MaxValue;
                var numeric = _count % YieldFrequency;
                if (numeric > 0)
                {
                    Thread.SpinWait((int)(1f + (numeric * 0.032f)));
                }
                else
                {
                    //Thread.Sleep(0);
                    SwitchToThread();
                }
            }
            else
            {
                //Thread.Sleep(0);
                SwitchToThread();
            }
        }

        public void Reset()
        {
            _count = 0;
        }

        public int Count
        {
            get { return _count; }
        }

        [DllImport("Kernel32", ExactSpelling = true)]
        static extern void SwitchToThread();
    }
}
#endif