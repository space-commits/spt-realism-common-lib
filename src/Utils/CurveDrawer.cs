using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using System.Reflection;

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

        private static Vector2 ScrollPos;
        private static Rect WindowRect = new Rect(50, 50, 400, 650);
        private const int WindowId = 123456;

        public static bool ShowUI = false;

        private static float KeyTime = 0f;
        private static float KeyValue = 0f;
        private static float InTangent = 0f;
        private static float OutTangent = 0f;
        private static int KeyIndex = -1;

        private static string SelectedCurveName = "Curve_0";
        private static CurveAxis SelectedAxis = CurveAxis.X;
        private static int SelectedKeyIndex = -1;

        // string buffers (fix input issues)
        private static string TimeStr = "0";
        private static string ValueStr = "0";
        private static string InTanStr = "0";
        private static string OutTanStr = "0";
        private static string SaveDirectory = PluginConfig.CurveFilePath.Value;

        static CurveDrawer()
        {
            LoadFromFile(SaveDirectory);
        }

        private static Vector3Curve CurrentCurve => Curves[SelectedCurveName];

        private static AnimationCurve GetActiveCurve()
        {
            return SelectedAxis switch
            {
                CurveAxis.X => CurrentCurve.XCurve,
                CurveAxis.Y => CurrentCurve.YCurve,
                CurveAxis.Z => CurrentCurve.ZCurve,
                _ => CurrentCurve.XCurve
            };
        }

        private static void ResetEditorFields()
        {
            TimeStr = "0";
            ValueStr = "0";
            InTanStr = "0";
            OutTanStr = "0";
        }

        private static void LoadKeyToEditor(Keyframe k)
        {
            TimeStr = k.time.ToString();
            ValueStr = k.value.ToString();
            InTanStr = k.inTangent.ToString();
            OutTanStr = k.outTangent.ToString();
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
            GUILayout.Label("=== Curve Editor ===");

            // -------------------------
            // CURVE SELECTION
            // -------------------------
            GUILayout.Space(10);
            GUILayout.Label("Curves:");

            foreach (var kvp in Curves)
            {
                bool isSelected = kvp.Key == SelectedCurveName;

                if (GUILayout.Toggle(isSelected, kvp.Key, "Button"))
                {
                    if (SelectedCurveName != kvp.Key)
                    {
                        SelectedCurveName = kvp.Key;
                        SelectedKeyIndex = -1;
                    }
                }
            }

            GUILayout.Space(5);

            if (GUILayout.Button("New Curve"))
            {
                string newName = $"Curve_{Curves.Count}";
                Curves[newName] = new Vector3Curve();
                SelectedCurveName = newName;
            }

            if (GUILayout.Button("Delete Curve") && Curves.Count > 1)
            {
                Curves.Remove(SelectedCurveName);
                SelectedCurveName = Curves.Keys.First();
                SelectedKeyIndex = -1;
            }

            // -------------------------
            // AXIS SELECTION
            // -------------------------
            GUILayout.Space(10);
            GUILayout.Label("Axis:");

            foreach (CurveAxis axis in Enum.GetValues(typeof(CurveAxis)))
            {
                bool selected = axis == SelectedAxis;

                if (GUILayout.Toggle(selected, axis.ToString(), "Button"))
                {
                    if (SelectedAxis != axis)
                    {
                        SelectedAxis = axis;
                        SelectedKeyIndex = -1;
                    }
                }
            }

            // -------------------------
            // KEYFRAME LIST
            // -------------------------
            GUILayout.Space(10);
            GUILayout.Label("Keyframes:");

            var curve = GetActiveCurve();

            for (int i = 0; i < curve.length; i++)
            {
                var k = curve[i];
                string label = $"[{i}] t:{k.time:0.###} v:{k.value:0.###}";

                bool selected = i == SelectedKeyIndex;

                if (GUILayout.Toggle(selected, label, "Button"))
                {
                    if (SelectedKeyIndex != i)
                    {
                        SelectedKeyIndex = i;
                        LoadKeyToEditor(k);
                    }
                }
            }

            GUILayout.Space(5);

            if (GUILayout.Button("Add New Key"))
            {
                SelectedKeyIndex = -1;
                ResetEditorFields();
            }

            if (GUILayout.Button("Delete Selected Key") && SelectedKeyIndex >= 0)
            {
                DeleteKey(SelectedKeyIndex);
                SelectedKeyIndex = -1;
            }

            // -------------------------
            // KEYFRAME EDITOR
            // -------------------------
            GUILayout.Space(10);
            GUILayout.Label("Editor:");

            GUILayout.Label("Time");
            TimeStr = GUILayout.TextField(TimeStr);

            GUILayout.Label("Value");
            ValueStr = GUILayout.TextField(ValueStr);

            GUILayout.Label("In Tangent");
            InTanStr = GUILayout.TextField(InTanStr);

            GUILayout.Label("Out Tangent");
            OutTanStr = GUILayout.TextField(OutTanStr);

            if (GUILayout.Button("Apply"))
            {
                ApplyKey();
            }

            // -------------------------
            // LOGGING
            // -------------------------
            GUILayout.Space(10);

            if (GUILayout.Button("Log Current Curve"))
            {
                LogCurve(SelectedCurveName, CurrentCurve);
            }

            if (GUILayout.Button("Log ALL Curves"))
            {
                LogAllCurves();
            }

            GUILayout.Space(10);
            GUILayout.Label("Persistence");

            if (GUILayout.Button("Save Curves"))
            {
                SaveToFile(SaveDirectory);
            }

            if (GUILayout.Button("Reload Curves"))
            {
                LoadFromFile(SaveDirectory);
            }
        }

        private static void ApplyKey()
        {
            var curve = GetActiveCurve();

            float t = Parse(TimeStr);
            float v = Parse(ValueStr);
            float inT = Parse(InTanStr);
            float outT = Parse(OutTanStr);

            var key = new Keyframe(t, v, inT, outT);

            if (SelectedKeyIndex >= 0 && SelectedKeyIndex < curve.length)
            {
                curve.MoveKey(SelectedKeyIndex, key);
            }
            else
            {
                curve.AddKey(key);
            }

            curve.keys = curve.keys.OrderBy(k => k.time).ToArray();
        }

        private static void DeleteKey(int index)
        {
            var curve = GetActiveCurve();

            var keys = curve.keys.ToList();
            keys.RemoveAt(index);
            curve.keys = keys.ToArray();
        }

        private static float Parse(string s)
        {
            return float.TryParse(s, out var v) ? v : 0f;
        }

        private static float ParseFloat(string s, float fallback)
        {
            if (float.TryParse(s, out float result))
                return result;

            return fallback;
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

        private static string PluginDirectory
        {
            get
            {
                return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;
            }
        }

        private static string GetFilePath(string directory)
        {
            var saveDir = string.IsNullOrWhiteSpace(directory) ? PluginDirectory : directory;
            return Path.Combine(saveDir, "curves.json");
        }

        public static void SaveToFile(string directory)
        {
            try
            {
                var file = new CurveFile();

                foreach (var kvp in Curves)
                {
                    file.curves[kvp.Key] = new SerializableVector3Curve(kvp.Value);
                }

                var settings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented
                };

                string json = JsonConvert.SerializeObject(file, settings);
                string filePath = GetFilePath(directory);

                Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? string.Empty);

                File.WriteAllText(filePath, json);

                Plugin.ModLogger.LogInfo($"Saved curves to {filePath}");
            }
            catch (Exception e)
            {
                Plugin.ModLogger.LogError($"Save failed: {e}");
            }
        }

        public static void LoadFromFile(string directory)
        {
            try
            {
                string filePath = GetFilePath(directory);

                if (!File.Exists(filePath))
                {
                    Plugin.ModLogger.LogWarning("No curve file found, creating new.");
                    return;
                }

                string json = File.ReadAllText(filePath);

                var file = JsonConvert.DeserializeObject<CurveFile>(json);

                if (file == null || file.curves == null)
                {
                    Plugin.ModLogger.LogWarning("Invalid curve file.");
                    return;
                }

                Curves.Clear();

                foreach (var kvp in file.curves)
                {
                    Curves[kvp.Key] = kvp.Value.ToVector3Curve();
                }

                if (Curves.Count > 0)
                    SelectedCurveName = Curves.Keys.First();

                Plugin.ModLogger.LogInfo($"Loaded curves from {filePath}");
            }
            catch (Exception e)
            {
                Plugin.ModLogger.LogError($"Load failed: {e}");
            }
        }
    }

    [Serializable]
    public class SerializableKeyframe
    {
        public float time;
        public float value;
        public float inTangent;
        public float outTangent;

        public SerializableKeyframe() { }

        public SerializableKeyframe(Keyframe k)
        {
            time = k.time;
            value = k.value;
            inTangent = k.inTangent;
            outTangent = k.outTangent;
        }

        public Keyframe ToKeyframe()
        {
            return new Keyframe(time, value, inTangent, outTangent);
        }
    }

    [Serializable]
    public class SerializableCurve
    {
        public List<SerializableKeyframe> keys = new();

        public SerializableCurve() { }

        public SerializableCurve(AnimationCurve curve)
        {
            foreach (var k in curve.keys)
                keys.Add(new SerializableKeyframe(k));
        }

        public AnimationCurve ToAnimationCurve()
        {
            var curve = new AnimationCurve();

            foreach (var k in keys)
                curve.AddKey(k.ToKeyframe());

            return curve;
        }
    }

    [Serializable]
    public class SerializableVector3Curve
    {
        public SerializableCurve X = new();
        public SerializableCurve Y = new();
        public SerializableCurve Z = new();

        public SerializableVector3Curve() { }

        public SerializableVector3Curve(Vector3Curve curve)
        {
            X = new SerializableCurve(curve.XCurve);
            Y = new SerializableCurve(curve.YCurve);
            Z = new SerializableCurve(curve.ZCurve);
        }

        public Vector3Curve ToVector3Curve()
        {
            return new Vector3Curve(
                X.ToAnimationCurve(),
                Y.ToAnimationCurve(),
                Z.ToAnimationCurve()
            );
        }
    }

    [Serializable]
    public class CurveFile
    {
        public Dictionary<string, SerializableVector3Curve> curves = new();
    }
}
