using BepInEx.Configuration;
using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using static GClass2175;

namespace RealismCommonLib.Utils
{
    //TODO: make better
    public class Vector3Curve
    {
        public AnimationCurve XCurve;
        public AnimationCurve YCurve;
        public AnimationCurve ZCurve;

        public Vector3Curve(float x, float y, float z) 
        {
            XCurve = new AnimationCurve();
            YCurve = new AnimationCurve();
            ZCurve = new AnimationCurve();

            XCurve.AddKey(0f, 0f);
            YCurve.AddKey(0f, 0f);
            ZCurve.AddKey(0f, 0f);

            XCurve.AddKey(1f, x);
            YCurve.AddKey(1f, y);
            ZCurve.AddKey(1f, z);
        }

        public Vector3 Evaluate(float t)
        {
            return new Vector3(
                XCurve.Evaluate(t),
                YCurve.Evaluate(t),
                ZCurve.Evaluate(t)
            );
        }
    }

    public class Vector3CurveReverse
    {
        public AnimationCurve XCurve;
        public AnimationCurve YCurve;
        public AnimationCurve ZCurve;

        public Vector3CurveReverse(float x, float y, float z)
        {
            XCurve = new AnimationCurve();
            YCurve = new AnimationCurve();
            ZCurve = new AnimationCurve();

            XCurve.AddKey(1f, 0f);
            YCurve.AddKey(1f, 0f);
            ZCurve.AddKey(1f, 0f);

            XCurve.AddKey(0f, x);
            YCurve.AddKey(0f, y);
            ZCurve.AddKey(0f, z);
        }

        public Vector3 Evaluate(float t)
        {
            return new Vector3(
                XCurve.Evaluate(t),
                YCurve.Evaluate(t),
                ZCurve.Evaluate(t)
            );
        }
    }

    //TODO: have multiple dev curves
    public static class CurveDrawer
    {
        public static AnimationCurve RotationCurve1 = new AnimationCurve();

        public static void CurveBuilderDrawer(ConfigEntryBase entry)
        {
            GUILayout.Space(5);

            if (GUILayout.Button("Add Key"))
            {
                AddOrOverrideKeyByIndex();
            }

            GUILayout.Space(5);

            if (GUILayout.Button("Clear Curve"))
            {
                RotationCurve1 = new AnimationCurve();
            }

            GUILayout.Space(5);

            if (GUILayout.Button("Log Curve"))
            {
                LogCurve();
            }
        }

        private static void AddOrOverrideKeyByIndex()
        {
            int index = PluginConfig.KeyIndex.Value;

            var keyFrame = new Keyframe(
                PluginConfig.KeyTime.Value,
                PluginConfig.KeyValue.Value,
                PluginConfig.InTangent.Value,
                PluginConfig.OutTangent.Value
            );

            if (index < 0)
            {
                RotationCurve1.AddKey(keyFrame);
                return;
            }

            if (index < RotationCurve1.length)
            {
                RotationCurve1.MoveKey(index, keyFrame);
            }
            else
            {
                RotationCurve1.AddKey(keyFrame);
            }

            LogCurve();
        }

        static void LogCurve()
        {
            var keys = RotationCurve1.keys;

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("new AnimationCurve(");

            foreach (var k in keys)
            {
                sb.AppendLine(
                    $"    new Keyframe({k.time}f, {k.value}f, {k.inTangent}f, {k.outTangent}f),"
                );
            }

            sb.AppendLine(");");

            Plugin.ModLogger.LogWarning(sb.ToString());
        }
    }
}
