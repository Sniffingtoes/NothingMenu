using BepInEx;

using Custom.Inputs;

using ExitGames.Client.Photon;

using g3;

using GorillaLocomotion;

using GorillaNetworking;

using Nothing.Classes;
using Nothing.Menu;
using Nothing.Mods;
using Nothing.Notifications;
using Nothing.Patches;

using NothingMenu.Mods;

using Photon.Pun;
using Photon.Realtime;
using Photon.Voice;

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

using UnityEngine;
using UnityEngine.XR;

using static Nothing.Menu.Buttons;
using static Nothing.Menu.GunTemplate;
using static NothingMenu.Mods.Player;

namespace NothingMenu.Mods
{
    internal class Fun
    {
        public static void Frozone()
        {
            if (ControllerInputPoller.instance.rightGrab)
            {
                GameObject FrozoneCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                FrozoneCube.AddComponent<GorillaSurfaceOverride>().overrideIndex = 61;
                FrozoneCube.transform.localScale = new Vector3(0.025f, 0.3f, 0.4f);

                FrozoneCube.transform.position = GorillaTagger.Instance.rightHandTransform.position - new Vector3(0, .05f, 0);
                FrozoneCube.transform.rotation = GorillaTagger.Instance.rightHandTransform.rotation;

                FrozoneCube.GetComponent<Renderer>().material.color = Color.blue;
                GameObject.Destroy(FrozoneCube, 1f);
            }

            if (ControllerInputPoller.instance.leftGrab)
            {
                GameObject FrozoneCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                FrozoneCube.AddComponent<GorillaSurfaceOverride>().overrideIndex = 61;
                FrozoneCube.transform.localScale = new Vector3(0.025f, 0.3f, 0.4f);

                FrozoneCube.transform.position = GorillaTagger.Instance.leftHandTransform.position - new Vector3(0, .05f, 0);
                FrozoneCube.transform.rotation = GorillaTagger.Instance.leftHandTransform.rotation;

                FrozoneCube.GetComponent<Renderer>().material.color = Color.blue;
                GameObject.Destroy(FrozoneCube, 1f);
            }
        }


        private static bool canDash = true;
        private static bool wasGrabbed = false;
        private static float dashTimer = 0f;

        public static void Dash()
        {
            bool isGrabbing = ControllerInputPoller.instance.rightGrab;

            if (!canDash)
            {
                dashTimer += Time.deltaTime;
                if (dashTimer >= 1f)
                {
                    canDash = true;
                    dashTimer = 0f;
                }
            }

            if (isGrabbing && !wasGrabbed && canDash)
            {

                Vector3 dashVelocity = GTPlayer.Instance.headCollider.transform.forward * 10f;

                GorillaTagger.Instance.rigidbody.AddForce(dashVelocity, ForceMode.VelocityChange);

                canDash = false;
            }

            wasGrabbed = isGrabbing;
        }

        public static void Scitzo()
        {
            bool flag = GorillaParent.instance == null || Camera.main == null;
            bool flag2 = !flag;
            if (flag2)
            {
                Vector3 position = Camera.main.transform.position;
                IReadOnlyList<VRRig> activeRigs = VRRigCache.ActiveRigs;
                bool flag3 = activeRigs == null;
                if (!flag3)
                {
                    foreach (VRRig vrrig in activeRigs)
                    {
                        bool flag4 = vrrig == null || vrrig.isOfflineVRRig;
                        bool flag5 = !flag4;
                        if (flag5)
                        {
                            try
                            {
                                bool flag6 = vrrig.head != null && vrrig.head.rigTarget != null;
                                bool flag7 = flag6;
                                if (flag7)
                                {
                                    vrrig.head.rigTarget.LookAt(position);
                                    vrrig.head.rigTarget.rotation *= Quaternion.Euler(0f, 0f, 0f);
                                }
                            }
                            catch
                            {
                            }
                        }
                    }
                }
            }
        }

        private static Camera mainCam;
        private static Transform camTransform;
        private const float MaxDistanceSqr = 70f;

