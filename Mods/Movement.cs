using BepInEx;
using Custom.Inputs;
using GorillaLocomotion;
using GorillaLocomotion.Swimming;
using Nothing.Menu;
using Oculus.Platform;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityEngine.Windows;
using UnityEngine.XR;

using Valve.VR;

using static Nothing.Menu.GunTemplate;

namespace Nothing.Mods
{
    public class Movement
    {
        public static GameObject platl;
        public static GameObject platr;

        public static void Platforms()
        {
            Color platformColor = Color.black;

            if (Get.leftGrab)
            {
                if (platl == null)
                {
                    platl = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    platl.transform.localScale = new Vector3(0.025f, 0.3f, 0.4f);
                    platl.transform.position = GorillaTagger.Instance.leftHandTransform.position;
                    platl.transform.rotation = GorillaTagger.Instance.leftHandTransform.rotation;
                    platl.GetComponent<Renderer>().material.color = platformColor;

                    var surface = platl.AddComponent<GorillaSurfaceOverride>();
                    surface.overrideIndex = 0;
                }
            }
            else if (platl != null)
            {
                UnityEngine.Object.Destroy(platl);
                platl = null;
            }

            if (Get.rightGrab)
            {
                if (platr == null)
                {
                    platr = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    platr.transform.localScale = new Vector3(0.025f, 0.3f, 0.4f);
                    platr.transform.position = GorillaTagger.Instance.rightHandTransform.position;
                    platr.transform.rotation = GorillaTagger.Instance.rightHandTransform.rotation;
                    platr.GetComponent<Renderer>().material.color = platformColor;

                    var surface = platr.AddComponent<GorillaSurfaceOverride>();
                    surface.overrideIndex = 0;
                }
            }
            else if (platr != null)
            {
                UnityEngine.Object.Destroy(platr);
                platr = null;
            }
        }

        public static float SpeedBoostSpeed = 6.3f;
        public static class BoostSettings { public static string[] labels = { "slow", "normal", "fast" }; public static float[] values = { 6.3f, 7.3f, 8.0f }; public static int index = 0; }

        public static void SpeedBoost()
        {
            GorillaLocomotion.GTPlayer.Instance.maxJumpSpeed = SpeedBoostSpeed;
            GorillaLocomotion.GTPlayer.Instance.jumpMultiplier = SpeedBoostSpeed;
        }

        public static void GripSpeedBoost()
        {
            if (Get.rGripFloat || Get.lGripFloat)
            {
                SpeedBoost();
            }
        }

        public static float FlySpeed = 15f;
        public static class FlySettings { public static string[] labels = { "really slow", "slow", "fast", "super fast" }; public static float[] values = { 5f, 15f, 30f, 60f }; public static int index { get { return PlayerPrefs.GetInt("FlySpeedIndex", 1); } set { PlayerPrefs.SetInt("FlySpeedIndex", value); PlayerPrefs.Save(); } } }

        public static void Fly()
        {
            if (Get.bButton)
            {
                GTPlayer.Instance.transform.position += GorillaTagger.Instance.headCollider.transform.forward * (Time.deltaTime * FlySpeed);

                GorillaTagger.Instance.rigidbody.linearVelocity = Vector3.zero;
            }
        }

        public static void TriggerFly()
        {
            if (Get.rTrigger)
            {
                GTPlayer.Instance.transform.position += GorillaTagger.Instance.headCollider.transform.forward * (Time.deltaTime * FlySpeed);
                GorillaTagger.Instance.rigidbody.linearVelocity = Vector3.zero;
            }
        }

        public static void HandFly()
        {
            if (Get.bButton)
            {
                GTPlayer.Instance.transform.position += GorillaTagger.Instance.rightHandTransform.transform.forward * (Time.deltaTime * FlySpeed);
                GorillaTagger.Instance.rigidbody.linearVelocity = Vector3.zero;
            }
        }

        public static void SlingshotFly()
        {
            if (Get.bButton)
                GorillaTagger.Instance.rigidbody.linearVelocity += GTPlayer.Instance.headCollider.transform.forward * (Time.deltaTime * (FlySpeed * 2));
        }

