using System;
using System.Collections.Generic;

using GorillaNetworking;

using HarmonyLib;

using TMPro;

using UnityEngine;

using Custom.Inputs;

namespace NothingMenu.Mods
{
    internal class Visuals
    {
        private class CachedLine
        {
            public LineRenderer Renderer;
            public int LastFrame;
        }

        private static readonly Dictionary<string, CachedLine> _lineCache = new Dictionary<string, CachedLine>();
        private static Shader _lineShader;
        private static Material _lineMaterial;
        private static float _nextLineCleanup;
        private static int _boneLineIndex;

        private static Material LineMaterial
        {
            get
            {
                if (_lineMaterial == null)
                {
                    _lineShader ??= Shader.Find("GUI/Text Shader");
                    _lineMaterial = new Material(_lineShader);
                }

                return _lineMaterial;
            }
        }

        private static LineRenderer GetLine(string key, int positionCount, float width, Color color)
        {
            if (!_lineCache.TryGetValue(key, out CachedLine cached) || cached.Renderer == null)
            {
                GameObject obj = new GameObject(key);
                LineRenderer renderer = obj.AddComponent<LineRenderer>();
                renderer.useWorldSpace = true;
                renderer.material = LineMaterial;
                cached = new CachedLine { Renderer = renderer };
                _lineCache[key] = cached;
            }

            LineRenderer line = cached.Renderer;
            cached.LastFrame = Time.frameCount;
            if (!line.gameObject.activeSelf) line.gameObject.SetActive(true);
            line.positionCount = positionCount;
            line.startWidth = width;
            line.endWidth = width;
            line.startColor = color;
            line.endColor = color;
            return line;
        }

        private static void CleanupOldLines()
        {
            if (Time.unscaledTime < _nextLineCleanup) return;
            _nextLineCleanup = Time.unscaledTime + 0.5f;

            List<string> stale = null;
            foreach (var pair in _lineCache)
            {
                if (Time.frameCount - pair.Value.LastFrame <= 8) continue;
                if (pair.Value.Renderer != null)
                    UnityEngine.Object.Destroy(pair.Value.Renderer.gameObject);
                (stale ??= new List<string>()).Add(pair.Key);
            }

            if (stale == null) return;
            foreach (string key in stale)
                _lineCache.Remove(key);
        }

        public static void Tracer()
        {
            CleanupOldLines();
            IReadOnlyList<VRRig> activeRigs = VRRigCache.ActiveRigs;
            bool flag = activeRigs == null;
            if (!flag)
            {
                foreach (VRRig vrrig in activeRigs)
                {
                    bool flag2 = vrrig != GorillaTagger.Instance.offlineVRRig;
                    bool flag3 = flag2;
                    if (flag3)
                    {
                        Color playerColor = vrrig.playerColor;
                        LineRenderer lineRenderer = GetLine("Tracer_" + vrrig.GetInstanceID(), 2, 0.01f, playerColor);
                        lineRenderer.SetPosition(0, GorillaTagger.Instance.rightHandTransform.position);
                        lineRenderer.SetPosition(1, vrrig.transform.position);
                    }
                }
            }
        }

        public static void BoxESP()
        {
            CleanupOldLines();
            IReadOnlyList<VRRig> activeRigs = VRRigCache.ActiveRigs;
            bool flag = activeRigs == null;
            if (!flag)
            {
                foreach (VRRig vrrig in activeRigs)
                {
                    bool flag2 = vrrig != GorillaTagger.Instance.offlineVRRig;
                    if (flag2)
                    {
                        Color playerColor = vrrig.playerColor;
                        LineRenderer lineRenderer = GetLine("BoxESP_" + vrrig.GetInstanceID(), 16, 0.015f, playerColor);
                        float num = 0.4f;
                        float num2 = 0.65f;
                        float num3 = 0.4f;
                        Vector3 position = vrrig.transform.position;
                        Vector3 vector = position + new Vector3(-num, -num2, -num3);
                        Vector3 vector2 = position + new Vector3(num, -num2, -num3);
                        Vector3 vector3 = position + new Vector3(num, -num2, num3);
                        Vector3 vector4 = position + new Vector3(-num, -num2, num3);
                        Vector3 vector5 = position + new Vector3(-num, num2, -num3);
                        Vector3 vector6 = position + new Vector3(num, num2, -num3);
                        Vector3 vector7 = position + new Vector3(num, num2, num3);
                        Vector3 vector8 = position + new Vector3(-num, num2, num3);
                        lineRenderer.positionCount = 16;
                        lineRenderer.SetPositions(new Vector3[]
                        {
                            vector,
                            vector2,
                            vector3,
                            vector4,
                            vector,
                            vector5,
                            vector6,
                            vector7,
                            vector8,
                            vector5,
                            vector6,
                            vector2,
                            vector3,
                            vector7,
                            vector8,
                            vector4
                        });
                    }
                }
            }
        }

