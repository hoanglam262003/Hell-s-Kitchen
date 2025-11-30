using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class ParentAndAdjustOffset : NetworkTransform
{
    public GameObject ParentObject;
    public Vector3 Offset;

    /// <summary>
    /// Override this so this NetworkTransform becomes client-authoritative
    /// (same behavior as the removed ClientNetworkTransform)
    /// </summary>
    protected override bool OnIsServerAuthoritative()
    {
        return false; // => Disable server authority (client authority enabled)
    }

    private bool IsMotionAuthority()
    {
        // Motion authority = client owner
        return IsOwner;
    }

    public override void OnNetworkSpawn()
    {
        // Only motion authority (client owner) applies the parenting and offset
        if (IsMotionAuthority())
        {
            if (ParentObject != null)
            {
                NetworkObject.TrySetParent(ParentObject, false);

                if (!SwitchTransformSpaceWhenParented)
                {
                    InLocalSpace = true;
                }

                transform.localPosition = Offset;
            }
        }

        base.OnNetworkSpawn();
    }
}