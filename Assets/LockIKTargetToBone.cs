using UnityEngine;

[RequireComponent(typeof(Transform))]
public class LockIKTargetToBone : MonoBehaviour
{
  [SerializeField] private Transform boneEnd; // Assign the bone's end transform in the Inspector

  // private Vector3 initialOffset; // To preserve relative position if needed

  void Start()
  {
    if (boneEnd == null)
    {
      Debug.LogError("Bone end is not assigned. Please assign the bone's end Transform.");
      enabled = false;
      return;
    }

    // Calculate initial offset between the target and the bone's end
    // initialOffset = transform.position - boneEnd.position;
  }

  void LateUpdate()
  {
    // Lock the position to the end of the bone while preserving the offset
    transform.position = boneEnd.position;
    // + initialOffset;
  }
}