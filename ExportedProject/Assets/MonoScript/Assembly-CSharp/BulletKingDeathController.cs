using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletKingDeathController : BraveBehaviour
{
	public List<GameObject> explosionVfx;

	public float explosionMidDelay = 0.3f;

	public int explosionCount = 10;

	public GameObject bigExplosionVfx;

	public float throneFallDelay = 1f;

	public GameObject thronePrefab;

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
		GameObject spawnedExplosion = SpawnManager.SpawnVFX(bigExplosionVfx, collider.UnitCenter, Quaternion.identity);
		tk2dBaseSprite explosionSprite = spawnedExplosion.GetComponent<tk2dSprite>();
		explosionSprite.HeightOffGround = 0.8f;
		base.sprite.AttachRenderer(explosionSprite);
		base.sprite.UpdateZDepth();
		base.aiAnimator.ChildAnimator.gameObject.SetActive(false);
		yield return new WaitForSeconds(throneFallDelay);
		base.aiAnimator.PlayUntilFinished("throne_fall");
		while (base.aiAnimator.IsPlaying("throne_fall"))
		{
			yield return null;
		}
		AdditionalBraveLight[] lights = GetComponentsInChildren<AdditionalBraveLight>();
		foreach (AdditionalBraveLight additionalBraveLight in lights)
		{
			GameObject gameObject = new GameObject("bullet king light");
			gameObject.transform.position = base.sprite.WorldCenter;
			AdditionalBraveLight additionalBraveLight2 = gameObject.AddComponent<AdditionalBraveLight>();
			additionalBraveLight2.LightColor = additionalBraveLight.LightColor;
			additionalBraveLight2.LightIntensity = additionalBraveLight.LightIntensity;
			additionalBraveLight2.LightRadius = additionalBraveLight.LightRadius;
			additionalBraveLight2.Initialize();
		}
		GameObject throne = Object.Instantiate(thronePrefab, base.transform.position, Quaternion.identity);
		PhysicsEngine.Instance.RegisterOverlappingGhostCollisionExceptions(throne.GetComponent<SpeculativeRigidbody>());
		base.aiActor.StealthDeath = true;
		base.healthHaver.persistsOnDeath = true;
		base.healthHaver.DeathAnimationComplete(null, null);
		Object.Destroy(base.gameObject);
	}
}
