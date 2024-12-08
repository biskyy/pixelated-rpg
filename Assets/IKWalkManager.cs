using UnityEngine;

public class IKWalkManager : MonoBehaviour
{
  [SerializeField] public Transform root;
  [SerializeField] public LayerMask walkableLayer;
  [SerializeField] public IKFoot[] feet;
  [SerializeField] public int legsAllowToMovedAtTheSameTime = 1;
  [SerializeField] public AnimationCurve footAnimationCurve;
  [SerializeField] public float footAnimationDuration = 2f;
  [SerializeField] public float footAnimationSpeed = 2f;
  [SerializeField] public float minStepDistance = 3f;
  [SerializeField] public Vector3 stepRandomness = new Vector3(0.2f, 0, 0.2f);
  [SerializeField] public bool lockedToGround = true;
  [SerializeField] public float rotationThreshold = 25f;
  public Vector3 prevPosition, globalVelocity, localVelocity;

  void Start()
  {
    feet = GetComponentsInChildren<IKFoot>();
    prevPosition = transform.position;
  }

  void Update()
  {
    globalVelocity = (transform.position - prevPosition) / Time.deltaTime;
    prevPosition = transform.position;
    globalVelocity = new Vector3(
      Mathf.Clamp(globalVelocity.x, -minStepDistance / 2, minStepDistance / 2),
      globalVelocity.y,
      Mathf.Clamp(globalVelocity.z, -minStepDistance / 2, minStepDistance / 2));
    // Calculate local velocity which also works with root rotations
    localVelocity = transform.InverseTransformDirection(globalVelocity);
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