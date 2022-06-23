using System;
using System.Collections;
using Brave.BulletScript;
using Dungeonator;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/DraGun/Grenade1")]
public class DraGunGrenade1 : Script
{
	public int NumRockets = 11;

	public float Magnitude = 4.5f;

	protected override IEnumerator Top()
	{
		bool reverse = BraveUtility.RandomBool();
		StartTask(FireWave(reverse, false, 0f));
		yield return Wait(75);
		StartTask(FireWave(reverse, true, 0.25f));
		yield return Wait(120);
	}

	private IEnumerator FireWave(bool reverse, bool offset, float sinOffset)
	{
		DraGunController dragunController = base.BulletBank.GetComponent<DraGunController>();
		CellArea area = base.BulletBank.aiActor.ParentRoom.area;
		Vector2 start = area.UnitBottomLeft + new Vector2(1f, 25.5f);
		for (int i = 0; i < ((!offset) ? NumRockets : (NumRockets - 1)); i++)
		{
			float t = ((!offset) ? ((float)i) : ((float)i + 0.5f)) / ((float)NumRockets - 1f);
			float dx = 34f * t;
			float dy = Mathf.Sin((t * 2.5f + sinOffset) * (float)Math.PI) * Magnitude;
			if (reverse)
			{
				dx = 34f - dx;
			}
			FireRocket(dragunController.skyRocket, start + new Vector2(dx, dy));
			FireRocket(dragunController.skyRocket, start + new Vector2(dx, 0f - dy));
			if (Mathf.Abs(dy) < 1f)
			{
				FireRocket(dragunController.skyRocket, start + new Vector2(dx, Magnitude));
				FireRocket(dragunController.skyRocket, start + new Vector2(dx, 0f - Magnitude));
			}
			yield return Wait(15);
		}
	}

	private void FireRocket(GameObject skyRocket, Vector2 target)
	{
		SkyRocket component = SpawnManager.SpawnProjectile(skyRocket, base.Position, Quaternion.identity).GetComponent<SkyRocket>();
		component.TargetVector2 = target;
		tk2dSprite componentInChildren = component.GetComponentInChildren<tk2dSprite>();
		component.transform.position = component.transform.position.WithY(component.transform.position.y - componentInChildren.transform.localPosition.y);
		component.ExplosionData.ignoreList.Add(base.BulletBank.specRigidbody);
	}
}
