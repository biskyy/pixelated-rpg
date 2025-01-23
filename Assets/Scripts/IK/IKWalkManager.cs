using UnityEngine;

public class IKWalkManager : MonoBehaviour
{
  // ---- General ---- //
  [Header("General")]
  [SerializeField] public Transform root;
  [SerializeField] public bool lockedToGround = true;
  [SerializeField] public LayerMask walkableLayer;
  [SerializeField] private IKFoot[] feet;
  [SerializeField] public int legsAllowToMovedAtTheSameTime = 1;
  [SerializeField] public float minStepDistanceForward = 3f;
  [SerializeField] public float minStepDistanceRight = 3f;
  [SerializeField] public Vector3 stepRandomness = new Vector3(0.2f, 0, 0.2f);
  [SerializeField] public float rotationThreshold = 25f;

  // ---- Feet animation ---- //
  [Header("Feet Animation")]
  [SerializeField] public AnimationCurve footAnimationCurve;
  [SerializeField] public float footAnimationDuration = 1f;
  [SerializeField] public float footAnimationSpeed = 2f;


  // ---- Root velocity calculations ---- //
  [Header("Root Velocity")]
  [SerializeField] public Vector3 globalVelocity;
  [SerializeField] public Vector3 localVelocity;
  private Vector3 prevPosition, smoothedVelocity;

  // Smoothing factor:
  // 0 = No smoothing (velocity remains at 0, no movement smoothing applied)
  // Values close to 0 (e.g., 0.0001) = Very high smoothing (slow response to changes)
  // 1 = No smoothing applied (immediate response to changes)
  [SerializeField] private float smoothingFactor = 0.05f;

  // Minimum velocity magnitude to consider as moving
  [SerializeField] private float velocityDeadZoneThreshold = 0.01f;

  void Start()
  {
    feet = GetComponentsInChildren<IKFoot>();
    prevPosition = transform.position;
  }

  void Update()
  {

    CalculateRootVelocity();
  }

  private void CalculateRootVelocity()
  {
    // Calculate the current global velocity
    Vector3 currentVelocity = (root.position - prevPosition) / Time.deltaTime;

    // Smooth the global velocity
    smoothedVelocity = Vector3.Lerp(smoothedVelocity, currentVelocity, smoothingFactor);

    // Convert to local space to align with root transform and avoid errors
    smoothedVelocity = transform.InverseTransformDirection(smoothedVelocity);

    // Clamp the smoothed velocity
    smoothedVelocity = smoothedVelocity.With(
        x: Mathf.Clamp(smoothedVelocity.x, -minStepDistanceRight / 3f, minStepDistanceRight / 3f),
        z: Mathf.Clamp(smoothedVelocity.z, -minStepDistanceForward / 1.5f, minStepDistanceForward / 1.5f)
    );

    // Apply dead zone threshold to avoid floating-point drift
    smoothedVelocity = ApplyVelocityDeadZone(smoothedVelocity, velocityDeadZoneThreshold);

    // Convert back to world space
    smoothedVelocity = transform.TransformDirection(smoothedVelocity);

    // Update the previous position for the next frame
    prevPosition = root.position;

    globalVelocity = smoothedVelocity;

    localVelocity = root.InverseTransformDirection(globalVelocity);
  }

  // Function to apply dead zone to a velocity vector
  private Vector3 ApplyVelocityDeadZone(Vector3 velocity, float threshold)
  {
    if (velocity.magnitude < threshold)
    {
      return Vector3.zero;
    }
    return velocity;
  }

  private Vector3 GetCenterOfFeet()
  {
    Vector3 totalPosition = Vector3.zero;

    foreach (IKFoot foot in feet)
      totalPosition += foot.transform.localPosition;

    return totalPosition / feet.Length;
  }

  // private void SyncTargetsToBones() {
  //   for (int i = 0; i < feet.Length; i++)
  //     feet[i].transform.position = feetBones[i].position;
  // }

  public int GetFeetMoving()
  {
    int counter = 0;
    foreach (IKFoot foot in feet)
    {
      if (foot.IsMoving())
        counter++;
    }
    return counter;
  }

}