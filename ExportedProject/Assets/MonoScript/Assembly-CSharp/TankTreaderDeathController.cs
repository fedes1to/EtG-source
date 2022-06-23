using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankTreaderDeathController : BraveBehaviour
{
	public List<GameObject> explosionVfx;

	public float explosionMidDelay = 0.3f;

	public int explosionCount = 10;

	[Space(12f)]
	public List<GameObject> bigExplosionVfx;

	public float bigExplosionMidDelay = 0.3f;

	public int bigExplosionCount = 10;

	[Space(12f)]
	public List<GameObject> debrisObjects;

	public int debrisCount = 10;

	public int debrisMinForce = 5;

	public int debrisMaxForce = 5;

	public int debrisUpForce = 8;

	public float debrisAngleVariance = 15f;

	public ExplosionDebrisLauncher debrisLauncher;

	public void Start()
	{
		base.healthHaver.ManualDeathHandling = true;
		base.healthHaver.OnPreDeath += OnBossDeath;
		base.healthHaver.OverrideKillCamTime = 4.5f;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	private void OnBossDeath(Vector2 dir)
	{
		base.behaviorSpeculator.enabled = false;
		base.aiActor.BehaviorOverridesVelocity = true;
		base.aiActor.BehaviorVelocity = Vector2.zero;
		TankTreaderMiniTurretController[] componentsInChildren = GetComponentsInChildren<TankTreaderMiniTurretController>();
		foreach (TankTreaderMiniTurretController tankTreaderMiniTurretController in componentsInChildren)
		{
			tankTreaderMiniTurretController.enabled = false;
		}
		StartCoroutine(OnDeathExplosionsCR());
	}

	private IEnumerator OnDeathExplosionsCR()
	{
		PixelCollider collider = base.specRigidbody.HitboxPixelCollider;
		for (int j = 0; j < explosionCount; j++)
		{
			Vector2 minPos = collider.UnitBottomLeft;
			Vector2 maxPos = collider.UnitTopRight;
			GameObject vfxPrefab = BraveUtility.RandomElement(explosionVfx);
			Vector2 pos = BraveUtility.RandomVector2(minPos, maxPos, new Vector2(0.5f, 0.5f));
			GameObject vfxObj = SpawnManager.SpawnVFX(vfxPrefab, pos, Quaternion.identity);
			tk2dBaseSprite vfxSprite = vfxObj.GetComponent<tk2dBaseSprite>();
			vfxSprite.HeightOffGround = Random.Range(3f, 4.5f);
			base.sprite.AttachRenderer(vfxSprite);
			base.sprite.UpdateZDepth();
			if (j < explosionCount - 1)
			{
				yield return new WaitForSeconds(explosionMidDelay);
			}
		}
		for (int i = 0; i < bigExplosionCount; i++)
		{
			Vector2 minPos2 = collider.UnitBottomLeft;
			Vector2 maxPos2 = collider.UnitTopRight;
			GameObject vfxPrefab2 = BraveUtility.RandomElement(bigExplosionVfx);
			Vector2 pos2 = BraveUtility.RandomVector2(minPos2, maxPos2, new Vector2(1f, 1f));
			GameObject vfxObj2 = SpawnManager.SpawnVFX(vfxPrefab2, pos2, Quaternion.identity);
			tk2dBaseSprite vfxSprite2 = vfxObj2.GetComponent<tk2dBaseSprite>();
			vfxSprite2.HeightOffGround = Random.Range(3f, 4.5f);
			base.sprite.AttachRenderer(vfxSprite2);
			base.sprite.UpdateZDepth();
			if (i < bigExplosionCount - 1)
			{
				yield return new WaitForSeconds(bigExplosionMidDelay);
			}
		}
		Vector2 unitBottomLeft = collider.UnitBottomLeft;
		Vector2 unitCenter = collider.UnitCenter;
		Vector2 unitTopRight = collider.UnitTopRight;
		for (int k = 0; k < debrisCount; k++)
		{
			Vector2 vector = BraveUtility.RandomVector2(unitBottomLeft, unitTopRight, new Vector2(1f, 1f));
			GameObject gameObject = SpawnManager.SpawnVFX(BraveUtility.RandomElement(debrisObjects), vector, Quaternion.identity);
			if ((bool)gameObject)
			{
				gameObject.transform.parent = SpawnManager.Instance.VFX;
				DebrisObject orAddComponent = gameObject.GetOrAddComponent<DebrisObject>();
				if ((bool)base.aiActor)
				{
					base.aiActor.sprite.AttachRenderer(orAddComponent.sprite);
				}
				orAddComponent.angularVelocity = 0f;
				orAddComponent.angularVelocityVariance = 10f;
				orAddComponent.GravityOverride = 20f;
				orAddComponent.decayOnBounce = 0.5f;
				orAddComponent.bounceCount = 1;
				orAddComponent.canRotate = true;
				float angle = (vector - unitCenter).ToAngle() + Random.Range(0f - debrisAngleVariance, debrisAngleVariance);
				Vector2 vector2 = BraveMathCollege.DegreesToVector(angle) * Random.Range(debrisMinForce, debrisMaxForce);
				Vector3 startingForce = new Vector3(vector2.x, vector2.y, debrisUpForce);
				if ((bool)orAddComponent.minorBreakable)
				{
					orAddComponent.minorBreakable.enabled = true;
				}
				orAddComponent.Trigger(startingForce, 1f);
			}
		}
		if ((bool)debrisLauncher)
		{
			debrisLauncher.Launch();
		}
		base.healthHaver.DeathAnimationComplete(null, null);
		Object.Destroy(base.gameObject);
	}
}
