using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace RealismCommonLib.Utils
{
    public static class MathUtils
    {
        public static Vector3 ClampVector(Vector3 value, Vector3 min, Vector3 max)
        {
            return new Vector3(
                Mathf.Clamp(value.x, min.x, max.x),
                Mathf.Clamp(value.y, min.y, max.y),
                Mathf.Clamp(value.z, min.z, max.z)
            );
        }

        public static bool AreVector2sEqual(Vector2 a, Vector2 b, float epsilon = 0.001f)
        {
            return Vector2.Distance(a, b) < epsilon;
        }

        public static bool IsGreaterThanOrEqualTo(float a, float b, float epsilon = 0.0001f)
        {
            return IsGreaterThan(a, b, epsilon) || AreFloatsEqual(a, b, epsilon);
        }

        public static bool IsLessThanOrEqualTo(float a, float b, float epsilon = 0.0001f)
        {
            return IsLessThan(a, b, epsilon) || AreFloatsEqual(a, b, epsilon);
        }

        public static bool IsLessThan(float a, float b, float epsilon = 0.0001f)
        {
            return (b - a) > epsilon;
        }

        public static bool IsGreaterThan(float a, float b, float epsilon = 0.0001f)
        {
            return (a - b) > epsilon;
        }

        public static bool AreFloatsEqual(float a, float b, float epsilon = 0.0001f)
        {
            float difference = Math.Abs(a - b);
            return difference < epsilon;
        }
    }
}
