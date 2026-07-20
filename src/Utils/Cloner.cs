using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace RealismCommonLib.Utils
{
    public static class Cloner
    {
        public static T ShallowClone<T>(T obj)
        {
            var method = typeof(object).GetMethod(
                "MemberwiseClone",
                BindingFlags.Instance | BindingFlags.NonPublic
            );

            return (T)method.Invoke(obj, null);
        }
    }
}
