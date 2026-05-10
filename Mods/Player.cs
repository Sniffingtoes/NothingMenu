using BepInEx;

using ExitGames.Client.Photon;

using GorillaNetworking;

using GorillaTagScripts;
using GorillaTagScripts.VirtualStumpCustomMaps;

using HarmonyLib;

using Nothing.Classes;
using Nothing.Menu;
using Nothing.Notifications;

using Oculus.Platform;

using Photon.Pun;
using Photon.Realtime;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using UnityEngine;

using Custom.Inputs;

using static Nothing.Menu.GunTemplate;

namespace NothingMenu.Mods
{
    internal class Player
    {
        private static GameObject lBall;
        private static GameObject rBall;
        private static bool isGhostToggled = false;
        private static bool wasButtonPressed = false;



        public static void Ghostmonke()
        {
            bool isButtonPressed = Get.aButton;

            if (isButtonPressed && !wasButtonPressed)
            {
                isGhostToggled = !isGhostToggled;

            }
            wasButtonPressed = isButtonPressed;

            if (isGhostToggled)
            {
                GorillaTagger.Instance.offlineVRRig.enabled = false;

                if (lBall == null)
                {
                    lBall = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    lBall.transform.localScale = new Vector3(0.12f, 0.12f, 0.12f);
                    lBall.GetComponent<Renderer>().material.shader = Shader.Find("GorillaTag/UberShader");
                    lBall.GetComponent<Renderer>().material.color = Color.black;
                    UnityEngine.Object.Destroy(lBall.GetComponent<Collider>());
                }

                if (rBall == null)
                {
                    rBall = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    rBall.transform.localScale = new Vector3(0.12f, 0.12f, 0.12f);
                    rBall.GetComponent<Renderer>().material.shader = Shader.Find("GorillaTag/UberShader");
                    rBall.GetComponent<Renderer>().material.color = Color.black;
                    UnityEngine.Object.Destroy(rBall.GetComponent<Collider>());
                }

                lBall.transform.position = GorillaTagger.Instance.leftHandTransform.position;
                rBall.transform.position = GorillaTagger.Instance.rightHandTransform.position;
            }
            else
            {
                GorillaTagger.Instance.offlineVRRig.enabled = true;

                if (lBall != null) { UnityEngine.Object.Destroy(lBall); lBall = null; }
                if (rBall != null) { UnityEngine.Object.Destroy(rBall); rBall = null; }
            }
        }

        private static bool isInvisToggled = false;
        private static bool wasInvisButtonPressed = false;
        private static GameObject leftBall;
        private static GameObject rightBall;

        public static void invis()
        {
            bool isButtonPressed = Get.bButton;

            if (isButtonPressed && !wasInvisButtonPressed)
            {
                isInvisToggled = !isInvisToggled;
            }
            wasInvisButtonPressed = isButtonPressed;

            if (isInvisToggled)
            {
                GorillaTagger.Instance.offlineVRRig.headBodyOffset.x = 180f;

                if (leftBall == null)
                {
                    leftBall = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    rightBall = GameObject.CreatePrimitive(PrimitiveType.Sphere);

                    GameObject.Destroy(leftBall.GetComponent<Rigidbody>());
                    GameObject.Destroy(leftBall.GetComponent<SphereCollider>());
                    GameObject.Destroy(rightBall.GetComponent<Rigidbody>());
                    GameObject.Destroy(rightBall.GetComponent<SphereCollider>());

                    leftBall.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                    rightBall.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

                    Color myColor = GorillaTagger.Instance.offlineVRRig.playerColor;
                    leftBall.GetComponent<Renderer>().material.color = Color.black;
                    rightBall.GetComponent<Renderer>().material.color = Color.black;

                    leftBall.GetComponent<Renderer>().material.shader = Shader.Find("GorillaTag/UberShader");
                    rightBall.GetComponent<Renderer>().material.shader = Shader.Find("GorillaTag/UberShader");
                }

                leftBall.transform.position = GorillaTagger.Instance.leftHandTransform.position;
                rightBall.transform.position = GorillaTagger.Instance.rightHandTransform.position;
            }
            else
            {
                GorillaTagger.Instance.offlineVRRig.headBodyOffset.x = 0f;

                if (leftBall != null)
                {
                    GameObject.Destroy(leftBall);
                    GameObject.Destroy(rightBall);
                    leftBall = null;
                    rightBall = null;
                }
            }
        }

        public static void NoClip()
        {
            bool disablecolliders2 = Get.rIndexFloat;
            MeshCollider[] colliders = Resources.FindObjectsOfTypeAll<MeshCollider>();

            foreach (MeshCollider collider in colliders)
            {
                collider.enabled = !disablecolliders2;
            }
        }

