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
        private float _interval;
        private float _elapsed;

        public bool IsActive { get; set; } = false;

        public TimeGate(float interval)
        {
            _interval = interval;
        }

        public void Reset()
        {
            _elapsed = 0f;
            IsActive = true;
        }

        public void Update()
        {
            _elapsed += Time.deltaTime;
            if (_elapsed >= _interval)
            {
                _elapsed = 0f;
                IsActive = false;
            }
        }
    }
}
