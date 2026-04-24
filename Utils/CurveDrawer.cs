using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RealismCommonLib.Utils
{
    public enum CurveAxis
    {
        X,
        Y,
        Z
    }

    public class Vector3Curve
    {
        public AnimationCurve XCurve;
        public AnimationCurve YCurve;
        public AnimationCurve ZCurve;

        public Vector3Curve()
        {
            XCurve = new AnimationCurve();
            YCurve = new AnimationCurve();
            ZCurve = new AnimationCurve();
        }

        public Vector3Curve(AnimationCurve x, AnimationCurve y, AnimationCurve z)
        {
            XCurve = x ?? new AnimationCurve();
            YCurve = y ?? new AnimationCurve();
            ZCurve = z ?? new AnimationCurve();
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

    //TODO: remove, stances will use a single curve for in and out
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

    public static class CurveDrawer
    {
        public static CurveAxis CurrentAxis = CurveAxis.X;

        private static Dictionary<string, Vector3Curve> Curves = new();
        private static string CurrentCurveName = "Curve_0";

        private static Vector2 ScrollPos;
        private static Rect WindowRect = new Rect(50, 50, 400, 650);
        private const int WindowId = 123456;

        public static bool ShowUI = false;

        private static string KeyTimeStr = "0";
        private static string KeyValueStr = "0";
        private static string InTangentStr = "0";
        private static string OutTangentStr = "0";
        private static string KeyIndexStr = "-1";

        private static float KeyTime = 0f;
        private static float KeyValue = 0f;
        private static float InTangent = 0f;
        private static float OutTangent = 0f;
        private static int KeyIndex = -1;

        static CurveDrawer()
        {
            Curves[CurrentCurveName] = new Vector3Curve();
        }

        private static Vector3Curve CurrentCurve
        {
            get
            {
                if (!Curves.ContainsKey(CurrentCurveName))
                    Curves[CurrentCurveName] = new Vector3Curve();

                return Curves[CurrentCurveName];
            }
        }

        public static Vector3Curve? GetCurve(string name)
        {
            if (Curves.TryGetValue(name, out var curve))
                return curve;

            return null;
        }

        // -------------------------
        // INPUT (call from Update)
        // -------------------------
        public static void HandleInput()
        {
            if (PluginConfig.ToggleCurveEditorKey.Value.IsDown())
            {
                ShowUI = !ShowUI;
            }
        }

        // -------------------------
        // GUI (call from OnGUI)
        // -------------------------
        public static void DrawGUI()
        {
            if (!ShowUI) return;

            WindowRect = GUI.Window(WindowId, WindowRect, DrawWindow, "Curve Editor");
        }

        private static void DrawWindow(int id)
        {
            ScrollPos = GUILayout.BeginScrollView(ScrollPos, GUILayout.Width(380), GUILayout.Height(600));

            DrawContents();

            GUILayout.EndScrollView();

            GUI.DragWindow();
        }

        // -------------------------
        // MAIN UI CONTENT
        // -------------------------
        private static void DrawContents()
        {
            GUILayout.Space(5);
            GUILayout.Label("=== Curve Editor ===");

            GUILayout.Label($"Current: {CurrentCurveName}");

            // -------------------------
            // Curve Selection
            // -------------------------
            GUILayout.Space(10);
            GUILayout.Label("Curves:");

            var curveNames = Curves.Keys.ToArray();
            int currentIndex = Array.IndexOf(curveNames, CurrentCurveName);

            int newIndex = GUILayout.SelectionGrid(currentIndex, curveNames, 1);

            if (newIndex >= 0 && newIndex < curveNames.Length)
            {
                CurrentCurveName = curveNames[newIndex];
            }

            GUILayout.Space(5);

            if (GUILayout.Button("New Curve"))
            {
                string newName = $"Curve_{Curves.Count}";
                Curves[newName] = new Vector3Curve();
                CurrentCurveName = newName;
            }

            if (GUILayout.Button("Delete Curve") && Curves.Count > 1)
            {
                Curves.Remove(CurrentCurveName);
                CurrentCurveName = Curves.Keys.First();
            }

            // -------------------------
            // Axis Selection
            // -------------------------
            GUILayout.Space(10);
            GUILayout.Label("Axis:");

            CurrentAxis = (CurveAxis)GUILayout.SelectionGrid(
                (int)CurrentAxis,
                new[] { "X", "Y", "Z" },
                1
            );

            // -------------------------
            // Key Controls
            // -------------------------
            GUILayout.Space(10);
            GUILayout.Label("Key Controls:");

            if (GUILayout.Button("Add / Override Key"))
            {
                AddOrOverrideKeyByIndex();
            }

            if (GUILayout.Button("Clear Current Axis"))
            {
                GetActiveCurve().keys = Array.Empty<Keyframe>();
            }

            if (GUILayout.Button("Clear Entire Curve"))
            {
                Curves[CurrentCurveName] = new Vector3Curve();
            }

            GUILayout.Space(10);
            GUILayout.Label("Keyframe Editor");

            // TIME
            GUILayout.Label("Time");
            KeyTimeStr = GUILayout.TextField(KeyTimeStr);
            KeyTime = ParseFloat(KeyTimeStr, KeyTime);

            // VALUE
            GUILayout.Label("Value");
            KeyValueStr = GUILayout.TextField(KeyValueStr);
            KeyValue = ParseFloat(KeyValueStr, KeyValue);

            // IN TANGENT
            GUILayout.Label("In Tangent");
            InTangentStr = GUILayout.TextField(InTangentStr);
            InTangent = ParseFloat(InTangentStr, InTangent);

            // OUT TANGENT
            GUILayout.Label("Out Tangent");
            OutTangentStr = GUILayout.TextField(OutTangentStr);
            OutTangent = ParseFloat(OutTangentStr, OutTangent);

            // INDEX
            GUILayout.Label("Key Index (-1 = add)");
            KeyIndexStr = GUILayout.TextField(KeyIndexStr);
            KeyIndex = (int)ParseFloat(KeyIndexStr, KeyIndex);

            // -------------------------
            // Logging
            // -------------------------
            GUILayout.Space(10);
            GUILayout.Label("Logging:");

            if (GUILayout.Button("Log Current Curve"))
            {
                LogCurve(CurrentCurveName, CurrentCurve);
            }

            if (GUILayout.Button("Log ALL Curves"))
            {
                LogAllCurves();
            }
        }

        private static float DrawFloatField(float value)
        {
            string text = GUILayout.TextField(value.ToString("0.###"));

            if (float.TryParse(text, out float result))
                return Mathf.Clamp(result, -1000f, 1000f);

            return value;
        }

        private static float ParseFloat(string s, float fallback)
        {
            if (float.TryParse(s, out float result))
                return result;

            return fallback;
        }

        // -------------------------
        // CURVE LOGIC
        // -------------------------
        private static AnimationCurve GetActiveCurve()
        {
            return CurrentAxis switch
            {
                CurveAxis.X => CurrentCurve.XCurve,
                CurveAxis.Y => CurrentCurve.YCurve,
                CurveAxis.Z => CurrentCurve.ZCurve,
                _ => CurrentCurve.XCurve
            };
        }

        private static void AddOrOverrideKeyByIndex()
        {
            var curve = GetActiveCurve();

            int index = KeyIndex;

            var keyFrame = new Keyframe(
                KeyTime,
                KeyValue,
                InTangent,
                OutTangent
            );

            if (index < 0)
            {
                curve.AddKey(keyFrame);
            }
            else if (index < curve.length)
            {
                curve.MoveKey(index, keyFrame);
            }
            else
            {
                curve.AddKey(keyFrame);
            }

            curve.keys = curve.keys.OrderBy(k => k.time).ToArray();

            LogCurve(CurrentCurveName, CurrentCurve);
        }

        // -------------------------
        // LOGGING
        // -------------------------
        private static void LogAllCurves()
        {
            StringBuilder sb = new StringBuilder();

            foreach (var kvp in Curves)
            {
                sb.AppendLine($"// Curve: {kvp.Key}");
                sb.AppendLine(FormatVector3Curve(kvp.Value));
                sb.AppendLine();
            }

            Plugin.ModLogger.LogWarning(sb.ToString());
        }

        private static void LogCurve(string name, Vector3Curve curve)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"// Curve: {name}");
            sb.AppendLine(FormatVector3Curve(curve));

            Plugin.ModLogger.LogWarning(sb.ToString());
        }

        private static string FormatVector3Curve(Vector3Curve curve)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("new Vector3Curve(");

            sb.AppendLine(FormatCurve("    ", curve.XCurve) + ",");
            sb.AppendLine(FormatCurve("    ", curve.YCurve) + ",");
            sb.AppendLine(FormatCurve("    ", curve.ZCurve));

            sb.AppendLine(");");

            return sb.ToString();
        }

        private static string FormatCurve(string indent, AnimationCurve curve)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"{indent}new AnimationCurve(");

            foreach (var k in curve.keys)
            {
                sb.AppendLine(
                    $"{indent}    new Keyframe({k.time}f, {k.value}f, {k.inTangent}f, {k.outTangent}f),"
                );
            }

            sb.Append($"{indent})");

            return sb.ToString();
        }
    }
}
