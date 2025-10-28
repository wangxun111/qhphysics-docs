using System;
using UnityEngine;

namespace QH.Physics
{
	public class SphereCollider : ColliderBase
	{
		public Vector3 position;
		public Single radius;

		public SphereCollider(UnityEngine.SphereCollider sourceCollider)
		{
			InstanceID = sourceCollider.gameObject.GetInstanceID();
			radius = sourceCollider.radius * sourceCollider.transform.lossyScale.x;
			SyncPosition(sourceCollider);
		}

		public override void Sync(ICollider source)
		{
			SphereCollider sphereCollider = source as SphereCollider;
			position = sphereCollider.position;
			radius = sphereCollider.radius;
			InstanceID = sphereCollider.InstanceID;
		}

		public void SyncPosition(UnityEngine.SphereCollider sourceCollider)
		{
			Vector3 center = sourceCollider.center;
			center.x *= sourceCollider.transform.lossyScale.x;
			center.y *= sourceCollider.transform.lossyScale.y;
			center.z *= sourceCollider.transform.lossyScale.z;
			center = sourceCollider.transform.rotation * center;
			position = sourceCollider.transform.position + center;
		}

		public override Vector3 TestPoint(Vector3 point, Vector3 prevPoint, out Vector3 normal)
		{
			Vector3 vector = point - position;
			Single magnitude = vector.magnitude;
			if (magnitude < radius)
			{
				if (!Mathf.Approximately(magnitude, 0f))
				{
					normal = vector / magnitude;
				}
				else
				{
					normal = Vector3.up;
				}
				return normal * (radius - magnitude);
			}
			normal = Vector3.zero;
			return Vector3.zero;
		}

		public void DebugDraw(Color c)
		{
		}
	}
}
