using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.ComponentModel;




namespace MassEffectTimer
{
    public class Waker
    {

        [DllImport("kernel32.dll")]
        public static extern SafeWaitHandle CreateWaitableTimer(IntPtr lpTimerAttributes, bool bManualReset, string lpTimerName);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWaitableTimer(SafeWaitHandle hTimer, [In] ref long pDueTime, int lPeriod, IntPtr pfnCompletionRoutine, IntPtr lpArgToCompletionRoutine, bool fResume);

        [System.Runtime.InteropServices.DllImport("Kernel32.dll", EntryPoint = "SetThreadExecutionState",
        CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = true)]
        public extern static EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE state);
        [System.FlagsAttribute]
        public enum PowerThreadRequirements : uint
        {
            ReleaseHold = 0x80000000,
            HoldSystem = (0x00000001 | ReleaseHold),
            HoldDisplay = (0x00000002 | ReleaseHold),
            HoldSystemAndDisplay = (HoldSystem | HoldDisplay | ReleaseHold),
        }

        [System.FlagsAttribute]
        public enum EXECUTION_STATE : uint //!< Add by KCL, [he] found this by searching SetThreadExecutionState on offline MSDN
        {
            /// Informs the system that the state being set should remain in effect until the next call
            /// that uses ES_CONTINUOUS and one of the other state flags is cleared. ///
            ES_CONTINUOUS = 0x80000000, ///

            /// Forces the display to be on by resetting the display idle timer. ///
            ES_DISPLAY_REQUIRED = 0x00000002, ///

            /// Forces the system to be in the working state by resetting the system idle timer. ///
            ES_SYSTEM_REQUIRED = 0x00000001,
        }
        ManualResetEvent _shutdownEvent = new ManualResetEvent(false);
        ManualResetEvent _pauseEvent = new ManualResetEvent(true);
        Thread _thread;
        public DateTime endtime;

        public Waker() { }

        public void SetEndtime(DateTime et)
        {
            endtime = et;
        }
        public void Start()
        {
            _thread = new Thread(DoWork);
            _thread.IsBackground = true;
            _thread.Start();
        }

        public void Pause()
        {
            _pauseEvent.Reset();
        }

        public void Resume()
        {
            _pauseEvent.Set();
        }

        public void Stop()
        {
            // Signal the shutdown event
            _shutdownEvent.Set();

            // Make sure to resume any paused threads
            _pauseEvent.Set();

            // Wait for the thread to exit
            _thread.Join();
        }

        public void DoWork()
        {
            while (true)
            {
                _pauseEvent.WaitOne(Timeout.Infinite);

                if (_shutdownEvent.WaitOne(0))
                    break;

                TimeSpan five= TimeSpan.FromSeconds(5.00);
                DateTime endtime1 = endtime.Subtract(five);
                //DateTime utc = DateTime.Now.AddMinutes(1);
                long duetime = endtime1.ToFileTime();
  
                using (SafeWaitHandle handle = CreateWaitableTimer(IntPtr.Zero, true, "MyWaitabletimer"))
                {
                    if (SetWaitableTimer(handle, ref duetime, 0, IntPtr.Zero, IntPtr.Zero, true))
                    {
                        using (EventWaitHandle wh = new EventWaitHandle(false, EventResetMode.AutoReset))
                        {
                            wh.SafeWaitHandle = handle;
                            wh.WaitOne();
                            SetThreadExecutionState(EXECUTION_STATE.ES_DISPLAY_REQUIRED); //| EXECUTION_STATE.ES_CONTINUOUS | EXECUTION_STATE.ES_SYSTEM_REQUIRED);


                            //Then when you don't need the monitor anymore   
                            //Allow monitor to power down   
                            // SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);
                        }
                    }
                    else
                    {
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                    }
                }

                // You could make it a recursive call here, setting it to 1 hours time or similar
                //Console.WriteLine("Wake up call");
                //Console.ReadLine();
            }
        }
    }
}
