using UnityEngine;

public static class RigExtensions
{
    public static bool Active(this VRRig rig)
    {
        return rig != null && rig.gameObject.activeInHierarchy;
    }

    public static VRRig GetClosest(this VRRig localRig)
    {
        VRRig closest = null;
        float minDistance = float.MaxValue;
        Vector3 currentPos = localRig.transform.position;

        foreach (VRRig rig in VRRigCache.ActiveRigs)
        {
            if (rig == localRig || rig == null) continue;

            float distance = Vector3.Distance(currentPos, rig.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = rig;
            }
        }
        return closest;
    }
}
