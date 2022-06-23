using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GiantPowderSkullDeathController : BraveBehaviour
{
	public List<GameObject> explosionVfx;

	public float explosionMidDelay = 0.3f;

	public int explosionCount = 10;

	public GameObject bigExplosionVfx;

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
		base.aiAnimator.PlayUntilFinished("death", true);
		StartCoroutine(OnDeathExplosionsCR());
		StartCoroutine(HandleParticleSystemsCR());
	}

	private IEnumerator OnDeathExplosionsCR()
	{
		PixelCollider collider = base.specRigidbody.HitboxPixelCollider;
		for (int i = 0; i < explosionCount; i++)
		{
			Vector2 minPos = collider.UnitBottomLeft;
			Vector2 maxPos = collider.UnitTopRight;
			GameObject vfxPrefab = BraveUtility.RandomElement(explosionVfx);
			Vector2 pos = BraveUtility.RandomVector2(minPos, maxPos, new Vector2(0.2f, 0.2f));
			GameObject vfxObj = SpawnManager.SpawnVFX(vfxPrefab, pos, Quaternion.identity);
			tk2dBaseSprite vfxSprite = vfxObj.GetComponent<tk2dBaseSprite>();
			vfxSprite.HeightOffGround = 0.8f;
			base.sprite.AttachRenderer(vfxSprite);
			base.sprite.UpdateZDepth();
			yield return new WaitForSeconds(explosionMidDelay);
		}
		SpawnManager.SpawnVFX(bigExplosionVfx, collider.UnitCenter, Quaternion.identity);
		base.aiActor.StealthDeath = true;
		base.healthHaver.persistsOnDeath = true;
		base.healthHaver.DeathAnimationComplete(null, null);
		base.specRigidbody.enabled = false;
		base.aiActor.ToggleRenderers(false);
		base.renderer.enabled = false;
	}

	private IEnumerator HandleParticleSystemsCR()
	{
		PowderSkullParticleController particleController = base.aiActor.GetComponentInChildren<PowderSkullParticleController>();
		ParticleSystem mainParticleSystem = particleController.GetComponent<ParticleSystem>();
		ParticleSystem trailParticleSystem = particleController.RotationChild.GetComponentInChildren<ParticleSystem>();
		float startRate = mainParticleSystem.emission.rate.constant;
		mainParticleSystem.transform.parent = null;
		BraveUtility.EnableEmission(trailParticleSystem, false);
		float t = 0f;
		float duration = 6f;
		while (t < duration)
		{
			t += BraveTime.DeltaTime;
			BraveUtility.SetEmissionRate(mainParticleSystem, Mathf.Lerp(startRate, 0f, t / duration));
			yield return null;
		}
		Object.Destroy(base.gameObject);
	}
}