        public static void JoystickFly()
        {
            Vector3 right = GTPlayer.Instance.bodyCollider.transform.right;
            Vector3 forward = GTPlayer.Instance.bodyCollider.transform.forward;

            right.y = 0f;
            forward.y = 0f;
            right.Normalize();
            forward.Normalize();

            Vector3 inputMovement = new Vector3(
                SteamVR_Actions.gorillaTag_LeftJoystick2DAxis.axis.x,
                SteamVR_Actions.gorillaTag_RightJoystick2DAxis.axis.y,
                SteamVR_Actions.gorillaTag_LeftJoystick2DAxis.axis.y
            );

            Vector3 desiredMovement = (inputMovement.x * right) + (inputMovement.z * forward) + (inputMovement.y * Vector3.up);

            desiredMovement *= (Time.deltaTime * FlySpeed * 100f);

            Rigidbody rb = GTPlayer.Instance.bodyCollider.attachedRigidbody;

            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, desiredMovement, 0.1287f);

            const float gravityForce = 9.81f;
            rb.AddForce(Vector3.up * gravityForce, ForceMode.Acceleration);
        }

        public static void NoclipFly()
        {
            if (Get.bButton)
            {
                MeshCollider[] array = Resources.FindObjectsOfTypeAll<MeshCollider>();
                for (int i = 0; i < array.Length; i++)
                {
                    array[i].enabled = false;
                }

                GTPlayer.Instance.transform.position += GTPlayer.Instance.headCollider.transform.forward * (Time.deltaTime * FlySpeed);

                GTPlayer.Instance.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
            }
            else
            {
                MeshCollider[] array = Resources.FindObjectsOfTypeAll<MeshCollider>();
                for (int i = 0; i < array.Length; i++)
                {
                    array[i].enabled = true;
                }
            }
        }
        public static void NoclipFlyTrigger()
        {
            if (Get.rIndexFloat)
            {
                MeshCollider[] array = Resources.FindObjectsOfTypeAll<MeshCollider>();
                for (int i = 0; i < array.Length; i++)
                {
                    array[i].enabled = false;
                }

                GTPlayer.Instance.transform.position += GTPlayer.Instance.headCollider.transform.forward * (Time.deltaTime * FlySpeed);

                GTPlayer.Instance.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
            }
            else
            {
                MeshCollider[] array = Resources.FindObjectsOfTypeAll<MeshCollider>();
                for (int i = 0; i < array.Length; i++)
                {
                    array[i].enabled = true;
                }
            }
        }


        public static void ZeroGravity()
        {
            Physics.gravity = Vector3.zero;
        }

        private static bool Ir;
        private static GameObject Thingy;

        public static void Checkpoint()
        {
            if (Get.rightGrab && !Movement.Ir)
            {
                Movement.Thingy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                UnityEngine.Object.Destroy(Movement.Thingy.GetComponent<SphereCollider>());

                Movement.Thingy.GetComponent<Renderer>().material.color = Color.black;
                Movement.Thingy.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                Movement.Thingy.transform.position = GorillaTagger.Instance.rightHandTransform.position;

                Movement.Ir = true;
            }

            bool isTeleporting = Get.bButton;

            if (isTeleporting && Movement.Thingy != null)
            {
                GorillaTagger.Instance.transform.position = Movement.Thingy.transform.position;
                if (GTPlayer.Instance != null)
                    GTPlayer.Instance.transform.position = Movement.Thingy.transform.position;
            }

            MeshCollider[] colliders = Resources.FindObjectsOfTypeAll<MeshCollider>();
            foreach (MeshCollider collider in colliders)
            {
                collider.enabled = !isTeleporting;
            }

            if (Get.rTrigger)
            {
                Movement.Ir = false;
                UnityEngine.Object.Destroy(Movement.Thingy);
            }

        }
        public static void SlideControl()
        {
            GTPlayer.Instance.slideControl = 1E+10f;
        }

        internal static Vector3 previousMousePosition;
        public static float speed = 17f;

