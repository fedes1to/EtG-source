using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossFinalRogueDeathController : BraveBehaviour
{
	public List<GameObject> explosionVfx;

	public float explosionMidDelay = 0.3f;

	public int explosionCount = 10;

	[Space(12f)]
	public List<GameObject> bigExplosionVfx;

	public float bigExplosionMidDelay = 0.3f;

	public int bigExplosionCount = 10;

	public GameObject DeathStarExplosionVFX;

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
		base.behaviorSpeculator.enabled = false;
		base.aiActor.BehaviorOverridesVelocity = true;
		base.aiActor.BehaviorVelocity = Vector2.zero;
		base.aiAnimator.PlayUntilCancelled("die");
		StartCoroutine(Drift());
		StartCoroutine(OnDeathExplosionsCR());
	}

	private IEnumerator Drift()
	{
		BossFinalRogueController bossController = GetComponent<BossFinalRogueController>();
		Vector2 initialLockPos = bossController.CameraPos;
		bossController.EndCameraLock();
		while ((bool)base.gameObject)
		{
			GameManager.Instance.MainCameraController.OverridePosition = initialLockPos;
			base.transform.position = base.transform.position + new Vector3(1f, -1f, 0f) * BraveTime.DeltaTime;
			base.specRigidbody.Reinitialize();
			yield return null;
		}
	}

	private IEnumerator OnDeathExplosionsCR()
	{
		yield return null;
		BossKillCam extantCam = Object.FindObjectOfType<BossKillCam>();
		if ((bool)extantCam)
		{
			extantCam.ForceCancelSequence();
		}
		GameManager.Instance.MainCameraController.DoContinuousScreenShake(new ScreenShakeSettings(2f, 20f, 1f, 0f, Vector2.right), this);
		for (int k = 0; k < GameManager.Instance.AllPlayers.Length; k++)
		{
			GameManager.Instance.AllPlayers[k].SetInputOverride("past");
		}
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
			else if (DeathStarExplosionVFX != null)
			{
				GameObject deathStarObj = SpawnManager.SpawnVFX(DeathStarExplosionVFX, collider.UnitCenter, Quaternion.identity);
				tk2dBaseSprite deathStarSprite = deathStarObj.GetComponent<tk2dBaseSprite>();
				deathStarSprite.HeightOffGround = 3f;
				base.sprite.AttachRenderer(deathStarSprite);
				base.sprite.UpdateZDepth();
				AkSoundEngine.PostEvent("Play_BOSS_queenship_explode_01", base.gameObject);
				base.sprite.renderer.enabled = false;
				for (int l = 0; l < base.healthHaver.bodySprites.Count; l++)
				{
					if ((bool)base.healthHaver.bodySprites[l])
					{
						base.healthHaver.bodySprites[l].renderer.enabled = false;
					}
				}
				yield return new WaitForSeconds(1f);
				Pixelator.Instance.FadeToColor(2f, Color.white);
				yield return new WaitForSeconds(2f);
				Pixelator.Instance.FadeToColor(2f, Color.white, true, 1f);
			}
			else
			{
				Pixelator.Instance.FadeToColor(3f, Color.white, true, 1f);
			}
		}
		GameManager.Instance.MainCameraController.StopContinuousScreenShake(this);
		base.healthHaver.DeathAnimationComplete(null, null);
		Object.Destroy(base.gameObject);
		PilotPastController ppc = Object.FindObjectOfType<PilotPastController>();
		GameManager.Instance.MainCameraController.SetManualControl(false);
		ppc.OnBossKilled();
	}
}