        public static void Box2DESP()
        {
            CleanupOldLines();
            IReadOnlyList<VRRig> activeRigs = VRRigCache.ActiveRigs;
            bool flag = activeRigs == null;
            if (!flag)
            {
                foreach (VRRig vrrig in activeRigs)
                {
                    bool flag2 = vrrig != GorillaTagger.Instance.offlineVRRig;
                    if (flag2)
                    {
                        Color playerColor = vrrig.playerColor;
                        LineRenderer lineRenderer = GetLine("Box2DESP_" + vrrig.GetInstanceID(), 5, 0.015f, playerColor);
                        float num = 0.35f;
                        float num2 = 0.5f;
                        Vector3 position = vrrig.transform.position;
                        Vector3 vector = GorillaTagger.Instance.mainCamera.transform.position - position;
                        vector.y = 0f;
                        Quaternion quaternion = Quaternion.LookRotation(vector);
                        Vector3 vector2 = quaternion * Vector3.right * num;
                        Vector3 vector3 = Vector3.up * num2;
                        Vector3 vector4 = position + vector3 - vector2;
                        Vector3 vector5 = position + vector3 + vector2;
                        Vector3 vector6 = position - vector3 - vector2;
                        Vector3 vector7 = position - vector3 + vector2;
                        lineRenderer.positionCount = 5;
                        lineRenderer.SetPositions(new Vector3[]
                        {
                            vector4,
                            vector5,
                            vector7,
                            vector6,
                            vector4
                        });
                    }
                }
            }
        }

        public static void BoneESP()
        {
            CleanupOldLines();
            _boneLineIndex = 0;
            IReadOnlyList<VRRig> activeRigs = VRRigCache.ActiveRigs;
            bool flag = activeRigs == null;
            if (!flag)
            {
                foreach (VRRig vrrig in activeRigs)
                {
                    bool flag2 = vrrig != GorillaTagger.Instance.offlineVRRig && vrrig.mainSkin != null;
                    if (flag2)
                    {
                        Color playerColor = vrrig.playerColor;
                        Vector3 position = vrrig.head.rigTarget.position;
                        Vector3 vector = position - new Vector3(0f, 0.08f, 0f);
                        Vector3 end = position - new Vector3(0f, 0.45f, 0f);
                        Vector3 position2 = vrrig.leftHand.rigTarget.position;
                        Vector3 position3 = vrrig.rightHand.rigTarget.position;
                        Vector3 vector2 = vector + vrrig.mainSkin.transform.right * -0.1f;
                        Vector3 vector3 = vector + vrrig.mainSkin.transform.right * 0.1f;
                        Vector3 vector4 = Vector3.Lerp(vector2, position2, 0.5f) + vrrig.mainSkin.transform.right * -0.15f - vrrig.mainSkin.transform.forward * 0.05f;
                        Vector3 vector5 = Vector3.Lerp(vector3, position3, 0.5f) + vrrig.mainSkin.transform.right * 0.15f - vrrig.mainSkin.transform.forward * 0.05f;
                        Visuals.DrawBoneLine(position, vector, playerColor);
                        Visuals.DrawBoneLine(vector, end, playerColor);
                        Visuals.DrawBoneLine(vector, vector2, playerColor);
                        Visuals.DrawBoneLine(vector2, vector4, playerColor);
                        Visuals.DrawBoneLine(vector4, position2, playerColor);
                        Visuals.DrawBoneLine(vector, vector3, playerColor);
                        Visuals.DrawBoneLine(vector3, vector5, playerColor);
                        Visuals.DrawBoneLine(vector5, position3, playerColor);
                    }
                }
            }
        }

