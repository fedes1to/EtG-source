using System.Collections;
using Dungeonator;
using UnityEngine;

public class PastLabMarineController : MonoBehaviour
{
	public tk2dSpriteAnimator LeftRedMarine;

	public tk2dSpriteAnimator LeftGreenMarine;

	public tk2dSpriteAnimator RightRedMarine;

	public tk2dSpriteAnimator RightGreenMarine;

	public Transform LeftRedMarineShootPoint;

	public Transform RightRedMarineShootPoint;

	public DungeonDoorController AreaDoor;

	public DungeonDoorController CellDoor;

	public Transform[] SpeakPoints;

	public TalkDoerLite VictoryTalkDoer;

	public SpeculativeRigidbody[] BossCollisionExceptions;

	public ScreenShakeSettings AmbientScreenShakeSettings;

	public float MinTimeBetweenAmbientShakes = 3f;

	public float MaxTimeBetweenAmbientShakes = 5f;

	public tk2dSpriteAnimator TerrorPortal;

	private bool m_inCombat;

	private bool m_occupied;

	private int m_idleCounter;

	private AIBulletBank m_bulletBank;

	private bool m_hasRemarkedOnDoorway;

	private bool m_forceSkip;

	public void Engage()
	{
		m_bulletBank = GetComponent<AIBulletBank>();
		m_inCombat = true;
		AreaDoor.DoSeal(GameManager.Instance.Dungeon.data.Entrance);
		StartCoroutine(HandlePortal());
		HealthHaver healthHaver = StaticReferenceManager.AllHealthHavers.Find((HealthHaver h) => h.IsBoss);
		healthHaver.GetComponent<ObjectVisibilityManager>().ChangeToVisibility(RoomHandler.VisibilityStatus.CURRENT);
		healthHaver.GetComponent<GenericIntroDoer>().TriggerSequence(GameManager.Instance.PrimaryPlayer);
		for (int i = 0; i < BossCollisionExceptions.Length; i++)
		{
			healthHaver.specRigidbody.RegisterSpecificCollisionException(BossCollisionExceptions[i]);
		}
		StartCoroutine(HandleDialogue("#PRIMERDYNE_MARINE_ENTRY_01"));
	}

	private IEnumerator HandlePortal()
	{
		float ela2 = 0f;
		float dura2 = 0.5f;
		while (ela2 < dura2)
		{
			ela2 += GameManager.INVARIANT_DELTA_TIME;
			yield return null;
		}
		TerrorPortal.gameObject.SetActive(true);
		TerrorPortal.ignoreTimeScale = true;
		(TerrorPortal.Sprite as tk2dSprite).GenerateUV2 = true;
		TerrorPortal.sprite.usesOverrideMaterial = true;
		ela2 = 0f;
		dura2 = 5f;
		Vector4 localTime = new Vector4(0f, 0f, 0f, 0f);
		while (ela2 < dura2)
		{
			float ivdt = GameManager.INVARIANT_DELTA_TIME;
			ela2 += ivdt;
			localTime += new Vector4(ivdt / 20f, ivdt, ivdt * 2f, ivdt * 3f);
			TerrorPortal.sprite.renderer.material.SetVector("_LocalTime", localTime);
			yield return null;
		}
		TerrorPortal.PlayAndDisableObject("portal_out");
	}

	private IEnumerator Start()
	{
		while (Dungeon.IsGenerating)
		{
			yield return null;
		}
		StartCoroutine(HandleInitialRoomLockdown());
		Pixelator.Instance.TriggerPastFadeIn();
		float shakeTimer = Random.Range(MinTimeBetweenAmbientShakes, MaxTimeBetweenAmbientShakes);
		while (GameManager.Instance.PrimaryPlayer.CenterPosition.y < LeftRedMarine.transform.position.y)
		{
			shakeTimer -= BraveTime.DeltaTime;
			if (shakeTimer <= 0f)
			{
				GameManager.Instance.MainCameraController.DoScreenShake(AmbientScreenShakeSettings, null);
				shakeTimer += Random.Range(MinTimeBetweenAmbientShakes, MaxTimeBetweenAmbientShakes);
			}
			yield return null;
		}
		Engage();
	}

