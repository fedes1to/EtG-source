using System;
using System.Collections.Generic;
using UnityEngine;

public class PersistentVFXBehaviour : BraveBehaviour
{
	public void BecomeDebris(Vector3 startingForce, float startingHeight, params Type[] keepComponents)
	{
		List<Type> list = new List<Type>(keepComponents);
		Component[] components = GetComponents<Component>();
		foreach (Component component in components)
		{
			if (!(component is tk2dBaseSprite) && !(component is tk2dSpriteAnimator) && !(component is Renderer) && !(component is MeshFilter) && !(component is DebrisObject) && !(component is SpriteAnimatorStopper) && !(component is Transform) && !list.Contains(component.GetType()))
			{
				UnityEngine.Object.Destroy(component);
			}
		}
		DebrisObject orAddComponent = base.gameObject.GetOrAddComponent<DebrisObject>();
		orAddComponent.angularVelocity = 45f;
		orAddComponent.angularVelocityVariance = 20f;
		orAddComponent.decayOnBounce = 0.5f;
		orAddComponent.bounceCount = 1;
		orAddComponent.canRotate = true;
		orAddComponent.Trigger(startingForce, startingHeight);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
