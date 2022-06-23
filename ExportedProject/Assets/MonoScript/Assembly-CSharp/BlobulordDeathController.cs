using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlobulordDeathController : BraveBehaviour
{
	public List<GameObject> explosionVfx;

	public float explosionMidDelay = 0.3f;

	public int explosionCount = 10;

	public float finalScale = 0.1f;

	public GameObject bigExplosionVfx;

	public float crawlerSpawnDelay = 0.3f;

	[EnemyIdentifier]
	public string crawlerGuid;

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
		Vector2 colliderMinPos = collider.UnitBottomLeft;
		Vector2 colliderMaxPos = collider.UnitTopRight;
		Vector2 scalePoint = base.specRigidbody.UnitCenter;
		base.specRigidbody.enabled = false;
		GameObject scaleParent = new GameObject("Blobulord Scaler");
		scaleParent.transform.position = scalePoint;
		base.transform.parent = scaleParent.transform;
		float scale = 1f;
		float totalTime = 0f;
		for (int i = 0; i < explosionCount; i++)
		{
			Vector2 minPos = scalePoint - (scalePoint - colliderMinPos) * scale;
			Vector2 maxPos = scalePoint - (scalePoint - colliderMaxPos) * scale;
			GameObject vfxPrefab = BraveUtility.RandomElement(explosionVfx);
			Vector2 pos = BraveUtility.RandomVector2(minPos, maxPos, new Vector2(0.2f, 0.2f));
			GameObject vfxObj = SpawnManager.SpawnVFX(vfxPrefab, pos, Quaternion.identity);
			tk2dBaseSprite vfxSprite = vfxObj.GetComponent<tk2dBaseSprite>();
			vfxSprite.HeightOffGround = 0.8f;
			base.sprite.AttachRenderer(vfxSprite);
			base.sprite.UpdateZDepth();
			float timer = 0f;
			while (timer < explosionMidDelay)
			{
				yield return null;
				timer += BraveTime.DeltaTime;
				totalTime += BraveTime.DeltaTime;
				scale = BraveMathCollege.QuantizeFloat(Mathf.Lerp(1f, finalScale, totalTime / ((float)explosionCount * explosionMidDelay)), 0.04f);
				scaleParent.transform.localScale = new Vector3(scale, scale, 1f);
			}
		}
		GameObject spawnedExplosion = SpawnManager.SpawnVFX(bigExplosionVfx, scalePoint, Quaternion.identity);
		tk2dBaseSprite explosionSprite = spawnedExplosion.GetComponent<tk2dSprite>();
		explosionSprite.HeightOffGround = 0.8f;
		base.sprite.AttachRenderer(explosionSprite);
		base.sprite.UpdateZDepth();
		base.aiActor.StealthDeath = true;
		base.healthHaver.persistsOnDeath = true;
		base.healthHaver.DeathAnimationComplete(null, null);
		yield return new WaitForSeconds(crawlerSpawnDelay);
		AIActor crawlerPrefab = EnemyDatabase.GetOrLoadByGuid(crawlerGuid);
		AIActor crawler = AIActor.Spawn(crawlerPrefab, base.specRigidbody.UnitCenter.ToIntVector2(VectorConversions.Floor), base.aiActor.ParentRoom);
		if ((bool)crawler)
		{
			crawler.PreventAutoKillOnBossDeath = true;
		}
		Object.Destroy(base.gameObject);
	}
}