        public static void FuckYou()
        {
            IReadOnlyList<VRRig> activeRigs = VRRigCache.ActiveRigs;
            bool flag = activeRigs == null;
            if (!flag)
            {
                bool flag2 = Fun.mainCam == null;
                if (flag2)
                {
                    Fun.mainCam = Camera.main;
                    bool flag3 = Fun.mainCam != null;
                    if (flag3)
                    {
                        Fun.camTransform = Fun.mainCam.transform;
                    }
                }
                else
                {
                    Vector3 position = Fun.camTransform.position;
                    for (int i = 0; i < activeRigs.Count; i++)
                    {
                        VRRig vrrig = activeRigs[i];
                        bool flag4;
                        if (vrrig != null && !vrrig.isOfflineVRRig)
                        {
                            VRMap head = vrrig.head;
                            flag4 = (((head != null) ? head.rigTarget : null) != null);
                        }
                        else
                        {
                            flag4 = false;
                        }
                        bool flag5 = flag4;
                        if (flag5)
                        {
                            Vector3 position2 = vrrig.transform.position;
                            bool flag6 = (position2 - position).sqrMagnitude <= 70f;
                            if (flag6)
                            {
                                try
                                {
                                    Vector3 vector = position2 - position;
                                    Vector3 vector2 = position2 + vector;
                                    vrrig.head.rigTarget.LookAt(vector2);
                                }
                                catch
                                {
                                }
                            }
                        }
                    }
                }
            }
        }

        public static GameObject Bat = GameObject.Find("Cave Bat Holdable");

        public static void GrabBat()
        {
            bool rightGrab = ControllerInputPoller.instance.rightGrab;
            if (rightGrab)
            {
                Fun.Bat.transform.position = GorillaTagger.Instance.rightHandTransform.position;
            }
            bool leftGrab = ControllerInputPoller.instance.leftGrab;
            if (leftGrab)
            {
                Fun.Bat.transform.position = GorillaTagger.Instance.leftHandTransform.position;
            }
        }

        public static GameObject Bug = GameObject.Find("Floating Bug Holdable");

        public static void GrabBug()
        {
            bool rightGrab = ControllerInputPoller.instance.rightGrab;
            if (rightGrab)
            {
                Fun.Bug.transform.position = GorillaTagger.Instance.rightHandTransform.position;
            }
            bool leftGrab = ControllerInputPoller.instance.leftGrab;
            if (leftGrab)
            {
                Fun.Bug.transform.position = GorillaTagger.Instance.leftHandTransform.position;
            }
        }

        private static GameObject draw;

        public static void Draw()
        {
            bool flag = ControllerInputPoller.instance == null;
            if (!flag)
            {
                bool flag2 = ControllerInputPoller.instance.rightGrab;
                if (flag2)
                {
                    Fun.draw = GameObject.CreatePrimitive(0);
                    Fun.draw.transform.position = GorillaTagger.Instance.rightHandTransform.position;
                    UnityEngine.Object.Destroy(Fun.draw.GetComponent<SphereCollider>());
                    Fun.draw.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                    Fun.draw.GetComponent<Renderer>().material.color = Color.black;
                    UnityEngine.Object.Destroy(Fun.draw, 20f);
                }
                bool leftGrab = ControllerInputPoller.instance.leftGrab;
                if (leftGrab)
                {
                    Fun.draw = GameObject.CreatePrimitive(0);
                    Fun.draw.transform.position = GorillaTagger.Instance.leftHandTransform.position;
                    UnityEngine.Object.Destroy(Fun.draw.GetComponent<SphereCollider>());
                    Fun.draw.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                    Fun.draw.GetComponent<Renderer>().material.color = Color.black;
                    UnityEngine.Object.Destroy(Fun.draw, 20f);
                }
            }
        }

