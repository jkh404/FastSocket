using System;
using System.Threading;

namespace FastSocket
{
    /// <summary>
    /// 单线程计时器
    /// </summary>
    public class SingleThreadTimer
    {
        private Thread? _thread;
        private  TimerCallback _timerCallback;
        private readonly object? _callObjArg;
        private int _dueTime;
        private int _period;
        private bool _isStart;
        private object _lockObj=new object();

        private DateTime lastTime; 

        public SingleThreadTimer(TimerCallback timerCallback, int dueTime, int period, object? callObjArg=null)
        {
            _timerCallback=timerCallback;
            _callObjArg=callObjArg;
            _dueTime=dueTime;
            _period=period;
            
        }

        private void ThreadFunc(object? obj)
        {
            Thread.Sleep(_dueTime);
            var _tempPeriod=_period/10;
            while (_isStart) 
            {
                try
                {
                    _timerCallback?.Invoke(obj);
                }
                catch (Exception)
                {

                }
                lastTime=DateTime.UtcNow;
                for (int i = 0; i < 10; i++)
                {
                    Thread.Sleep(_tempPeriod);
                    if (i==9) continue;
                    var timeSpan=DateTime.UtcNow - lastTime;
                    int tick = (int)(_period-timeSpan.TotalMilliseconds);
                    if (tick<0) break;
                    if (tick<_tempPeriod)
                    {
                        Thread.Sleep(tick);
                        break;
                    }
                }
            }
        }

        public void Start() 
        {
            lock (_lockObj)
            {
                _thread=new Thread(ThreadFunc);
                _thread.IsBackground=true;
                _isStart=true;
                _thread.Start(_callObjArg);
            }

        }
        public void Stop()
        {
            lock (_lockObj)
            {
                _isStart=false;
            }
        }
    }
}
