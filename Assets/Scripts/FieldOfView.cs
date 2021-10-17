using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FieldOfView : MonoBehaviour {

	public float viewRadius = 7f;
	[Range(0,360)]
	public float viewAngle = 70f;

	public LayerMask targetMask;
	public LayerMask obstacleMask;

	[HideInInspector]
	public List<Transform> visibleTargets = new List<Transform>();

	private Transform mainTarget;
	private float distanceMainTarget = 0;

	void Start() {
		StartCoroutine ("FindTargetsWithDelay", .2f);
	}


	IEnumerator FindTargetsWithDelay(float delay) {
		while (true) {
			yield return new WaitForSeconds (delay);
			FindVisibleTargets();
		}
	}

	void FindVisibleTargets() {
		visibleTargets.Clear();
		mainTarget = null;
		distanceMainTarget = 0;
		Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, targetMask);

		for (int i = 0; i < targetsInViewRadius.Length; i++) {
			Transform target = targetsInViewRadius[i].transform;
			Vector3 dirToTarget = (target.position - transform.position).normalized;
			if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2) {
				float distanceTarget = Vector3.Distance (transform.position, target.position);

				if (!Physics.Raycast (transform.position, dirToTarget, distanceTarget, obstacleMask)) {
					if(distanceMainTarget == 0f || distanceTarget < distanceMainTarget)
                    {
						distanceMainTarget = distanceTarget;
						mainTarget = target;
						visibleTargets.Add(target);
					}
				}
			}
		}

		if(mainTarget != null)
        {
			gameObject.SendMessage("TargetViwed", mainTarget, SendMessageOptions.DontRequireReceiver);
        }
        else
        {
			gameObject.SendMessage("NoTargetViwed", SendMessageOptions.DontRequireReceiver);
		}
	}

	public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal) {
		if (!angleIsGlobal) {
			angleInDegrees += transform.eulerAngles.y;
		}
		return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad),0,Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
	}
}
