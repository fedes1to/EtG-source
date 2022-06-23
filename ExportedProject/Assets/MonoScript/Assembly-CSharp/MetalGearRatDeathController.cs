using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MetalGearRatDeathController : BraveBehaviour
{
	public GameObject PunchoutMinigamePrefab;

	public List<GameObject> explosionVfx;

	public float explosionMidDelay = 0.3f;

	public int explosionCount = 10;

	private bool m_challengesSuppressed;

	public void Start()
	{
		base.healthHaver.ManualDeathHandling = true;
		base.healthHaver.OnPreDeath += OnBossDeath;
		base.healthHaver.OverrideKillCamTime = 3.5f;
	}

	protected override void OnDestroy()
	{
		if (ChallengeManager.CHALLENGE_MODE_ACTIVE && m_challengesSuppressed)
		{
			ChallengeManager.Instance.SuppressChallengeStart = false;
			m_challengesSuppressed = false;
		}
		base.OnDestroy();
	}

	private void OnBossDeath(Vector2 dir)
	{
		base.aiAnimator.PlayUntilCancelled("death");
		base.aiAnimator.PlayVfx("death");
		GameManager.Instance.StartCoroutine(OnDeathExplosionsCR());
		GameManager.Instance.StartCoroutine(OnDeathCR());
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
	}

	private IEnumerator OnDeathCR()
	{
		SuperReaperController.PreventShooting = true;
		PlayerController[] allPlayers = GameManager.Instance.AllPlayers;
		foreach (PlayerController playerController in allPlayers)
		{
			playerController.SetInputOverride("metal gear death");
		}
		yield return new WaitForSeconds(2f);
		Pixelator.Instance.FadeToColor(0.75f, Color.white);
		Minimap.Instance.TemporarilyPreventMinimap = true;
		GameUIRoot.Instance.HideCoreUI(string.Empty);
		GameUIRoot.Instance.ToggleLowerPanels(false, false, string.Empty);
		yield return new WaitForSeconds(3f);
		MetalGearRatIntroDoer introDoer = GetComponent<MetalGearRatIntroDoer>();
		introDoer.ModifyCamera(false);
		yield return new WaitForSeconds(0.75f);
		base.aiActor.StealthDeath = true;
		base.healthHaver.persistsOnDeath = true;
		base.healthHaver.DeathAnimationComplete(null, null);
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
		if ((bool)base.aiAnimator.ChildAnimator)
		{
			Object.Destroy(base.aiAnimator.ChildAnimator.gameObject);
		}
		if ((bool)base.aiAnimator)
		{
			Object.Destroy(base.aiAnimator);
		}
		if ((bool)base.specRigidbody)
		{
			Object.Destroy(base.specRigidbody);
		}
		RegenerateCache();
		MetalGearRatRoomController room = Object.FindObjectOfType<MetalGearRatRoomController>();
		room.TransformToDestroyedRoom();
		GameManager.Instance.PrimaryPlayer.WarpToPoint(room.transform.position + new Vector3(17.25f, 14.5f));
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER && (bool)GameManager.Instance.SecondaryPlayer)
		{
			GameManager.Instance.SecondaryPlayer.WarpToPoint(room.transform.position + new Vector3(27.5f, 14.5f));
		}
		base.aiActor.ToggleRenderers(false);
		GameObject punchoutMinigame = Object.Instantiate(PunchoutMinigamePrefab);
		PunchoutController punchoutController = punchoutMinigame.GetComponent<PunchoutController>();
		punchoutController.Init();
		yield return null;
		PlayerController[] allPlayers2 = GameManager.Instance.AllPlayers;
		foreach (PlayerController playerController2 in allPlayers2)
		{
			playerController2.ClearInputOverride("metal gear death");
		}
		Pixelator.Instance.FadeToColor(1f, Color.white, true);
		yield return new WaitForSeconds(1f);
		Minimap.Instance.TemporarilyPreventMinimap = false;
		Object.Destroy(base.gameObject);
	}
}
