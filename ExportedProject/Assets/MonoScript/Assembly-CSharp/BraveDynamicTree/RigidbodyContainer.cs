using System;
using UnityEngine;

namespace BraveDynamicTree
{
	public interface RigidbodyContainer
	{
		void RayCast(b2RayCastInput input, Func<b2RayCastInput, SpeculativeRigidbody, float> callback);

		void Query(b2AABB aabb, Func<SpeculativeRigidbody, bool> callback);

		int CreateProxy(b2AABB aabb, SpeculativeRigidbody rigidbody);

		bool MoveProxy(int proxyId, b2AABB aabb, Vector2 displacement);

		void DestroyProxy(int proxyId);
	}
}