        public static void WASD()
        {
            GorillaTagger.Instance.rigidbody.useGravity = false;
            GorillaTagger.Instance.rigidbody.linearVelocity = Vector3.zero;

            float currentSpeed = Movement.FlySpeed * Time.deltaTime;

            if (UnityInput.Current.GetKey(KeyCode.W))
            {
                GorillaTagger.Instance.transform.position += GorillaTagger.Instance.headCollider.transform.forward * currentSpeed;
            }
            if (UnityInput.Current.GetKey(KeyCode.A))
            {
                GorillaTagger.Instance.transform.position += GorillaTagger.Instance.headCollider.transform.right * currentSpeed * -1f;
            }
            if (UnityInput.Current.GetKey(KeyCode.S))
            {
                GorillaTagger.Instance.transform.position += GorillaTagger.Instance.headCollider.transform.forward * currentSpeed * -1f;
            }
            if (UnityInput.Current.GetKey(KeyCode.D))
            {
                GorillaTagger.Instance.transform.position += GorillaTagger.Instance.headCollider.transform.right * currentSpeed;
            }
            if (UnityInput.Current.GetKey(KeyCode.Space))
            {
                GorillaTagger.Instance.transform.position += Vector3.up * currentSpeed;
            }
            if (UnityInput.Current.GetKey(KeyCode.LeftShift))
            {
                GorillaTagger.Instance.transform.position += Vector3.down * currentSpeed;
            }

            if (UnityInput.Current.GetMouseButton(1))
            {
                Vector3 mouseDelta = UnityInput.Current.mousePosition - Movement.previousMousePosition;

                float sensitivity = 0.3f;
                Vector3 newRotation = Camera.main.transform.localEulerAngles;
                newRotation.y += mouseDelta.x * sensitivity;
                newRotation.x -= mouseDelta.y * sensitivity;
                Camera.main.transform.localEulerAngles = newRotation;
            }

            Movement.previousMousePosition = UnityInput.Current.mousePosition;
        }

        public static bool canTeleport = true;

        public static void TeleportGun()
        {
            if (!Get.rIndexFloat && !Mouse.current.leftButton.isPressed)
            {
                canTeleport = true;
            }

            StartBothGuns(() =>
            {
                if (canTeleport && nray.collider != null)
                {
                    Rigidbody rb = GorillaTagger.Instance.GetComponent<Rigidbody>();
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    Vector3 destination = nray.point + (nray.normal * 0.1f);
                    GorillaTagger.Instance.transform.position = destination;
                    rb.position = destination;
                    GorillaTagger.Instance.StartVibration(true, 0.1f, 0.05f);

                    canTeleport = false;
                }
            }, false);
        }

        public static void upandDown()
        {
            if (Get.rightGrab)
            {
                GTPlayer.Instance.transform.position += GTPlayer.Instance.bodyCollider.transform.up * Time.deltaTime * 8f;
                GTPlayer.Instance.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
            }
            if (Get.leftGrab)
            {
                GTPlayer.Instance.transform.position += GTPlayer.Instance.bodyCollider.transform.up * Time.deltaTime * -8f;
                GTPlayer.Instance.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
            }
        }

        public static void leftandright()
        {
            if (Get.rightGrab)
            {
                GTPlayer.Instance.transform.position += GTPlayer.Instance.bodyCollider.transform.right * Time.deltaTime * 8f;
                GTPlayer.Instance.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
            }
            if (Get.leftGrab)
            {
                GTPlayer.Instance.transform.position += GTPlayer.Instance.bodyCollider.transform.right * Time.deltaTime * -8f;
                GTPlayer.Instance.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
            }
        }

        public static void forwardsandbackwards()
        {
            if (Get.rightGrab)
            {
                GTPlayer.Instance.transform.position += GTPlayer.Instance.bodyCollider.transform.forward * Time.deltaTime * -8f;
                GTPlayer.Instance.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
            }
            if (Get.leftGrab)
            {
                GTPlayer.Instance.transform.position += GTPlayer.Instance.bodyCollider.transform.forward * Time.deltaTime * 8f;
                GTPlayer.Instance.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
            }
        }

        public static void NoTagFreeze()
        {
            GTPlayer.Instance.disableMovement = false;
        }

        public static void ForceTagFreeze()
        {
            GTPlayer.Instance.disableMovement = true;
        }


    }

}
