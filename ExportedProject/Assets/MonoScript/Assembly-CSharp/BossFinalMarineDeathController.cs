using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossFinalMarineDeathController : BraveBehaviour
{
	public List<GameObject> explosionVfx;

	public float explosionMidDelay = 0.3f;

	public int explosionCount = 10;

	[Space(12f)]
	public List<GameObject> bigExplosionVfx;

	public float bigExplosionMidDelay = 0.3f;

	public int bigExplosionCount = 10;

	public void Start()
	{
		base.healthHaver.ManualDeathHandling = true;
		base.healthHaver.OnPreDeath += OnBossDeath;
		base.healthHaver.OverrideKillCamTime = 5f;
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
		base.aiAnimator.PlayUntilCancelled("die");
		GameManager.Instance.Dungeon.StartCoroutine(OnDeathExplosionsCR());
	}

	private IEnumerator OnDeathExplosionsCR()
	{
		PastLabMarineController plmc = Object.FindObjectOfType<PastLabMarineController>();
		PixelCollider collider = base.specRigidbody.HitboxPixelCollider;
		for (int j = 0; j < explosionCount; j++)
		{
			Vector2 minPos = collider.UnitBottomLeft;
			Vector2 maxPos = collider.UnitTopRight;
			GameObject vfxPrefab = BraveUtility.RandomElement(explosionVfx);
			Vector2 pos = BraveUtility.RandomVector2(minPos, maxPos, new Vector2(0.5f, 0.5f));
			GameObject vfxObj = SpawnManager.SpawnVFX(vfxPrefab, pos, Quaternion.identity);
			tk2dBaseSprite vfxSprite = vfxObj.GetComponent<tk2dBaseSprite>();
			vfxSprite.HeightOffGround = 3f;
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
			vfxSprite2.HeightOffGround = 3f;
			base.sprite.AttachRenderer(vfxSprite2);
			base.sprite.UpdateZDepth();
			if (i < bigExplosionCount - 1)
			{
				yield return new WaitForSeconds(bigExplosionMidDelay);
			}
		}
		base.healthHaver.DeathAnimationComplete(null, null);
		Object.Destroy(base.gameObject);
		yield return new WaitForSeconds(2f);
		Pixelator.Instance.FadeToColor(2f, Color.white);
		plmc.OnBossKilled();
	}
}