        public static void WallWalk()
        {
            if (Get.rightGrab)
            {
                GorillaLocomotion.GTPlayer.Instance.bodyCollider.attachedRigidbody.AddForce(GorillaLocomotion.GTPlayer.Instance.bodyCollider.transform.right * (Time.deltaTime * (5.6f / Time.deltaTime)), ForceMode.Acceleration);
            }

            if (Get.leftGrab)
            {
                GorillaLocomotion.GTPlayer.Instance.bodyCollider.attachedRigidbody.AddForce(GorillaLocomotion.GTPlayer.Instance.bodyCollider.transform.right * (Time.deltaTime * (8.8f / Time.deltaTime)), ForceMode.Acceleration);
            }

            if (Get.rightGrab && Get.leftGrab)
            {
                GorillaLocomotion.GTPlayer.Instance.bodyCollider.attachedRigidbody.AddForce(GorillaLocomotion.GTPlayer.Instance.bodyCollider.transform.forward * (Time.deltaTime * (5.6f / Time.deltaTime)), ForceMode.Acceleration);
            }
        }

        public static void FixHead()
        {
            VRRig.LocalRig.head.trackingRotationOffset.x = 0f;
            VRRig.LocalRig.head.trackingRotationOffset.y = 0f;
            VRRig.LocalRig.head.trackingRotationOffset.z = 0f;
        }

        public static bool isUpsideDown = false;
        public static bool isBrokenNeck = false;
        public static bool isBackwards = false;
        public static bool isSideways = false;

        public static void UpsideDownHead()
        {
            isUpsideDown = !isUpsideDown;
            if (isUpsideDown)
            {
                VRRig.LocalRig.head.trackingRotationOffset = new Vector3(0f, 0f, 180f);
            }
            else
            {
                FixHead();
            }
        }

        public static void BrokenNeck()
        {
            isBrokenNeck = !isBrokenNeck;
            if (isBrokenNeck)
            {
                VRRig.LocalRig.head.trackingRotationOffset = new Vector3(0f, 0f, 90f);
            }
            else
            {
                FixHead();
            }
        }

        public static void BackwardsHead()
        {
            isBackwards = !isBackwards;
            if (isBackwards)
            {
                VRRig.LocalRig.head.trackingRotationOffset = new Vector3(0f, 180f, 0f);
            }
            else
            {
                FixHead();
            }
        }

        public static void SidewaysHead()
        {
            isSideways = !isSideways;
            if (isSideways)
            {
                VRRig.LocalRig.head.trackingRotationOffset = new Vector3(0f, 90f, 0f);
            }
            else
            {
                FixHead();
            }
        }

        public static void GrabRig()
        {
            bool rightGrab = Get.rightGrab;
            if (rightGrab)
            {
                GorillaTagger.Instance.offlineVRRig.enabled = false;
                GorillaTagger.Instance.offlineVRRig.transform.position = GorillaTagger.Instance.rightHandTransform.position;
            }
            else
            {
                GorillaTagger.Instance.offlineVRRig.enabled = true;
            }
        }

        private static float times = 0f;
        private static bool lag = false;
        private static readonly float delay = 0.37f;

        public static void FakeLag()
        {
            bool flag = GorillaTagger.Instance == null;
            if (!flag)
            {
                bool flag2 = Get.rightGrab;
                if (flag2)
                {
                    bool flag3 = Time.time - Player.times >= Player.delay;
                    if (flag3)
                    {
                        Player.lag = !Player.lag;
                        GorillaTagger.Instance.offlineVRRig.enabled = !Player.lag;
                        Player.times = Time.time;
                    }
                }
                else
                {
                    GorillaTagger.Instance.offlineVRRig.enabled = true;
                    Player.lag = false;
                }
            }
        }

        public static void Tpose()
        {
            bool flag = GorillaTagger.Instance == null;
            if (!flag)
            {
                bool flag2 = Get.rightGrab;
                if (flag2)
                {
                    GorillaTagger.Instance.offlineVRRig.enabled = false;
                    GorillaTagger.Instance.offlineVRRig.head.rigTarget.transform.rotation = GorillaTagger.Instance.bodyCollider.transform.rotation;
                    GorillaTagger.Instance.offlineVRRig.rightHand.rigTarget.transform.rotation = GorillaTagger.Instance.bodyCollider.transform.rotation;
                    GorillaTagger.Instance.offlineVRRig.leftHand.rigTarget.transform.rotation = GorillaTagger.Instance.bodyCollider.transform.rotation;
                    GorillaTagger.Instance.offlineVRRig.leftHand.rigTarget.transform.position = GorillaTagger.Instance.offlineVRRig.transform.position + GorillaTagger.Instance.offlineVRRig.transform.right * 1.5f;
                    GorillaTagger.Instance.offlineVRRig.rightHand.rigTarget.transform.position = GorillaTagger.Instance.offlineVRRig.transform.position + GorillaTagger.Instance.offlineVRRig.transform.right * -1.5f;
                }
                else
                {
                    GorillaTagger.Instance.offlineVRRig.enabled = true;
                }
            }
        }

