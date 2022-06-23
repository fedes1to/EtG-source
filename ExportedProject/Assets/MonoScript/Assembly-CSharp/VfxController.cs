using System.Collections.Generic;
using UnityEngine;

public class VfxController : BraveBehaviour
{
	public List<AIAnimator.NamedVFXPool> OtherVfx;

	protected override void OnDestroy()
	{
		base.OnDestroy();
		for (int i = 0; i < OtherVfx.Count; i++)
		{
			AIAnimator.NamedVFXPool namedVFXPool = OtherVfx[i];
			namedVFXPool.vfxPool.DestroyAll();
		}
	}

	public VFXPool GetVfx(string name)
	{
		AIAnimator.NamedVFXPool namedVFXPool = OtherVfx.Find((AIAnimator.NamedVFXPool n) => n.name == name);
		if (namedVFXPool != null)
		{
			return namedVFXPool.vfxPool;
		}
		return null;
	}

	public void PlayVfx(string name, Vector2? sourceNormal = null, Vector2? sourceVelocity = null)
	{
		for (int i = 0; i < OtherVfx.Count; i++)
		{
			AIAnimator.NamedVFXPool namedVFXPool = OtherVfx[i];
			if (namedVFXPool.name == name)
			{
				if ((bool)namedVFXPool.anchorTransform)
				{
					namedVFXPool.vfxPool.SpawnAtLocalPosition(Vector3.zero, 0f, namedVFXPool.anchorTransform, sourceNormal, sourceVelocity, true);
				}
				else
				{
					namedVFXPool.vfxPool.SpawnAtPosition(base.specRigidbody.UnitCenter, 0f, base.transform, sourceNormal, sourceVelocity, null, true);
				}
			}
		}
	}

	public void StopVfx(string name)
	{
		for (int i = 0; i < OtherVfx.Count; i++)
		{
			AIAnimator.NamedVFXPool namedVFXPool = OtherVfx[i];
			if (namedVFXPool.name == name)
			{
				namedVFXPool.vfxPool.DestroyAll();
			}
		}
	}
}
