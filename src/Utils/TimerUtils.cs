using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace RealismCommonLib.Utils
{
    public class TimeInterval
    {
        private float _interval;
        private float _elapsed;

        public TimeInterval(float interval)
        {
            _interval = interval;
        }

        public bool Ready()
        {
            _elapsed += Time.deltaTime;
            if (_elapsed >= _interval)
            {
                _elapsed = 0f;
                return true;
            }
            return false;
        }
    }

    public class TimeGate
    {
        private float _elapsed;

        public float Interval { get; set; }

        public bool IsActive { get; set; } = false;

        public TimeGate(float interval)
        {
            Interval = interval;
        }

        public void Reset()
        {
            _elapsed = 0f;
            IsActive = true;
        }

        public void Update()
        {
            _elapsed += Time.deltaTime;
            if (_elapsed >= Interval)
            {
                _elapsed = 0f;
                IsActive = false;
            }
        }
    }

    public class DelayTimer
    {
        private float _elapsed;
        private bool _active;

        public float Duration { get; set; }
        public bool IsActive => _active;

        public DelayTimer(float duration)
        {
            Duration = duration;
        }

        public void Start()
        {
            _elapsed = 0f;
            _active = true;
        }

        public void Stop()
        {
            _active = false;
        }

        public bool Update()
        {
            if (!_active)
                return false;

            _elapsed += Time.deltaTime;

            if (_elapsed >= Duration)
            {
                _active = false;
                return true;
            }

            return false;
        }
    }
}