        public static void Spin()
        {
            bool flag = GorillaTagger.Instance == null;
            if (!flag)
            {
                bool flag2 = Get.rightGrab;
                if (flag2)
                {
                    GorillaTagger.Instance.offlineVRRig.enabled = false;
                    GorillaTagger.Instance.offlineVRRig.transform.rotation = Quaternion.Euler(GorillaTagger.Instance.offlineVRRig.transform.rotation.eulerAngles + new Vector3(0f, 500f * Time.deltaTime, 0f));
                }
                else
                {
                    GorillaTagger.Instance.offlineVRRig.enabled = true;
                }
            }
        }

        public static void SpazHands()
        {
            if (GorillaTagger.Instance != null && GorillaTagger.Instance.offlineVRRig != null)
            {
                Vector3 randomRotation = new Vector3(
                    UnityEngine.Random.Range(0f, 360f),
                    UnityEngine.Random.Range(0f, 360f),
                    UnityEngine.Random.Range(0f, 360f)
                );

                GorillaTagger.Instance.offlineVRRig.leftHand.rigTarget.eulerAngles = randomRotation;

                GorillaTagger.Instance.offlineVRRig.rightHand.rigTarget.eulerAngles = new Vector3(
                    UnityEngine.Random.Range(0f, 360f),
                    UnityEngine.Random.Range(0f, 360f),
                    UnityEngine.Random.Range(0f, 360f)
                );
            }
        }

        public static void Spaz()
        {
            if (GorillaTagger.Instance != null && GorillaTagger.Instance.offlineVRRig != null)
            {
                GorillaTagger.Instance.offlineVRRig.head.rigTarget.eulerAngles = new Vector3(
                    UnityEngine.Random.Range(0f, 360f),
                    UnityEngine.Random.Range(0f, 360f),
                    UnityEngine.Random.Range(0f, 360f)
                );

                GorillaTagger.Instance.offlineVRRig.leftHand.rigTarget.eulerAngles = new Vector3(
                    UnityEngine.Random.Range(0f, 360f),
                    UnityEngine.Random.Range(0f, 360f),
                    UnityEngine.Random.Range(0f, 360f)
                );

                GorillaTagger.Instance.offlineVRRig.rightHand.rigTarget.eulerAngles = new Vector3(
                    UnityEngine.Random.Range(0f, 360f),
                    UnityEngine.Random.Range(0f, 360f),
                    UnityEngine.Random.Range(0f, 360f)
                );
            }
        }

        public static void Ascend()
        {
            bool flag = GorillaTagger.Instance == null;
            if (!flag)
            {
                bool flag2 = Get.rightGrab;
                if (flag2)
                {
                    GorillaTagger.Instance.offlineVRRig.enabled = false;
                    GorillaTagger.Instance.offlineVRRig.transform.position += new Vector3(0f, 5f * Time.deltaTime, 0f);
                }
                else
                {
                    GorillaTagger.Instance.offlineVRRig.enabled = true;
                }
            }
        }

        public static void Helicopter()
        {
            bool flag = GorillaTagger.Instance == null;
            if (!flag)
            {
                bool flag2 = Get.rightGrab;
                if (flag2)
                {
                    GorillaTagger.Instance.offlineVRRig.enabled = false;
                    Player.Ascend();
                    Player.Spin();
                    Player.Tpose();
                }
                else
                {
                    GorillaTagger.Instance.offlineVRRig.enabled = true;
                }
            }
        }

        private static bool isFrozen = false;
        private static bool wasGripPressed = false;

        public static void FreezeRig()
        {
            if (GorillaTagger.Instance == null || GorillaTagger.Instance.offlineVRRig == null) return;

            bool isGripPressed = Get.rightGrab;

            if (isGripPressed && !wasGripPressed)
            {
                isFrozen = !isFrozen;
            }
            wasGripPressed = isGripPressed;

            if (isFrozen)
            {
                GorillaTagger.Instance.offlineVRRig.enabled = false;
                GorillaTagger.Instance.offlineVRRig.transform.position = GorillaTagger.Instance.headCollider.transform.position;
                GorillaTagger.Instance.offlineVRRig.transform.rotation = GorillaTagger.Instance.headCollider.transform.rotation;
            }
            else
            {
                GorillaTagger.Instance.offlineVRRig.enabled = true;
            }
        }

    }
}

