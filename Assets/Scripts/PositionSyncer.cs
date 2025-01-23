using UnityEngine;

public class PositionSyncer : MonoBehaviour
{
  [Header("Target Object")]
  [SerializeField] private Transform target; // The target object to sync to

  [Header("Objects to Sync")]
  [SerializeField] private Transform[] objectsToSync; // List of objects to synchronize

  [Header("Sync Options")]
  [SerializeField] private bool syncPosition = true;
  [SerializeField] private bool syncRotation = true;
  [SerializeField] private bool syncScale = false; // Optional: Sync scale

  [Header("Smoothing")]
  [SerializeField] private bool smoothSync = false;
  [SerializeField] private float smoothSpeed = 5f; // Speed for smoothing

  void Update()
  {
    foreach (Transform obj in objectsToSync)
    {
      if (obj == null || target == null) continue;

      // Sync position
      if (syncPosition)
      {
        if (smoothSync)
          obj.position = Vector3.Lerp(obj.position, target.position, Time.deltaTime * smoothSpeed);
        else
          obj.position = target.position;
      }

      // Sync rotation
      if (syncRotation)
      {
        if (smoothSync)
          obj.rotation = Quaternion.Lerp(obj.rotation, target.rotation, Time.deltaTime * smoothSpeed);
        else
          obj.rotation = target.rotation;
      }

      // Sync scale
      if (syncScale)
      {
        if (smoothSync)
          obj.localScale = Vector3.Lerp(obj.localScale, target.localScale, Time.deltaTime * smoothSpeed);
        else
          obj.localScale = target.localScale;
      }
    }
  }
}