        public static void ChaseGun()
        {
            if (GorillaTagger.Instance == null) return;

            GunTemplate.StartBothGuns(delegate
            {
                if (GunTemplate.lockedPlayer != null)
                {
                    GorillaTagger.Instance.offlineVRRig.enabled = false;

                    GorillaTagger.Instance.offlineVRRig.leftHandTransform.localPosition = new Vector3(-0.8f, 0.5f, 0f);
                    GorillaTagger.Instance.offlineVRRig.rightHandTransform.localPosition = new Vector3(0.8f, 0.5f, 0f);

                    Vector3 targetPos = GunTemplate.lockedPlayer.transform.position;
                    Transform myRigTransform = GorillaTagger.Instance.offlineVRRig.transform;

                    float distance = Vector3.Distance(myRigTransform.position, targetPos);

                    if (distance > 0.05f)
                    {
                        myRigTransform.position = Vector3.MoveTowards(
                            myRigTransform.position,
                            targetPos,
                            25f * Time.deltaTime
                        );
                        Tpose();
                    }
                    else
                    {
                        myRigTransform.position = targetPos;
                    }
                }
            }, true);

            if (GunTemplate.lockedPlayer == null || !ControllerInputPoller.instance.rightGrab)
            {
                if (GorillaTagger.Instance.offlineVRRig != null && !GorillaTagger.Instance.offlineVRRig.enabled)
                {
                    GorillaTagger.Instance.offlineVRRig.transform.position = GorillaLocomotion.GTPlayer.Instance.transform.position;
                    GorillaTagger.Instance.offlineVRRig.enabled = true;
                }
            }
        }


        private static float delaybetweenscore;
        public static void MaxQuestScore()
        {
            if (Time.time > delaybetweenscore)
            {
                delaybetweenscore = Time.time + 1f;
                VRRig.LocalRig.SetQuestScore(int.MaxValue);
            }
        }


        public static void HoverBoard()
        {
            GTPlayer.Instance.GrabPersonalHoverboard(false, Vector3.zero, Quaternion.identity, Color.black);
            GTPlayer.Instance.SetHoverAllowed(true, false);
            GTPlayer.Instance.SetHoverActive(true);
            VRRig.LocalRig.hoverboardVisual.gameObject.SetActive(true);
        }

        public static void UnlockAllGadgets()
        {
            foreach (var gadget in SIProgression.Instance.unlockedTechTreeData)
            {
                for (int i = 0; i < gadget.Length; i++) gadget[i] = true;
            }
        }


        public static void TallHead()
        {
            foreach (VRRig g in VRRigCache.ActiveRigs)
            {
                if (g == GorillaTagger.Instance.offlineVRRig) continue;

                g.headMesh.transform.localScale = new Vector3(g.transform.localScale.x, 2, g.transform.localScale.z);
            }
        }

        public static void ShortHead()
        {
            foreach (VRRig g in VRRigCache.ActiveRigs)
            {
                if (g == GorillaTagger.Instance.offlineVRRig) continue;

                g.headMesh.transform.localScale = new Vector3(2, g.transform.localScale.y, g.transform.localScale.z);
            }
        }

        public static void _2DMonke()
        {
            foreach (VRRig g in VRRigCache.ActiveRigs)
            {
                if (g == GorillaTagger.Instance.offlineVRRig) continue;

                g.transform.localScale = new Vector3(g.transform.localScale.x, g.transform.localScale.y, 0);
            }
        }

        public static void TallMonke()
        {
            foreach (VRRig g in VRRigCache.ActiveRigs)
            {
                if (g == GorillaTagger.Instance.offlineVRRig) continue;

                g.transform.localScale = new Vector3(g.transform.localScale.x, 2, g.transform.localScale.z);
            }
        }

        public static void ShortMonke()
        {
            foreach (VRRig g in VRRigCache.ActiveRigs)
            {
                if (g == GorillaTagger.Instance.offlineVRRig) continue;

                g.headMesh.transform.localScale = new Vector3(g.transform.localScale.x, 0.5f, g.transform.localScale.z);
            }
        }

        public static void BigMonke()
        {
            foreach (VRRig g in VRRigCache.ActiveRigs)
            {
                if (g == GorillaTagger.Instance.offlineVRRig) continue;

                g.headMesh.transform.localScale = new Vector3(2, 2, 2);
                g.transform.localScale = new Vector3(2, 2, 2);
            }
        }

        public static void FixMonke()
        {
            foreach (VRRig g in VRRigCache.ActiveRigs)
            {
                if (g == GorillaTagger.Instance.offlineVRRig) continue;

                g.headMesh.transform.localScale = new Vector3(1, 1, 1);
                g.transform.localScale = new Vector3(1, 1, 1);
            }
        }

    }
}
