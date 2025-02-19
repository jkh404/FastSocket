using System;
using System.Threading;
using System.Threading.Tasks;

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
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isStart;
        private object _lockObj=new object();


        public SingleThreadTimer(TimerCallback timerCallback, int dueTime, int period, object? callObjArg=null)
        {
            _timerCallback=timerCallback;
            _callObjArg=callObjArg;
            _dueTime=dueTime;
            _period=period;
            _cancellationTokenSource=new CancellationTokenSource();
        }

        private void ThreadFunc(object? obj)
        {
            Task.Delay(_dueTime, _cancellationTokenSource.Token).Wait();
            while (_isStart && !_cancellationTokenSource.Token.IsCancellationRequested && _timerCallback!=null)
            {
                _timerCallback?.Invoke(obj);
                Task.Delay(_period, _cancellationTokenSource.Token).Wait();
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
                _cancellationTokenSource.Cancel();
            }
        }
    }
}
