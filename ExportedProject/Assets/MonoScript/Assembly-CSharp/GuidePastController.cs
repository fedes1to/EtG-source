using System;
using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using Pathfinding;
using UnityEngine;

public class GuidePastController : MonoBehaviour
{
	public PlayerCommentInteractable TargetInteractable;

	public tk2dSprite ArtifactSprite;

	public TalkDoerLite DrWolfTalkDoer;

	public SpeculativeRigidbody DrWolfEnemyRigidbody;

	public TalkDoerLite PhantomEndTalkDoer;

	public tk2dSprite SpinnyGreenMachinePart;

	private PlayerController m_guide;

	private PlayerController m_coop;

	private AIActor m_dog;

	private Transform m_transform;

	private List<GameObject> m_fakeBullets = new List<GameObject>();

	private bool m_hasTriggeredBoss;

	private bool m_trapActive;

	private bool m_forceSkip;

	private float m_initialTriggerHeight = 8f;

	private bool m_hasTriggeredInitial;

	private float m_antechamberTriggerHeight = 29f;

	private bool m_hasTriggeredAntechamber;

	private IEnumerator Start()
	{
		m_transform = base.transform;
		m_guide = GameManager.Instance.PrimaryPlayer;
		while (Dungeon.IsGenerating)
		{
			yield return null;
		}
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			m_coop = GameManager.Instance.SecondaryPlayer;
		}
		m_dog = ((m_guide.companions.Count <= 0) ? null : m_guide.companions[0]);
		PlayerCommentInteractable targetInteractable = TargetInteractable;
		targetInteractable.OnInteractionBegan = (Action)Delegate.Combine(targetInteractable.OnInteractionBegan, new Action(SetupCutscene));
		PlayerCommentInteractable targetInteractable2 = TargetInteractable;
		targetInteractable2.OnInteractionFinished = (Action)Delegate.Combine(targetInteractable2.OnInteractionFinished, new Action(HandleBossCutscene));
		List<HealthHaver> healthHavers = StaticReferenceManager.AllHealthHavers;
		for (int i = 0; i < healthHavers.Count; i++)
		{
			if (!healthHavers[i].IsBoss && healthHavers[i].name.Contains("DrWolf", true))
			{
				healthHavers[i].specRigidbody.CollideWithOthers = false;
				healthHavers[i].aiActor.IsGone = true;
				break;
			}
		}
		yield return null;
		Pixelator.Instance.TriggerPastFadeIn();
		for (int j = 0; j < StaticReferenceManager.AllEnemies.Count; j++)
		{
			if (StaticReferenceManager.AllEnemies[j].ActorName == "Dr. Wolf")
			{
				DrWolfEnemyRigidbody = StaticReferenceManager.AllEnemies[j].specRigidbody;
			}
		}
		DrWolfEnemyRigidbody.enabled = false;
		GetBoss().gameObject.SetActive(false);
	}

	private void SetupCutscene()
	{
		PastCameraUtility.LockConversation(DrWolfTalkDoer.speakPoint.transform.position.XY() + new Vector2(0f, 15.5f));
		DrWolfTalkDoer.gameObject.SetActive(true);
	}

	private void HandleBossCutscene()
	{
		PlayerCommentInteractable[] array = UnityEngine.Object.FindObjectsOfType<PlayerCommentInteractable>();
		if (array != null)
		{
			for (int i = 0; i < array.Length; i++)
			{
				array[i].ForceDisable();
			}
		}
		StartCoroutine(BossCutscene_CR());
	}

	private IEnumerator BossCutscene_CR()
	{
		Vector2 wolfLastPos = DrWolfTalkDoer.specRigidbody.UnitCenter;
		DrWolfTalkDoer.PathfindToPosition(wolfLastPos + new Vector2(0f, 12f));
		while (DrWolfTalkDoer.CurrentPath != null)
		{
			DrWolfTalkDoer.specRigidbody.Velocity = DrWolfTalkDoer.GetPathVelocityContribution(wolfLastPos, 8);
			wolfLastPos = DrWolfTalkDoer.specRigidbody.UnitCenter;
			yield return null;
		}
		GameManager.Instance.PrimaryPlayer.ForceIdleFacePoint(Vector2.down);
		DrWolfTalkDoer.Interact(GameManager.Instance.PrimaryPlayer);
		while (DrWolfTalkDoer.IsTalking)
		{
			yield return null;
		}
		m_trapActive = true;
		StartCoroutine(HandleBulletTrap());
		StartCoroutine(HandleDogGoingNuts());
		wolfLastPos = DrWolfTalkDoer.specRigidbody.UnitCenter;
		CellValidator cellValidator = (IntVector2 a) => IntVector2.Distance(a, GameManager.Instance.PrimaryPlayer.CenterPosition.ToIntVector2()) > 8f;
		TalkDoerLite drWolfTalkDoer = DrWolfTalkDoer;
		Vector2 targetPosition = wolfLastPos + new Vector2(-7f, 9.75f);
		drWolfTalkDoer.PathfindToPosition(targetPosition, null, cellValidator);
		GameManager.Instance.MainCameraController.UpdateOverridePosition(GameManager.Instance.MainCameraController.OverridePosition + new Vector3(0f, 5f, 0f), 4f);
		while (DrWolfTalkDoer.CurrentPath != null)
		{
			DrWolfTalkDoer.specRigidbody.Velocity = DrWolfTalkDoer.GetPathVelocityContribution(wolfLastPos, 16);
			wolfLastPos = DrWolfTalkDoer.specRigidbody.UnitCenter;
			yield return null;
		}
		DrWolfTalkDoer.Interact(GameManager.Instance.PrimaryPlayer);
		while (DrWolfTalkDoer.IsTalking)
		{
			yield return null;
		}
		GameManager.Instance.PrimaryPlayer.ForceBlank();
		yield return new WaitForSeconds(1f);
		m_trapActive = false;
		DrWolfTalkDoer.Interact(GameManager.Instance.PrimaryPlayer);
		while (DrWolfTalkDoer.IsTalking)
		{
			yield return null;
		}
		PastCameraUtility.UnlockConversation();
		DrWolfTalkDoer.specRigidbody.enabled = false;
		DrWolfTalkDoer.gameObject.SetActive(false);
		TriggerBoss();
	}

	private IEnumerator HandleDogGoingNuts()
	{
		if (m_dog == null)
		{
			m_dog = ((m_guide.companions.Count <= 0) ? null : m_guide.companions[0]);
		}
		m_dog.ClearPath();
		AkSoundEngine.PostEvent("Play_BOSS_energy_shield_01", GameManager.Instance.gameObject);
		AkSoundEngine.PostEvent("Play_PET_dog_bark_02", GameManager.Instance.gameObject);
		Vector2 trapPoint = GameManager.Instance.PrimaryPlayer.CenterPosition;
		int currentTargetPoint = 6;
		m_dog.behaviorSpeculator.InterruptAndDisable();
		float cachedMovementSpeed = m_dog.MovementSpeed;
		m_dog.MovementSpeed = cachedMovementSpeed;
		while (m_trapActive)
		{
			if ((bool)m_dog && m_dog.PathComplete)
			{
				Vector2 targetPosition = trapPoint + (Quaternion.Euler(0f, 0f, 45 * currentTargetPoint) * Vector2.right).XY() * 5.5f;
				m_dog.PathfindToPosition(targetPosition);
				currentTargetPoint = (currentTargetPoint + 1) % 8;
			}
			yield return null;
		}
		m_dog.MovementSpeed = cachedMovementSpeed;
		m_dog.behaviorSpeculator.enabled = true;
	}

	private IEnumerator HandleBulletTrap()
	{
		float elapsed = 0f;
		float duration = 1f;
		Vector2 center = GameManager.Instance.PrimaryPlayer.CenterPosition;
		float lastAngle = 0f;
		GameObject fakeBulletPrefab = (GameObject)BraveResources.Load("Global Prefabs/FakeBullet");
		while (elapsed < duration)
		{
			elapsed += BraveTime.DeltaTime;
			float newAngle = Mathf.Lerp(0f, 360f, elapsed / duration);
			for (int i = Mathf.CeilToInt(lastAngle); (float)i < newAngle; i++)
			{
				if (i % 18 == 0)
				{
					for (int j = 0; j < 3; j++)
					{
						float num = 2 + j;
						GameObject item = UnityEngine.Object.Instantiate(fakeBulletPrefab, center.ToVector3ZUp() + Quaternion.Euler(0f, 0f, i + j * 120) * Vector3.right * num, Quaternion.identity);
						m_fakeBullets.Add(item);
					}
				}
			}
			lastAngle = newAngle;
			yield return null;
		}
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
		DrWolfEnemyRigidbody.enabled = true;
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

	public void OnBossKilled()
	{
		StartCoroutine(HandleBossKilled());
	}

	private IEnumerator HandleBossKilled()
	{
		GameStatsManager.Instance.SetCharacterSpecificFlag(PlayableCharacters.Guide, CharacterSpecificGungeonFlags.KILLED_PAST, true);
		GameStatsManager.Instance.RegisterStatChange(TrackedStats.TIMES_KILLED_PAST, 1f);
		GameStatsManager.Instance.SetFlag(GungeonFlags.BOSSKILLED_GUIDE_PAST, true);
		PhantomEndTalkDoer.gameObject.SetActive(true);
		PhantomEndTalkDoer.ShowOutlines = false;
		SpriteOutlineManager.RemoveOutlineFromSprite(PhantomEndTalkDoer.sprite, true);
		yield return null;
		SpriteOutlineManager.RemoveOutlineFromSprite(PhantomEndTalkDoer.sprite, true);
		while (GameManager.Instance.MainCameraController.ManualControl)
		{
			yield return null;
		}
		m_guide.WarpToPoint(TargetInteractable.specRigidbody.UnitCenter + new Vector2(-0.5f, -2.5f));
		m_guide.ForceIdleFacePoint(Vector2.down);
		if (m_coop != null)
		{
			m_coop.WarpToPoint(m_guide.specRigidbody.UnitCenter + new Vector2(-3f, 0f));
			m_coop.ForceIdleFacePoint(Vector2.down);
		}
		if (m_dog != null)
		{
			m_dog.CompanionWarp(m_guide.CenterPosition.ToVector3ZUp() + new Vector3(1f, -0.5f, 0f));
		}
		yield return null;
		PastCameraUtility.LockConversation(m_guide.CenterPosition);
		GameManager.Instance.MainCameraController.OverridePosition = m_guide.CenterPosition;
		yield return new WaitForSeconds(1f);
		Pixelator.Instance.FadeToColor(1f, Color.white, true);
		yield return new WaitForSeconds(1f);
		PhantomEndTalkDoer.Interact(m_guide);
		GameManager.Instance.MainCameraController.OverridePosition = m_guide.CenterPosition;
		m_guide.ForceIdleFacePoint(Vector2.down);
		Debug.Log("are we talking? " + PhantomEndTalkDoer.IsTalking);
		while (PhantomEndTalkDoer.IsTalking)
		{
			yield return null;
		}
		m_guide.ForceMoveInDirectionUntilThreshold(Vector2.down, m_guide.transform.position.y - 5f, 10f);
		float ela2 = 0f;
		while (ela2 < 0.5f)
		{
			GameManager.Instance.MainCameraController.OverridePosition = m_guide.CenterPosition;
			ela2 += GameManager.INVARIANT_DELTA_TIME;
			yield return null;
		}
		Pixelator.Instance.FreezeFrame();
		BraveTime.RegisterTimeScaleMultiplier(0f, base.gameObject);
		ela2 = 0f;
		while (ela2 < ConvictPastController.FREEZE_FRAME_DURATION)
		{
			ela2 += GameManager.INVARIANT_DELTA_TIME;
			yield return null;
		}
		BraveTime.ClearMultiplier(base.gameObject);
		TimeTubeCreditsController ttcc = new TimeTubeCreditsController();
		ttcc.ClearDebris();
		Pixelator.Instance.FadeToColor(0.15f, Color.white, true, 0.15f);
		yield return StartCoroutine(ttcc.HandleTimeTubeCredits(GameManager.Instance.PrimaryPlayer.sprite.WorldCenter, false, null, -1));
		AmmonomiconController.Instance.OpenAmmonomicon(true, true);
	}

	public void MakeGuideTalkAmbient(string stringKey, float duration = 3f, bool isThoughtBubble = false)
	{
		DoAmbientTalk(m_guide.transform, new Vector3(0.75f, 1.5f, 0f), stringKey, duration, isThoughtBubble, GameManager.Instance.PrimaryPlayer.characterAudioSpeechTag);
	}

	public void MakeDogTalk(string stringKey, float duration = 3f)
	{
		DoAmbientTalk(m_dog.transform, new Vector3(0.25f, 1f, 0f), stringKey, duration, false, string.Empty);
	}

	public void DoAmbientTalk(Transform baseTransform, Vector3 offset, string stringKey, float duration, bool isThoughtBubble = false, string audioTag = "")
	{
		if (isThoughtBubble)
		{
			Vector3 worldPosition = baseTransform.position + offset;
			float duration2 = -1f;
			string @string = StringTableManager.GetString(stringKey);
			bool instant = false;
			TextBoxManager.ShowThoughtBubble(worldPosition, baseTransform, duration2, @string, instant, false, audioTag);
		}
		else
		{
			TextBoxManager.ShowTextBox(baseTransform.position + offset, baseTransform, -1f, StringTableManager.GetString(stringKey), audioTag, false);
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

	private IEnumerator HandleIntroConversation()
	{
		if (m_dog == null)
		{
			m_dog = ((GameManager.Instance.PrimaryPlayer.companions.Count <= 0) ? null : GameManager.Instance.PrimaryPlayer.companions[0]);
		}
		if (m_dog != null)
		{
			MakeDogTalk("#DOG_YIPYIP");
			AkSoundEngine.PostEvent("Play_PET_dog_bark_02", GameManager.Instance.gameObject);
			yield return new WaitForSeconds(1f);
			MakeGuideTalkAmbient("#GUIDEPAST_ZIPIT");
			yield return new WaitForSeconds(3f);
		}
	}

	private IEnumerator HandleAntechamberConversation()
	{
		MakeGuideTalkAmbient("#GUIDEPAST_OMINOUS");
		yield return new WaitForSeconds(2f);
		if (m_dog != null)
		{
			MakeDogTalk("#DOG_YIP");
		}
		AkSoundEngine.PostEvent("Play_PET_dog_bark_02", GameManager.Instance.gameObject);
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
		float num = ((!GameManager.IsBossIntro) ? BraveTime.DeltaTime : GameManager.INVARIANT_DELTA_TIME);
		if ((bool)SpinnyGreenMachinePart && num > 0f)
		{
			int num2 = Mathf.CeilToInt(15f * num);
			GlobalSparksDoer.DoRandomParticleBurst(num2, SpinnyGreenMachinePart.WorldCenter + new Vector2(-0.5f, -0.5f), SpinnyGreenMachinePart.WorldCenter + new Vector2(0.5f, 0.5f), Vector3.up * 3f, 30f, 0.5f, null, null, Color.green);
		}
		if (ArtifactSprite.renderer.enabled)
		{
			GlobalSparksDoer.DoRandomParticleBurst(Mathf.CeilToInt(Mathf.Max(1f, 80f * num)), ArtifactSprite.WorldBottomLeft.ToVector3ZisY(), ArtifactSprite.WorldTopRight.ToVector3ZisY(), Vector3.up, 180f, 0.5f, null, null, null, GlobalSparksDoer.SparksType.BLACK_PHANTOM_SMOKE);
		}
		if (!(Time.timeSinceLevelLoad > 0.5f))
		{
			return;
		}
		if (!m_hasTriggeredInitial)
		{
			if (m_guide.transform.position.y > m_initialTriggerHeight + m_transform.position.y)
			{
				m_hasTriggeredInitial = true;
				StartCoroutine(HandleIntroConversation());
			}
		}
		else if (!m_hasTriggeredAntechamber && m_guide.transform.position.y > m_antechamberTriggerHeight + m_transform.position.y)
		{
			m_hasTriggeredAntechamber = true;
			StartCoroutine(HandleAntechamberConversation());
		}
	}
}
