using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class PilotPastController : MonoBehaviour
{
	public bool InstantBossFight;

	public TalkDoerLite FriendTalker;

	public TalkDoerLite HegemonyShip;

	public tk2dSprite[] AdditionalHegemonyShips;

	public GameObject FloatingCrap;

	public tk2dSprite TheRock;

	public Renderer Quad;

	public Vector2 BackgroundScrollSpeed;

	private PlayerController m_pilot;

	private PlayerController m_coop;

	private bool m_hasTriggeredBoss;

	private Vector2 m_backgroundOffset;

	private int m_scrollPositionXId = -1;

	private int m_scrollPositionYId = -1;

	private IEnumerator Start()
	{
		while (Dungeon.IsGenerating)
		{
			yield return null;
		}
		GameUIRoot.Instance.ToggleUICamera(false);
		HegemonyShip.renderer.enabled = false;
		for (int i = 0; i < AdditionalHegemonyShips.Length; i++)
		{
			AdditionalHegemonyShips[i].renderer.enabled = false;
		}
		if (InstantBossFight)
		{
			PlayerController primaryPlayer = GameManager.Instance.PrimaryPlayer;
			HegemonyShip.specRigidbody.enabled = false;
			HegemonyShip.gameObject.SetActive(false);
			FloatingCrap.SetActive(false);
			FriendTalker.gameObject.SetActive(false);
			TheRock.gameObject.SetActive(false);
			List<HealthHaver> allHealthHavers = StaticReferenceManager.AllHealthHavers;
			for (int j = 0; j < allHealthHavers.Count; j++)
			{
				if (allHealthHavers[j].IsBoss)
				{
					allHealthHavers[j].GetComponent<ObjectVisibilityManager>().ChangeToVisibility(RoomHandler.VisibilityStatus.CURRENT);
					allHealthHavers[j].GetComponent<GenericIntroDoer>().TriggerSequence(primaryPlayer);
					allHealthHavers[j].GetComponent<BossFinalRogueController>();
				}
			}
			yield break;
		}
		Pixelator.Instance.TriggerPastFadeIn();
		yield return null;
		SpriteOutlineManager.ToggleOutlineRenderers(HegemonyShip.sprite, false);
		m_pilot = GameManager.Instance.PrimaryPlayer;
		m_pilot.sprite.UpdateZDepth();
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			m_coop = GameManager.Instance.SecondaryPlayer;
			m_coop.transform.position += new Vector3(3f, -2f, 0f);
			m_coop.specRigidbody.Reinitialize();
			m_coop.sprite.UpdateZDepth();
		}
		m_pilot.SetInputOverride("past");
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			m_coop.SetInputOverride("past");
		}
		yield return new WaitForSeconds(0.4f);
		Pixelator.Instance.SetOcclusionDirty();
		PastCameraUtility.LockConversation((FriendTalker.speakPoint.transform.position.XY() + m_pilot.CenterPosition) / 2f);
		yield return null;
		Pixelator.Instance.SetOcclusionDirty();
		yield return new WaitForSeconds(5.6f);
		GameUIRoot.Instance.ToggleUICamera(true);
		FriendTalker.Interact(m_pilot);
		while (FriendTalker.IsTalking)
		{
			yield return null;
		}
		FriendTalker.spriteAnimator.Play();
		yield return new WaitForSeconds(0.5f);
		GameManager.Instance.MainCameraController.OverrideZoomScale = 0.5f;
		GameManager.Instance.MainCameraController.UpdateOverridePosition(GameManager.Instance.MainCameraController.OverridePosition + new Vector3(0f, 8.5f, 0f), 3f);
		ScreenShakeSettings arrivalSS = new ScreenShakeSettings(0.5f, 12f, 2f, 0f);
		GameManager.Instance.MainCameraController.DoContinuousScreenShake(arrivalSS, this);
		yield return new WaitForSeconds(2f);
		for (int k = 0; k < AdditionalHegemonyShips.Length; k++)
		{
			StartCoroutine(ArriveFromWarp(AdditionalHegemonyShips[k], Random.Range(0.75f, 1.25f)));
		}
		yield return StartCoroutine(ArriveFromWarp(HegemonyShip.sprite, 1f));
		GameManager.Instance.MainCameraController.StopContinuousScreenShake(this);
		yield return new WaitForSeconds(1f);
		float elapsed = 0f;
		Tribool hasClamped = Tribool.Unready;
		HegemonyShip.Interact(m_pilot);
		while (HegemonyShip.IsTalking)
		{
			elapsed += BraveTime.DeltaTime;
			if (elapsed > 1f)
			{
				if (hasClamped == Tribool.Unready)
				{
					if (FriendTalker.spriteAnimator.CurrentFrame == 4)
					{
						TheRock.spriteAnimator.Stop();
						TheRock.transform.parent = FriendTalker.GetComponent<tk2dSpriteAttachPoint>().attachPoints[0];
						TheRock.PlaceAtLocalPositionByAnchor(Vector3.zero, tk2dBaseSprite.Anchor.LowerCenter);
						++hasClamped;
					}
				}
				else if (hasClamped == Tribool.Ready && FriendTalker.spriteAnimator.CurrentFrame == 8)
				{
					FriendTalker.spriteAnimator.Stop();
					++hasClamped;
				}
			}
			yield return null;
		}
		GameManager.Instance.MainCameraController.UpdateOverridePosition(GameManager.Instance.MainCameraController.OverridePosition + new Vector3(0f, -8.5f, 0f), 3f);
		yield return new WaitForSeconds(1.5f);
		GameManager.Instance.MainCameraController.OverrideZoomScale = 1f;
		yield return null;
		FriendTalker.Interact(m_pilot);
		while (FriendTalker.IsTalking)
		{
			yield return null;
		}
		PastCameraUtility.UnlockConversation();
		HegemonyShip.specRigidbody.enabled = false;
		HegemonyShip.gameObject.SetActive(false);
		for (int l = 0; l < StaticReferenceManager.AllHealthHavers.Count; l++)
		{
			if (StaticReferenceManager.AllHealthHavers[l].IsBoss)
			{
				StaticReferenceManager.AllHealthHavers[l].GetComponent<ObjectVisibilityManager>().ChangeToVisibility(RoomHandler.VisibilityStatus.CURRENT);
				StaticReferenceManager.AllHealthHavers[l].GetComponent<GenericIntroDoer>().TriggerSequence(GameManager.Instance.PrimaryPlayer);
			}
		}
		ToggleFriendAndJunk(false);
	}

	public void Update()
	{
		Material material = Quad.material;
		if (m_scrollPositionXId < 0)
		{
			m_scrollPositionXId = Shader.PropertyToID("_PositionX");
		}
		if (m_scrollPositionYId < 0)
		{
			m_scrollPositionYId = Shader.PropertyToID("_PositionY");
		}
		m_backgroundOffset += BackgroundScrollSpeed * BraveTime.DeltaTime;
		material.SetFloat(m_scrollPositionXId, m_backgroundOffset.x);
		material.SetFloat(m_scrollPositionYId, m_backgroundOffset.y);
	}

	public void ToggleFriendAndJunk(bool state)
	{
		StartCoroutine(HandleFriendAndJunkToggle(state));
	}

	private IEnumerator HandleFriendAndJunkToggle(bool state)
	{
		float elapsed = 0f;
		tk2dBaseSprite[] crapSprites = FloatingCrap.GetComponentsInChildren<tk2dBaseSprite>(true);
		if (state)
		{
			FriendTalker.renderer.enabled = true;
			TheRock.renderer.enabled = true;
			SpriteOutlineManager.ToggleOutlineRenderers(FriendTalker.sprite, true);
			for (int i = 0; i < crapSprites.Length; i++)
			{
				crapSprites[i].renderer.enabled = true;
			}
		}
		FriendTalker.specRigidbody.enabled = state;
		while (elapsed < 2f)
		{
			elapsed += GameManager.INVARIANT_DELTA_TIME;
			float t = elapsed / 2f;
			if (state)
			{
				t = 1f - t;
			}
			Color targetcolor = Color.Lerp(Color.white, new Color(0.2f, 0.2f, 0.2f), t);
			FriendTalker.sprite.color = targetcolor;
			for (int j = 0; j < crapSprites.Length; j++)
			{
				crapSprites[j].color = targetcolor;
			}
			TheRock.sprite.color = targetcolor;
			yield return null;
		}
		if (!state)
		{
			FriendTalker.renderer.enabled = false;
			TheRock.renderer.enabled = false;
			SpriteOutlineManager.ToggleOutlineRenderers(FriendTalker.sprite, false);
			for (int k = 0; k < crapSprites.Length; k++)
			{
				crapSprites[k].renderer.enabled = false;
			}
		}
	}

	public IEnumerator EndPastSuccess()
	{
		GameStatsManager.Instance.SetCharacterSpecificFlag(PlayableCharacters.Pilot, CharacterSpecificGungeonFlags.KILLED_PAST, true);
		TimeTubeCreditsController ttcc = new TimeTubeCreditsController();
		ttcc.ClearDebris();
		yield return StartCoroutine(ttcc.HandleTimeTubeCredits(GameManager.Instance.PrimaryPlayer.sprite.WorldCenter, false, null, -1));
		AmmonomiconController.Instance.OpenAmmonomicon(true, true);
	}

	private void SetupCutscene()
	{
		PastCameraUtility.LockConversation(m_pilot.CenterPosition);
	}

	private void HandleBossCutscene()
	{
		StartCoroutine(BossCutscene_CR());
	}

	private IEnumerator BossCutscene_CR()
	{
		yield return null;
		TriggerBoss();
	}

	private IEnumerator ArriveFromWarp(tk2dBaseSprite targetSprite, float duration)
	{
		AkSoundEngine.PostEvent("Play_BOSS_queenship_emerge_01", base.gameObject);
		Transform targetTransform = targetSprite.transform;
		targetSprite.renderer.enabled = true;
		float width = (Quaternion.Euler(0f, 0f, -1f * targetTransform.rotation.eulerAngles.z) * targetSprite.GetBounds().size).x;
		SpriteOutlineManager.ToggleOutlineRenderers(targetSprite, true);
		Vector3 targetPosition = targetTransform.position;
		float elapsed = 0f;
		while (elapsed < duration)
		{
			elapsed += BraveTime.DeltaTime;
			float t = BraveMathCollege.LinearToSmoothStepInterpolate(0f, 1f, elapsed / duration, 1);
			targetTransform.position = targetTransform.position.WithX(targetPosition.x + Mathf.Lerp(width / 2f, 0f, t)).WithY(Mathf.Lerp(targetPosition.y + 60f, targetPosition.y, t));
			targetTransform.localScale = Quaternion.Euler(0f, 0f, -1f * targetTransform.rotation.eulerAngles.z) * Vector3.Lerp(new Vector3(0.1f, 10f, 1f), Vector3.one, t);
			yield return null;
		}
	}

	public void OnBossKilled()
	{
		if (!m_pilot.gameObject.activeSelf)
		{
			m_pilot.ResurrectFromBossKill();
		}
		if ((bool)m_coop && !m_coop.gameObject.activeSelf)
		{
			m_coop.ResurrectFromBossKill();
		}
		StartCoroutine(HandleBossKilled());
	}

	private IEnumerator HandleBossKilled()
	{
		GameStatsManager.Instance.SetCharacterSpecificFlag(PlayableCharacters.Pilot, CharacterSpecificGungeonFlags.KILLED_PAST, true);
		GameStatsManager.Instance.SetFlag(GungeonFlags.BOSSKILLED_ROGUE_PAST, true);
		GameStatsManager.Instance.RegisterStatChange(TrackedStats.TIMES_KILLED_PAST, 1f);
		ToggleFriendAndJunk(true);
		m_pilot.SetInputOverride("past");
		if (m_coop != null)
		{
			m_coop.SetInputOverride("past");
		}
		m_pilot.WarpToPoint(FriendTalker.specRigidbody.UnitCenter + new Vector2(3f, 0f));
		if (m_coop != null)
		{
			m_coop.WarpToPoint(m_pilot.specRigidbody.UnitCenter + new Vector2(3f, 0f));
		}
		FriendTalker.gameObject.SetActive(true);
		while (GameManager.Instance.MainCameraController.ManualControl)
		{
			yield return null;
		}
		GameManager.Instance.MainCameraController.OverrideZoomScale = 1f;
		yield return null;
		PastCameraUtility.LockConversation(m_pilot.CenterPosition);
		GameManager.Instance.MainCameraController.OverridePosition = m_pilot.CenterPosition;
		yield return new WaitForSeconds(1f);
		FriendTalker.Interact(m_pilot);
		GameManager.Instance.MainCameraController.OverridePosition = m_pilot.CenterPosition;
		while (FriendTalker.IsTalking)
		{
			yield return null;
		}
		yield return new WaitForSeconds(1f);
		Pixelator.Instance.FreezeFrame();
		BraveTime.RegisterTimeScaleMultiplier(0f, base.gameObject);
		float ela = 0f;
		while (ela < ConvictPastController.FREEZE_FRAME_DURATION)
		{
			ela += GameManager.INVARIANT_DELTA_TIME;
			yield return null;
		}
		BraveTime.ClearMultiplier(base.gameObject);
		TimeTubeCreditsController ttcc = new TimeTubeCreditsController();
		Pixelator.Instance.FadeToColor(0.15f, Color.white, true, 0.15f);
		yield return StartCoroutine(ttcc.HandleTimeTubeCredits(GameManager.Instance.PrimaryPlayer.sprite.WorldCenter, false, null, -1));
		AmmonomiconController.Instance.OpenAmmonomicon(true, true);
	}

	private HealthHaver GetBoss()
	{
		List<HealthHaver> allHealthHavers = StaticReferenceManager.AllHealthHavers;
		for (int i = 0; i < allHealthHavers.Count; i++)
		{
			if (allHealthHavers[i].IsBoss)
			{
				GenericIntroDoer component = allHealthHavers[i].GetComponent<GenericIntroDoer>();
				if ((bool)component && component.triggerType == GenericIntroDoer.TriggerType.BossTriggerZone)
				{
					return allHealthHavers[i];
				}
			}
		}
		return null;
	}

	private void TriggerBoss()
	{
		if (m_hasTriggeredBoss)
		{
			return;
		}
		List<HealthHaver> allHealthHavers = StaticReferenceManager.AllHealthHavers;
		for (int i = 0; i < allHealthHavers.Count; i++)
		{
			if (!allHealthHavers[i].IsBoss)
			{
				continue;
			}
			GenericIntroDoer component = allHealthHavers[i].GetComponent<GenericIntroDoer>();
			if ((bool)component && component.triggerType == GenericIntroDoer.TriggerType.BossTriggerZone)
			{
				component.gameObject.SetActive(true);
				ObjectVisibilityManager component2 = component.GetComponent<ObjectVisibilityManager>();
				component2.ChangeToVisibility(RoomHandler.VisibilityStatus.CURRENT);
				if (!SpriteOutlineManager.HasOutline(component.aiAnimator.sprite))
				{
					SpriteOutlineManager.AddOutlineToSprite(component.aiAnimator.sprite, Color.black, 0.1f);
				}
				component.aiAnimator.renderer.enabled = false;
				SpriteOutlineManager.ToggleOutlineRenderers(component.aiAnimator.sprite, false);
				component.aiAnimator.ChildAnimator.renderer.enabled = false;
				SpriteOutlineManager.ToggleOutlineRenderers(component.aiAnimator.ChildAnimator.sprite, false);
				component.TriggerSequence(GameManager.Instance.PrimaryPlayer);
				m_hasTriggeredBoss = true;
				break;
			}
		}
	}
}
