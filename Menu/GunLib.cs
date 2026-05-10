using BepInEx;

using System;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

namespace Nothing.Menu
{
    public class GunTemplate : MonoBehaviour
    {
        public static GameObject spherepointer;
        public static VRRig lockedPlayer;
        public static RaycastHit nray;
        public static float PointerScale = 0.1f;
        private static Material lineMaterial;

        public static void StartVrGun(Action action, bool LockOn)
        {
            if (ControllerInputPoller.instance.rightGrab)
            {
                Physics.Raycast(GorillaTagger.Instance.rightHandTransform.position, -GorillaTagger.Instance.rightHandTransform.up, out nray, float.MaxValue);

                if (spherepointer == null)
                {
                    spherepointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    spherepointer.transform.localScale = new Vector3(PointerScale, PointerScale, PointerScale);
                    spherepointer.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");
                    Destroy(spherepointer.GetComponent<SphereCollider>());
                }

                Color currentColor = Color.red;

                if (ControllerInputPoller.instance.rightControllerIndexFloat > 0.5f)
                {
                    if (LockOn && lockedPlayer == null && nray.collider != null)
                    {
                        VRRig detectedRig = nray.collider.GetComponentInParent<VRRig>();
                        if (detectedRig != null && detectedRig != GorillaTagger.Instance.offlineVRRig)
                        {
                            lockedPlayer = detectedRig;
                        }
                    }

                    if (lockedPlayer != null)
                    {
                        spherepointer.transform.position = lockedPlayer.transform.position;
                        currentColor = Color.blue;
                    }
                    else
                    {
                        spherepointer.transform.position = nray.point;
                        currentColor = Color.green;
                    }

                    action();
                }
                else
                {
                    lockedPlayer = null;
                    spherepointer.transform.position = nray.point;
                }

                if (spherepointer != null)
                {
                    spherepointer.GetComponent<Renderer>().material.color = currentColor;
                    DrawLine(GorillaTagger.Instance.rightHandTransform.position, spherepointer.transform.position, currentColor);
                }
            }
            else
            {
                Cleanup();
            }
        }

        public static void StartPcGun(Action action, bool LockOn)
        {
            if (Mouse.current.rightButton.isPressed)
            {
                Camera mainCam = GorillaTagger.Instance.mainCamera.GetComponent<Camera>();
                Vector2 mousePos = Mouse.current.position.ReadValue();
                Vector3 worldPosition = mainCam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 2.0f));

                if (spherepointer == null)
                {
                    spherepointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    spherepointer.transform.localScale = new Vector3(PointerScale, PointerScale, PointerScale);
                    spherepointer.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");
                    UnityEngine.Object.Destroy(spherepointer.GetComponent<SphereCollider>());
                }

                spherepointer.transform.position = worldPosition;
                Color currentColor = Color.red;

                if (Mouse.current.leftButton.isPressed)
                {
                    Ray ray = mainCam.ScreenPointToRay(mousePos);
                    if (Physics.Raycast(ray, out nray, float.PositiveInfinity))
                    {
                        if (LockOn && lockedPlayer == null)
                        {
                            VRRig detectedRig = nray.collider.GetComponentInParent<VRRig>();
                            if (detectedRig != null && detectedRig != GorillaTagger.Instance.offlineVRRig)
                            {
                                lockedPlayer = detectedRig;
                            }
                        }
                    }

                    if (lockedPlayer != null)
                    {
                        spherepointer.transform.position = lockedPlayer.transform.position;
                        currentColor = Color.blue;
                    }
                    else
                    {
                        currentColor = Color.green;
                    }

                    action();
                }
                else
                {
                    lockedPlayer = null;
                }

                spherepointer.GetComponent<Renderer>().material.color = currentColor;
                DrawLine(mainCam.transform.position, spherepointer.transform.position, currentColor);
            }
            else
            {
                Cleanup();
            }
        }

        private static void Cleanup()
        {
            if (spherepointer != null)
            {
                Destroy(spherepointer);
                spherepointer = null;
            }
            lockedPlayer = null;
        }

        private static void DrawLine(Vector3 start, Vector3 end, Color color)
        {
            GameObject lineObj = new GameObject("GunLine");
            LineRenderer lr = lineObj.AddComponent<LineRenderer>();
            if (lineMaterial == null) lineMaterial = new Material(Shader.Find("GUI/Text Shader"));
            lr.material = lineMaterial;
            lr.startColor = color;
            lr.endColor = color;
            lr.startWidth = 0.015f;
            lr.endWidth = 0.015f;
            lr.positionCount = 2;
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
            Destroy(lineObj, Time.deltaTime);
        }

        public static void StartBothGuns(Action action, bool locko)
        {
            if (XRSettings.isDeviceActive) StartVrGun(action, locko);
            else StartPcGun(action, locko);
        }

        public static void GunTest()
        {
            StartBothGuns(() => { }, true);
        }
    }
}
