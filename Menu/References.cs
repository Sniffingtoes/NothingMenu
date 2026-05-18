using Nothing.Classes;
using UnityEngine;

namespace Nothing.Menu
{
    public partial class Main
    {
        public static void CreateReference(bool isRightHanded)
        {
            reference = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            reference.transform.parent = isRightHanded ? GorillaTagger.Instance.leftHandTransform : GorillaTagger.Instance.rightHandTransform;
            SetupReference(reference, out buttonCollider);
        }

        public static void CreateReference2(bool isRightHanded)
        {
            reference2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            reference2.transform.parent = isRightHanded ? GorillaTagger.Instance.rightHandTransform : GorillaTagger.Instance.leftHandTransform;
            SetupReference(reference2, out buttonCollider2);
        }

        private static void SetupReference(GameObject obj, out SphereCollider collider)
        {
            obj.GetComponent<Renderer>().material.color = ThemeManager.GetColors().Background;
            obj.transform.localPosition = new Vector3(0f, -0.1f, 0f);
            obj.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

            collider = obj.GetComponent<SphereCollider>();
            collider.isTrigger = true;

            if (obj.GetComponent<ColorChanger>() == null)
            {
                obj.AddComponent<ColorChanger>();
            }
        }
    }
}
