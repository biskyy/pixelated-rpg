using UnityEngine;

public class IKFoot : MonoBehaviour
{
  private IKWalkManager manager;
  private Vector3 initialLocalPosition;
  private Vector3 prevTarget, currTarget, nextTarget;
  private Vector3 prevNormal, currNormal, nextNormal;
  private Vector3 prevRotation;
  public Vector3 rayPositionOffset = new Vector3(0, 1, 0);
  public float rayLength = 0.25f;
  private float lerp;

  private Vector3 currStepRayPosition, nextStepRayPosition;

  void Start()
  {
    manager = GetComponentInParent<IKWalkManager>();
    initialLocalPosition = transform.localPosition;

    currTarget = nextTarget = prevTarget = transform.position;
    currNormal = nextNormal = prevNormal = transform.position;
    prevRotation = new Vector3(0, 0, 0);
    lerp = 1;
  }

  void Update()
  {
    transform.position = currTarget;
    transform.up = currNormal;

    if (manager.lockedToGround)
    {
      // Calculate 2 random values to offset the foot by
      Vector3 randomOffset = new Vector3(
        Random.Range(-manager.stepRandomness.x, manager.stepRandomness.x),
        0,
        Random.Range(-manager.stepRandomness.z, manager.stepRandomness.z)
      );

      // Check if root has rotated
      float rootRotationDeltaY = Mathf.Abs(Mathf.DeltaAngle(manager.root.rotation.eulerAngles.y, prevRotation.y));

      // ---- Raycast relative to the root for checking next step ---- //
      nextStepRayPosition =
        // Get world space position of self relative to the root
        manager.root.TransformPoint(initialLocalPosition)
        + rayPositionOffset
        // Add root velocity for guessing next step
        + Vector3.forward * manager.globalVelocity.z
        + Vector3.right * manager.globalVelocity.x;

      Ray nextStepRay = new Ray(nextStepRayPosition, Vector3.down);

      if (Physics.Raycast(nextStepRay, out RaycastHit nextStepInfo, rayPositionOffset.y + rayLength, manager.walkableLayer))
      {
        if (rootRotationDeltaY > manager.rotationThreshold
          && manager.GetFeetMoving() < manager.legsAllowToMovedAtTheSameTime
          && !IsMoving())
        {
          lerp = 0;
          nextTarget = nextStepInfo.point + randomOffset;
          prevRotation = manager.root.rotation.eulerAngles;
        }

        if ((Mathf.Abs(currTarget.z - nextStepInfo.point.z) > manager.minStepDistanceForward)
          && manager.GetFeetMoving() < manager.legsAllowToMovedAtTheSameTime
          && !IsMoving())
        {
          lerp = 0;
          nextTarget = nextStepInfo.point + randomOffset;
        }
        if ((Mathf.Abs(currTarget.x - nextStepInfo.point.x) > manager.minStepDistanceRight)
          && manager.GetFeetMoving() < manager.legsAllowToMovedAtTheSameTime
          && !IsMoving())
        {
          lerp = 0;
          nextTarget = nextStepInfo.point + randomOffset;
        }

      }

      // ---- Raycast from the current foot position to the ground ---- //
      currStepRayPosition =
        manager.root.TransformPoint(transform.localPosition)
        + rayPositionOffset;

      currStepRayPosition.y = initialLocalPosition.y + rayPositionOffset.y;

      Ray currStepRay = new Ray(currStepRayPosition, Vector3.down);

      if (Physics.Raycast(currStepRay, out RaycastHit currStepInfo, rayPositionOffset.y + rayLength, manager.walkableLayer))
      {
        // Only change nextTarget if root isn't moving and self isn't moving
        if (manager.globalVelocity.magnitude == 0 && !IsMoving())
        {
          nextTarget = currStepInfo.point;
        }
      }
    }

    UpdateFootAnimation();
  }

  private void UpdateFootAnimation()
  {
    if (lerp < manager.footAnimationDuration)
    {
      float normalizedTime = lerp / manager.footAnimationDuration;

      float yOffset = manager.footAnimationCurve.Evaluate(normalizedTime);

      Vector3 inBetweenPosition = Vector3.Lerp(prevTarget, nextTarget, lerp);
      inBetweenPosition.y += yOffset;

      currTarget = inBetweenPosition;
      currNormal = Vector3.Lerp(prevNormal, nextNormal, lerp);

      lerp += Time.deltaTime * manager.footAnimationSpeed;
    }
    else
    {
      prevTarget = nextTarget;
      currTarget = nextTarget;

      prevNormal = nextNormal;
      currNormal = nextNormal;
    }
  }

  public bool IsMoving()
  {
    return lerp < manager.footAnimationDuration;
  }

  private void OnDrawGizmos()
  {
    Gizmos.color = Color.green;
    Gizmos.DrawSphere(nextTarget, 0.05f);

    if (manager)
    {
      Gizmos.color = Color.blue;
      Gizmos.DrawSphere(nextStepRayPosition, 0.05f);
      Gizmos.DrawRay(nextStepRayPosition, Vector3.down * rayLength);

      Gizmos.color = Color.magenta;
      Gizmos.DrawSphere(currStepRayPosition, 0.05f);
      Gizmos.DrawRay(currStepRayPosition, Vector3.down * rayLength);
    }
  }

}