﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AcroDD_Cart
{
    public class TickTimer
    {
        TimerCallback _cb;
        Stopwatch _sw;
        int _dueTime;
        int _period;
        bool _loop = true;
        Thread _task;

        public TickTimer(TimerCallback callback, object state, int dueTime, int period)
        {
            _cb = callback;
            _dueTime = dueTime;
            _period = period;
            _sw = new Stopwatch();
            _task = new Thread(onTimer);
            _task.Start(state);
        }
        public TickTimer(TimerCallback callback, int period)
        {
            _cb = callback;
            _period = period;
            _sw = new Stopwatch();
            _task = new Thread(onTimer);
            //object state = null;
            //_task.Start(state);
        }
        public void Start(object state = null)
        {
            _dueTime = 0;
            _loop = true;
            _task.Start(state);
        }
        public void Restart()
        {
            _task.Resume();
        }

        public void Stop()
        {
            //_loop = false;
            _task.Suspend();
        }
        public void Exit()
        {
            _task.Resume();
            _loop = false;
        }

        private void onTimer(object state)
        {
            Thread.Sleep(_dueTime);
            _sw.Restart();
            while (_loop)
            {
                long msec = _sw.ElapsedMilliseconds;
                int rest = _period - (int)(msec % _period);
                // 200msecだけ余らせてスリープ
                if (rest > 200)
                {
                    Thread.Sleep(rest - 200);
                }
                // 200msecの間、ちょうどになるまでループで待つ
                while (true)
                {
                    if (!_loop) break;
                    //Console.WriteLine(_sw.ElapsedMilliseconds);
                    if (_sw.ElapsedMilliseconds >= msec + rest)
                    {
                        break;
                    }
                }
                if (_cb != null)
                {
                    _cb(state);
                }
            }
            _sw.Stop();
        }
    }
}
