using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BunkerBossDeathController : BraveBehaviour
{
	public List<GameObject> explosionVfx;

	public float explosionMidDelay = 0.3f;

	public int explosionCount = 10;

	public List<GameObject> debrisObjects;

	public float debrisMidDelay;

	public int debrisCount;

	public int debrisMinForce = 5;

	public int debrisMaxForce = 5;

	public float debrisAngleVariance = 15f;

	public string deathAnimation;

	public float deathAnimationDelay;

	public List<GameObject> dustVfx;

	public float dustTime = 1f;

	public float dustMidDelay = 0.05f;

	public Vector2 dustOffset;

	public Vector2 dustDimensions;

	public float shakeMidDelay = 0.1f;

	public string flagAnimation;

	public void Start()
	{
		base.healthHaver.ManualDeathHandling = true;
		base.healthHaver.OnPreDeath += OnBossDeath;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	private void OnBossDeath(Vector2 dir)
	{
		StartCoroutine(OnDeathExplosionsCR());
		StartCoroutine(OnDeathDebrisCR());
		StartCoroutine(OnDeathAnimationCR());
	}

	private IEnumerator OnDeathExplosionsCR()
	{
		Vector2 minPos = base.specRigidbody.UnitBottomLeft;
		Vector2 maxPos = base.specRigidbody.UnitTopRight;
		for (int i = 0; i < explosionCount; i++)
		{
			GameObject vfxPrefab = BraveUtility.RandomElement(explosionVfx);
			Vector2 pos = BraveUtility.RandomVector2(minPos, maxPos, new Vector2(1f, 1.5f));
			SpawnManager.SpawnVFX(vfxPrefab, pos, Quaternion.identity);
			yield return new WaitForSeconds(explosionMidDelay);
		}
	}

	private IEnumerator OnDeathDebrisCR()
	{
		Vector2 minPos = base.specRigidbody.UnitBottomLeft;
		Vector2 centerPos = base.specRigidbody.UnitCenter;
		Vector2 maxPos = base.specRigidbody.UnitTopRight;
		for (int i = 0; i < debrisCount; i++)
		{
			GameObject shardPrefab = BraveUtility.RandomElement(debrisObjects);
			Vector2 pos = BraveUtility.RandomVector2(minPos, maxPos, new Vector2(-1.5f, -1.5f));
			GameObject shardObj = SpawnManager.SpawnVFX(shardPrefab, pos, Quaternion.identity);
			if ((bool)shardObj)
			{
				shardObj.transform.parent = SpawnManager.Instance.VFX;
				DebrisObject orAddComponent = shardObj.GetOrAddComponent<DebrisObject>();
				if ((bool)base.aiActor)
				{
					base.aiActor.sprite.AttachRenderer(orAddComponent.sprite);
				}
				orAddComponent.angularVelocity = Mathf.Sign(Random.value - 0.5f) * 125f;
				orAddComponent.angularVelocityVariance = 60f;
				orAddComponent.decayOnBounce = 0.5f;
				orAddComponent.bounceCount = 1;
				orAddComponent.canRotate = true;
				float angle = (pos - centerPos).ToAngle() + Random.Range(0f - debrisAngleVariance, debrisAngleVariance);
				Vector2 vector = BraveMathCollege.DegreesToVector(angle) * Random.Range(debrisMinForce, debrisMaxForce);
				Vector3 startingForce = new Vector3(vector.x, (!(vector.y < 0f)) ? 0f : vector.y, (!(vector.y > 0f)) ? 0f : vector.y);
				if ((bool)orAddComponent.minorBreakable)
				{
					orAddComponent.minorBreakable.enabled = true;
				}
				orAddComponent.Trigger(startingForce, 1f);
			}
			yield return new WaitForSeconds(debrisMidDelay);
		}
	}

	private IEnumerator OnDeathAnimationCR()
	{
		Vector2 minPos = base.specRigidbody.UnitBottomLeft + dustOffset;
		Vector2 maxPos = base.specRigidbody.UnitBottomLeft + dustOffset + dustDimensions;
		yield return new WaitForSeconds(deathAnimationDelay);
		base.aiAnimator.PlayUntilFinished(deathAnimation);
		float timer = dustTime;
		float intraTimer = 0f;
		float shakeTimer = 0f;
		IntVector2 shakeDir = IntVector2.Right;
		while (timer > 0f)
		{
			for (; intraTimer <= 0f; intraTimer += dustMidDelay)
			{
				GameObject prefab = BraveUtility.RandomElement(dustVfx);
				Vector2 vector = BraveUtility.RandomVector2(minPos, maxPos);
				GameObject gameObject = SpawnManager.SpawnVFX(prefab, vector, Quaternion.identity);
				tk2dBaseSprite component = gameObject.GetComponent<tk2dBaseSprite>();
				if ((bool)component)
				{
					base.sprite.AttachRenderer(component);
					component.HeightOffGround = 0.1f;
					component.UpdateZDepth();
				}
			}
			while (shakeTimer <= 0f)
			{
				base.transform.position += (Vector3)PhysicsEngine.PixelToUnit(shakeDir);
				shakeDir *= -1;
				shakeTimer += shakeMidDelay;
				if (shakeTimer > 0f)
				{
					base.specRigidbody.Reinitialize();
				}
			}
			yield return null;
			timer -= BraveTime.DeltaTime;
			intraTimer -= BraveTime.DeltaTime;
			shakeTimer -= BraveTime.DeltaTime;
		}
		if (shakeDir.x < 0)
		{
			base.transform.position += (Vector3)PhysicsEngine.PixelToUnit(shakeDir);
		}
		base.aiAnimator.PlayUntilFinished(flagAnimation);
		base.aiActor.StealthDeath = true;
		base.healthHaver.persistsOnDeath = true;
		base.healthHaver.DeathAnimationComplete(null, null);
		base.specRigidbody.PixelColliders.RemoveAt(1);
		base.specRigidbody.PixelColliders[0].ManualHeight -= 22;
		base.specRigidbody.RegenerateColliders = true;
		base.specRigidbody.Reinitialize();
		if ((bool)base.aiActor)
		{
			Object.Destroy(base.aiActor);
		}
		if ((bool)base.healthHaver)
		{
			Object.Destroy(base.healthHaver);
		}
		if ((bool)base.behaviorSpeculator)
		{
			Object.Destroy(base.behaviorSpeculator);
		}
		RegenerateCache();
	}
}