	private IEnumerator HandleInitialRoomLockdown()
	{
		CellDoor.SetSealedSilently(true);
		yield return new WaitForSeconds(5f);
		CellDoor.SetSealedSilently(false);
		CellDoor.Open();
	}

	private void Update()
	{
		m_forceSkip = false;
		bool flag = BraveInput.GetInstanceForPlayer(0).WasAdvanceDialoguePressed();
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			flag |= BraveInput.GetInstanceForPlayer(1).WasAdvanceDialoguePressed();
		}
		if (flag)
		{
			m_forceSkip = true;
		}
		if (m_inCombat && !m_occupied)
		{
			if (Random.value > 0.5f && m_idleCounter < 2)
			{
				StartCoroutine(HandleHide());
			}
			else
			{
				StartCoroutine(HandleShoot());
			}
		}
		if (!m_hasRemarkedOnDoorway && GameManager.Instance.PrimaryPlayer.transform.position.x > 70f)
		{
			m_hasRemarkedOnDoorway = true;
			MakeSoldierTalkAmbient("#PRIMERDYNE_MARINE_CANT_LEAVE", 5f, true);
		}
	}

	private IEnumerator HandleHide()
	{
		m_idleCounter++;
		m_occupied = true;
		LeftGreenMarine.Play("marine_green_left");
		RightGreenMarine.Play("marine_green_right");
		yield return new WaitForSeconds(Random.Range(2f, 3f));
		m_occupied = false;
	}

	private IEnumerator HandleShoot()
	{
		bool rightMarineFiring = Random.value > 0.33f;
		bool leftMarineFiring = Random.value <= 0.33f || Random.value > 0.66f;
		m_idleCounter = 0;
		m_occupied = true;
		LeftGreenMarine.Play("marine_green_left");
		RightGreenMarine.Play("marine_green_right");
		if (leftMarineFiring)
		{
			LeftRedMarine.PlayForDuration("marine_red_left_fire", -1f, "marine_red_left");
		}
		if (rightMarineFiring)
		{
			RightRedMarine.PlayForDuration("marine_red_right_fire", -1f, "marine_red_right");
		}
		yield return new WaitForSeconds(0.15f);
		while (LeftRedMarine.IsPlaying("marine_red_left_fire") && LeftRedMarine.CurrentFrame < 28)
		{
			if (leftMarineFiring)
			{
				FireBullet(LeftRedMarineShootPoint, new Vector2(1f, 1.2f));
			}
			if (rightMarineFiring)
			{
				FireBullet(RightRedMarineShootPoint, new Vector2(-1f, 1.1f));
			}
			yield return new WaitForSeconds(0.15f);
		}
		yield return new WaitForSeconds(Random.Range(0.25f, 1f));
		m_occupied = false;
	}

	private void FireBullet(Transform shootPoint, Vector2 dirVec)
	{
		GameObject gameObject = m_bulletBank.CreateProjectileFromBank(shootPoint.position, BraveMathCollege.Atan2Degrees(dirVec.normalized) + Random.Range(-10f, 10f), "default");
		gameObject.GetComponent<Projectile>().collidesWithPlayer = false;
	}

	public void MakeSoldierTalkAmbient(string stringKey, float duration = 3f, bool isThoughtBubble = false)
	{
		DoAmbientTalk(GameManager.Instance.PrimaryPlayer.transform, new Vector3(0.75f, 1.5f, 0f), stringKey, duration, isThoughtBubble);
	}

	public void DoAmbientTalk(Transform baseTransform, Vector3 offset, string stringKey, float duration, bool isThoughtBubble = false)
	{
		if (isThoughtBubble)
		{
			Vector3 worldPosition = baseTransform.position + offset;
			float duration2 = -1f;
			string @string = StringTableManager.GetString(stringKey);
			bool instant = false;
			string characterAudioSpeechTag = GameManager.Instance.PrimaryPlayer.characterAudioSpeechTag;
			TextBoxManager.ShowThoughtBubble(worldPosition, baseTransform, duration2, @string, instant, false, characterAudioSpeechTag);
		}
		else
		{
			TextBoxManager.ShowTextBox(baseTransform.position + offset, baseTransform, -1f, StringTableManager.GetString(stringKey), GameManager.Instance.PrimaryPlayer.characterAudioSpeechTag, false);
		}
		StartCoroutine(HandleManualTalkDuration(baseTransform, duration));
	}

	private IEnumerator HandleManualTalkDuration(Transform source, float duration)
	{
		float ela = 0f;
		while (ela < duration)
		{
			ela += BraveTime.DeltaTime;
			yield return null;
			if (m_forceSkip)
			{
				ela += duration;
			}
		}
		TextBoxManager.ClearTextBox(source);
		m_forceSkip = false;
	}

	private IEnumerator HandleDialogue(string stringKey)
	{
		m_occupied = true;
		TextBoxManager.ShowTextBox(SpeakPoints[0].position, SpeakPoints[0], 3f, StringTableManager.GetString(stringKey), string.Empty);
		yield return new WaitForSeconds(3f);
		m_occupied = false;
	}

	public void OnBossKilled()
	{
		m_inCombat = false;
		GameStatsManager.Instance.SetCharacterSpecificFlag(PlayableCharacters.Soldier, CharacterSpecificGungeonFlags.KILLED_PAST, true);
		GameStatsManager.Instance.RegisterStatChange(TrackedStats.TIMES_KILLED_PAST, 1f);
		GameStatsManager.Instance.SetFlag(GungeonFlags.BOSSKILLED_SOLDIER_PAST, true);
		StartCoroutine(HandleBossKilled());
	}

	private IEnumerator HandleBossKilled()
	{
		yield return new WaitForSeconds(3.5f);
		GameManager.Instance.PrimaryPlayer.transform.position = AreaDoor.transform.position + new Vector3(0f, 11f, 0f);
		GameManager.Instance.PrimaryPlayer.specRigidbody.Reinitialize();
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			GameManager.Instance.SecondaryPlayer.transform.position = GameManager.Instance.PrimaryPlayer.transform.position + new Vector3(2.5f, 0f, 0f);
			GameManager.Instance.SecondaryPlayer.specRigidbody.Reinitialize();
		}
		LeftGreenMarine.gameObject.SetActive(false);
		LeftRedMarine.gameObject.SetActive(false);
		RightRedMarine.gameObject.SetActive(false);
		RightGreenMarine.gameObject.SetActive(false);
		VictoryTalkDoer.gameObject.SetActive(true);
		PlayerController m_soldier = GameManager.Instance.PrimaryPlayer;
		PastCameraUtility.LockConversation(m_soldier.CenterPosition);
		GameManager.Instance.MainCameraController.SetManualControl(true, false);
		GameManager.Instance.MainCameraController.OverridePosition = m_soldier.CenterPosition;
		yield return new WaitForSeconds(0.5f);
		GameManager.Instance.MainCameraController.SetManualControl(true, false);
		PastCameraUtility.LockConversation(m_soldier.CenterPosition);
		Pixelator.Instance.FadeToColor(2f, Color.white, true);
		yield return new WaitForSeconds(2f);
		VictoryTalkDoer.Interact(GameManager.Instance.PrimaryPlayer);
		GameManager.Instance.MainCameraController.OverridePosition = m_soldier.CenterPosition;
		while (VictoryTalkDoer.IsTalking)
		{
			yield return null;
		}
		yield return new WaitForSeconds(0.5f);
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
		ttcc.ClearDebris();
		yield return StartCoroutine(ttcc.HandleTimeTubeCredits(m_soldier.sprite.WorldCenter, false, null, -1));
		AmmonomiconController.Instance.OpenAmmonomicon(true, true);
	}
}