        private static void DrawBoneLine(Vector3 start, Vector3 end, Color color)
        {
            LineRenderer lineRenderer = GetLine("BoneLine_" + _boneLineIndex++, 2, 0.02f, color);
            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, end);
        }

        public static void Beacons()
        {
            CleanupOldLines();
            IReadOnlyList<VRRig> activeRigs = VRRigCache.ActiveRigs;
            bool flag = activeRigs == null;
            if (!flag)
            {
                foreach (VRRig vrrig in activeRigs)
                {
                    bool flag2 = vrrig != GorillaTagger.Instance.offlineVRRig;
                    bool flag3 = flag2;
                    if (flag3)
                    {
                        Vector3 position = vrrig.transform.position;
                        Color playerColor = vrrig.playerColor;
                        playerColor.a = 0.5f;
                        LineRenderer lineRenderer = GetLine("Beacon_" + vrrig.GetInstanceID(), 2, 0.07f, playerColor);
                        lineRenderer.SetPositions(new Vector3[]
                        {
                            position + new Vector3(0f, 1000f, 0f),
                            position - new Vector3(0f, 1000f, 0f)
                        });
                    }
                }
            }
        }

        public static void AdvNametags()
        {
            IReadOnlyList<VRRig> activeRigs = VRRigCache.ActiveRigs;
            bool flag = activeRigs == null;
            if (!flag)
            {
                foreach (VRRig vrrig in activeRigs)
                {
                    bool flag2 = vrrig != GorillaTagger.Instance.offlineVRRig && vrrig.mainSkin.enabled;
                    if (flag2)
                    {
                        GameObject gameObject = new GameObject("Text");
                        TextMeshPro textMeshPro = gameObject.AddComponent<TextMeshPro>();
                        textMeshPro.fontSize = 1.2f;
                        textMeshPro.color = Color.white;
                        textMeshPro.alignment = TMPro.TextAlignmentOptions.Center;
                        GameObject gameObject2 = GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/motdBodyText");
                        bool flag3 = gameObject2 != null;
                        if (flag3)
                        {
                            textMeshPro.font = gameObject2.GetComponent<TextMeshPro>().font;
                        }
                        gameObject.transform.position = vrrig.headMesh.transform.position + new Vector3(0f, 0.6f, 0f);
                        gameObject.transform.LookAt(Camera.main.transform);
                        gameObject.transform.Rotate(0f, 180f, 0f);
                        int num = 0;
                        try
                        {
                            num = Traverse.Create(vrrig).Field("fps").GetValue<int>();
                        }
                        catch
                        {
                        }
                        string text = (num < 45) ? "red" : ((num > 80) ? "green" : "orange");
                        NetPlayer creator = vrrig.Creator;
                        string text2 = creator != null ? creator.NickName : "Unknown";
                        string text3 = creator != null ? creator.UserId : "N/A";
                        bool flag4 = creator != null && creator.IsMasterClient;
                        bool flag5 = vrrig.mainSkin.material.name.Contains("lava");
                        textMeshPro.text = string.Format("<b>{0}</b>\nID: {1}\nMaster: {2}\nTagged: {3}\nFPS: <color={4}>{5}</color>", new object[]
                        {
                            text2,
                            text3,
                            flag4 ? "Yes" : "No",
                            flag5 ? "Yes" : "No",
                            text,
                            num
                        });
                        UnityEngine.Object.Destroy(gameObject, Time.deltaTime);
                    }
                }
            }
        }

        public static void FakeUnbanSelf()
        {
            PhotonNetworkController.Instance.UpdateTriggerScreens();
            GorillaScoreboardTotalUpdater.instance.ClearOfflineFailureText();
            GorillaComputer.instance.screenText.DisableFailedState();
            GorillaComputer.instance.functionSelectText.DisableFailedState();
        }


    }
}
