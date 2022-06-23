using System.Collections;
using Brave.BulletScript;
using Dungeonator;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/DraGun/Roar1")]
public class DraGunRoar1 : Script
{
	public int NumRockets = 3;

	private static int[] s_xValues;

	private static int[] s_yValues;

	protected override IEnumerator Top()
	{
		if (s_xValues == null || s_yValues == null)
		{
			s_xValues = new int[NumRockets];
			s_yValues = new int[NumRockets];
			for (int j = 0; j < NumRockets; j++)
			{
				s_xValues[j] = j;
				s_yValues[j] = j;
			}
		}
		DraGunController dragunController = base.BulletBank.GetComponent<DraGunController>();
		CellArea area = base.BulletBank.aiActor.ParentRoom.area;
		Vector2 roomLowerLeft = area.UnitBottomLeft + new Vector2(2f, 21f);
		Vector2 dimensions = new Vector2(32f, 6f);
		Vector2 delta = new Vector2(dimensions.x / (float)NumRockets, dimensions.y / (float)NumRockets);
		BraveUtility.RandomizeArray(s_xValues);
		BraveUtility.RandomizeArray(s_yValues);
		for (int i = 0; i < NumRockets; i++)
		{
			int baseX = s_xValues[i];
			FireRocket(target: roomLowerLeft + new Vector2(y: ((float)s_yValues[i] + Random.value) * delta.y, x: ((float)baseX + Random.value) * delta.x), skyRocket: dragunController.skyBoulder);
			yield return Wait(10);
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
