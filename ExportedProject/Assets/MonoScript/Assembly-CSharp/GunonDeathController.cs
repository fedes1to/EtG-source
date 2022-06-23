using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunonDeathController : BraveBehaviour
{
	public List<GameObject> explosionVfx;

	public float explosionMidDelay = 0.3f;

	public int explosionCount = 10;

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
		base.aiAnimator.PlayUntilCancelled("death", true);
		StartCoroutine(HandleBossDeath());
		base.healthHaver.OnPreDeath -= OnBossDeath;
		AkSoundEngine.PostEvent("Play_BOSS_lichB_explode_01", base.gameObject);
	}

	private IEnumerator HandleBossDeath()
	{
		PixelCollider collider = base.specRigidbody.HitboxPixelCollider;
		GameManager.Instance.MainCameraController.DoContinuousScreenShake(new ScreenShakeSettings(2f, 20f, 1f, 0f, Vector2.right), this);
		bool faded = false;
		for (int i = 0; i < explosionCount; i++)
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
			if (!faded && (float)i * explosionMidDelay < 2f)
			{
				Pixelator.Instance.FadeToColor(2f, Color.white);
				faded = true;
			}
			yield return new WaitForSeconds(explosionMidDelay);
		}
		GameManager.Instance.MainCameraController.StopContinuousScreenShake(this);
		BossKillCam extantCam = Object.FindObjectOfType<BossKillCam>();
		if ((bool)extantCam)
		{
			extantCam.ForceCancelSequence();
		}
		PlayerController[] allPlayers = GameManager.Instance.AllPlayers;
		for (int j = 0; j < allPlayers.Length; j++)
		{
			allPlayers[j].CurrentInputState = PlayerInputState.NoInput;
		}
		GameManager.Instance.PrimaryPlayer.IsVisible = false;
		GameManager.Instance.MainCameraController.SetManualControl(true, false);
		GameManager.Instance.MainCameraController.OverridePosition = base.sprite.WorldCenter;
		Pixelator.Instance.FadeToColor(0.5f, Color.white, true);
		base.aiAnimator.PlayUntilCancelled("postdeath");
		base.aiActor.ShadowObject.transform.localPosition += new Vector3(0f, 0.625f, 0f);
		yield return new WaitForSeconds(7.3f);
		Pixelator.Instance.FadeToColor(1f, new Color(0.8f, 0.8f, 0.8f));
		yield return new WaitForSeconds(1f);
		Pixelator.Instance.FadeToColor(0.6f, new Color(0.8f, 0.8f, 0.8f), true);
		yield return new WaitForSeconds(1.6f);
		Pixelator.Instance.FadeToBlack(2f);
		yield return new WaitForSeconds(2f);
		GameManager.Instance.PrimaryPlayer.IsVisible = true;
		BulletPastRoomController[] pastRooms = Object.FindObjectsOfType<BulletPastRoomController>();
		for (int k = 0; k < pastRooms.Length; k++)
		{
			pastRooms[k].TriggerBulletmanEnding();
		}
		base.healthHaver.DeathAnimationComplete(null, null);
	}
}
