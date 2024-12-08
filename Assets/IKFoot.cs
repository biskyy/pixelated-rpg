using UnityEngine;

public class IKFoot : MonoBehaviour
{
  private IKWalkManager manager;
  private Vector3 offsetFromRoot;
  public Vector3 prevTarget, currTarget, nextTarget;
  public Vector3 prevNormal, currNormal, nextNormal;
  public Vector3 prevRotation, currRotation, nextRotation;
  private float lerp;
  [SerializeField] Vector3 targetOffset;

  void Start()
  {
    manager = GetComponentInParent<IKWalkManager>();
    offsetFromRoot = transform.localPosition;

    currTarget = nextTarget = prevTarget = transform.position;
    currNormal = nextNormal = prevNormal = transform.position;
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


      if (Physics.Raycast(manager.root.TransformPoint(
        offsetFromRoot
        + randomOffset
        + Vector3.up * 3f
        + Vector3.forward * manager.localVelocity.z
        + Vector3.right * manager.localVelocity.x
        ), Vector3.down, out RaycastHit info, 10f, manager.walkableLayer))
      {
        // Check if root has rotated
        float deltaY = Mathf.Abs(Mathf.DeltaAngle(manager.root.rotation.eulerAngles.y, prevRotation.y));
        if (deltaY > manager.rotationThreshold && manager.GetFeetMoving() < manager.legsAllowToMovedAtTheSameTime && lerp >= 1)
        {
          lerp = 0;
          nextTarget = info.point;
          prevRotation = manager.root.rotation.eulerAngles;
        }

        if (Vector3.Distance(currTarget, info.point) > manager.minStepDistance
          && manager.GetFeetMoving() < manager.legsAllowToMovedAtTheSameTime
          && lerp >= 1
          )
        {
          lerp = 0;
          nextTarget = info.point;
        }

      }
    }

    if (lerp < manager.footAnimationDuration)
    {
      float normalizedTime = lerp / manager.footAnimationDuration;

      float yOffset = manager.footAnimationCurve.Evaluate(normalizedTime);

      Vector3 inBetweenPosition = Vector3.Lerp(prevTarget, nextTarget, lerp);
      inBetweenPosition.y += yOffset;

      currTarget = inBetweenPosition;
      currNormal = Vector3.Lerp(prevNormal, nextNormal, lerp);

      lerp += Time.deltaTime * manager.footAnimationSpeed;
      // if (lerp > manager.footAnimationDuration) lerp = 0;
    }
    else
    {
      prevTarget = nextTarget;
      prevNormal = nextNormal;
    }
  }

  private void OnDrawGizmos()
  {
    Gizmos.color = Color.green;
    Gizmos.DrawSphere(nextTarget, 0.2f);

    if (manager)
    {
      Gizmos.color = Color.blue;
      print(manager.localVelocity);
      Gizmos.DrawSphere(manager.root.TransformPoint(
        offsetFromRoot
        + Vector3.up
        + Vector3.forward * manager.localVelocity.z
        + Vector3.right * manager.localVelocity.x
      ), 0.2f);
    }

    // Gizmos.color = Color.red;
    // Gizmos.Draw
  }

  public bool IsMoving()
  {
    return lerp < 1;
  }

}