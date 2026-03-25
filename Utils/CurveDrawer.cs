using BepInEx.Configuration;
using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using static GClass2175;

namespace RealismCommonLib.Utils
{
    public static class CurveDrawer
    {
        public static AnimationCurve DevCurve1 = new AnimationCurve();

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
                DevCurve1 = new AnimationCurve();
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
                DevCurve1.AddKey(keyFrame);
                return;
            }

            if (index < DevCurve1.length)
            {
                DevCurve1.MoveKey(index, keyFrame);
            }
            else
            {
                DevCurve1.AddKey(keyFrame);
            }

            LogCurve();
        }

        static void LogCurve()
        {
            var keys = DevCurve1.keys;

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
