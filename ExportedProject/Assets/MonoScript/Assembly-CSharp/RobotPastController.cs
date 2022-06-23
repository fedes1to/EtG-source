using System;
using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class RobotPastController : MonoBehaviour
{
	public bool InstantBossFight;

	public bool DoWavesOfEnemies;

	public TalkDoerLite WelcomeBot;

	public TalkDoerLite EmperorBot;

	public string[] validPrefixes;

	public string[] directionalAffixes;

	public GameObject RobotPrefab;

	public GameObject WarpVFX;

	public Vector2 outerRectMin;

	public Vector2 outerRectMax;

	public Vector2 innerRectMin;

	public Vector2 innerRectMax;

	public tk2dSprite EmperorSprite;

	public Rect excludedRect;

	[EnemyIdentifier]
	public string[] CritterIds;

	[NonSerialized]
	public List<Vector2> m_cachedPositions = new List<Vector2>();

	private List<List<Vector2>> m_points = new List<List<Vector2>>();

	private List<List<int>> m_ids = new List<List<int>>();

	private List<List<Vector2>> m_activePoints = new List<List<Vector2>>();

	private List<List<int>> m_activeIds = new List<List<int>>();

	private Dictionary<int, tk2dSpriteAnimator> m_extantRobots = new Dictionary<int, tk2dSpriteAnimator>();

	private List<tk2dSpriteAnimator> m_unusedRobots = new List<tk2dSpriteAnimator>();

	private List<List<string>> m_directionalAnimations = new List<List<string>>();

	private List<List<string>> m_directionalOffAnimations = new List<List<string>>();

	private List<Material> m_fadeMaterials = new List<Material>();

	[NonSerialized]
	private bool RobotsOff;

	private List<int> m_offPoints = new List<int>();

	private Vector2 m_centerPoint;

	private void Start()
	{
		RoomHandler entrance = GameManager.Instance.Dungeon.data.Entrance;
		innerRectMin = entrance.area.basePosition.ToVector2() + new Vector2(1f, 3f);
		innerRectMax = innerRectMin + entrance.area.dimensions.ToVector2() + new Vector2(-3f, -8.75f);
		outerRectMin = innerRectMin + new Vector2(-5f, -5f);
		outerRectMax = innerRectMax + new Vector2(5f, 5f);
		excludedRect = new Rect(EmperorSprite.WorldBottomLeft + new Vector2(-0.75f, -0.75f), EmperorSprite.WorldTopRight - EmperorSprite.WorldBottomLeft + new Vector2(1.25f, 1.25f));
		BraveUtility.DrawDebugSquare(innerRectMin, innerRectMax, Color.cyan, 1000f);
		BraveUtility.DrawDebugSquare(outerRectMin, outerRectMax, Color.cyan, 1000f);
		DistributePoints();
		float[] array = new float[5] { 0f, 0.5f, 0.7f, 0.9f, 1f };
		for (int i = 0; i < 5; i++)
		{
			Material material = new Material(RobotPrefab.GetComponent<Renderer>().sharedMaterial);
			material.SetColor("_OverrideColor", new Color(0.05f, 0.05f, 0.05f, array[i]));
			m_fadeMaterials.Add(material);
		}
		RobotPrefab.transform.position = new Vector3(1000f, -100f, -100f);
		if (InstantBossFight)
		{
			PlayerController primaryPlayer = GameManager.Instance.PrimaryPlayer;
			List<HealthHaver> allHealthHavers = StaticReferenceManager.AllHealthHavers;
			for (int j = 0; j < allHealthHavers.Count; j++)
			{
				if (allHealthHavers[j].IsBoss)
				{
					allHealthHavers[j].GetComponent<ObjectVisibilityManager>().ChangeToVisibility(RoomHandler.VisibilityStatus.CURRENT);
					allHealthHavers[j].GetComponent<GenericIntroDoer>().TriggerSequence(primaryPlayer);
				}
			}
		}
		else
		{
			StartCoroutine(HandlePastIntro());
		}
	}

	private TerminatorPanelController HandleTerminatorUIOverlay()
	{
		dfControl dfControl2 = GameUIRoot.Instance.Manager.AddPrefab(ResourceCache.Acquire("Global Prefabs/TerminatorPanel") as GameObject);
		(dfControl2 as dfPanel).Size = GameUIRoot.Instance.Manager.GetScreenSize() * GameUIRoot.Instance.Manager.UIScale;
		TerminatorPanelController component = dfControl2.GetComponent<TerminatorPanelController>();
		StartCoroutine(HandleTerminatorUIOverlay_CR(component));
		return component;
	}

	private IEnumerator HandleTerminatorUIOverlay_CR(TerminatorPanelController tpc)
	{
		float elapsed2 = 0f;
		tpc.Trigger();
		while (elapsed2 < 0.5f)
		{
			elapsed2 += BraveTime.DeltaTime;
			float t = elapsed2 / 0.5f;
			Pixelator.Instance.SetSaturationColorPower(Color.red, t);
			Pixelator.Instance.fade = Mathf.Lerp(1f, 2.5f, (t - 0.5f) * 2f);
			Pixelator.Instance.GetComponent<SENaturalBloomAndDirtyLens>().bloomIntensity = Mathf.Lerp(0.05f, 0.25f, (t - 0.5f) * 2f);
			yield return null;
		}
		while (tpc.IsActive)
		{
			yield return null;
		}
		elapsed2 = 0f;
		float duration = 1.25f;
		while (elapsed2 < duration)
		{
			elapsed2 += BraveTime.DeltaTime;
			float t2 = 1f - elapsed2 / duration;
			Pixelator.Instance.SetSaturationColorPower(Color.red, t2);
			Pixelator.Instance.fade = Mathf.Lerp(1f, 2.5f, t2);
			Pixelator.Instance.GetComponent<SENaturalBloomAndDirtyLens>().bloomIntensity = Mathf.Lerp(0.05f, 0.25f, t2);
			yield return null;
		}
	}

	private IEnumerator HandlePastIntro()
	{
		while (Dungeon.IsGenerating)
		{
			yield return null;
		}
		PlayerController m_robot = GameManager.Instance.PrimaryPlayer;
		PastCameraUtility.LockConversation(m_robot.CenterPosition);
		m_robot.IsVisible = false;
		yield return new WaitForSeconds(2f);
		SpawnManager.SpawnVFX(WarpVFX, m_robot.CenterPosition, Quaternion.identity, true);
		AkSoundEngine.PostEvent("Play_OBJ_chestwarp_use_01", base.gameObject);
		yield return new WaitForSeconds(1f);
		m_robot.IsVisible = true;
		GameManager.Instance.MainCameraController.OverridePosition = m_robot.CenterPosition;
		yield return new WaitForSeconds(1f);
		TerminatorPanelController tpc = HandleTerminatorUIOverlay();
		if ((bool)tpc)
		{
			while (tpc.IsActive)
			{
				yield return null;
			}
		}
		WelcomeBot.Interact(m_robot);
		GameManager.Instance.MainCameraController.OverridePosition = m_robot.CenterPosition;
		m_robot.ForceIdleFacePoint(Vector2.down);
		while (WelcomeBot.IsTalking)
		{
			yield return null;
		}
		PastCameraUtility.UnlockConversation();
		while (m_robot.CenterPosition.y < EmperorBot.transform.position.y - 12f)
		{
			yield return null;
		}
		PastCameraUtility.LockConversation(m_robot.CenterPosition);
		GameManager.Instance.MainCameraController.OverridePosition = EmperorSprite.WorldCenter + new Vector2(0f, -3f);
		yield return null;
		EmperorBot.Interact(m_robot);
		GameManager.Instance.MainCameraController.OverridePosition = EmperorSprite.WorldCenter + new Vector2(0f, -3f);
		m_robot.ForceIdleFacePoint(Vector2.down);
		WelcomeBot.gameObject.SetActive(false);
		WelcomeBot.specRigidbody.enabled = false;
		bool recentered = false;
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			GameManager.Instance.AllPlayers[i].ForceMoveInDirectionUntilThreshold(Vector2.down, GameManager.Instance.AllPlayers[i].CenterPosition.y - 5f, 0.25f);
		}
		while (EmperorBot.IsTalking)
		{
			if (!recentered && EmperorBot.GetDungeonFSM().FsmVariables.GetFsmBool("recenter").Value)
			{
				recentered = true;
				StartCoroutine(LaunchRecenter(m_robot.CenterPosition));
			}
			yield return null;
		}
		PastCameraUtility.UnlockConversation();
		GameManager.Instance.MainCameraController.SetManualControl(true);
		Vector3 sarahSpawnPos = EmperorBot.transform.position + new Vector3(6f, -10f, 0f);
		GameManager.Instance.MainCameraController.OverridePosition = sarahSpawnPos;
		yield return new WaitForSeconds(1f);
		if (DoWavesOfEnemies)
		{
			m_robot.CurrentRoom.TriggerReinforcementLayersOnEvent(RoomEventTriggerCondition.NPC_TRIGGER_A);
			yield return new WaitForSeconds(5f);
			while (m_robot.CurrentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All).Count > 1)
			{
				yield return new WaitForSeconds(1f);
			}
		}
		List<HealthHaver> healthHavers = StaticReferenceManager.AllHealthHavers;
		for (int j = 0; j < healthHavers.Count; j++)
		{
			if (healthHavers[j].IsBoss)
			{
				StartCoroutine(StartBossFight(healthHavers[j], m_robot));
			}
		}
	}

	private IEnumerator StartBossFight(HealthHaver boss, PlayerController m_robot)
	{
		AkSoundEngine.PostEvent("Play_OBJ_chestwarp_use_01", base.gameObject);
		SpawnManager.SpawnVFX(WarpVFX, boss.specRigidbody.UnitCenter, Quaternion.identity, true);
		yield return new WaitForSeconds(0.25f);
		boss.GetComponent<ObjectVisibilityManager>().ChangeToVisibility(RoomHandler.VisibilityStatus.CURRENT);
		yield return new WaitForSeconds(1f);
		boss.healthHaver.OverrideKillCamTime = 5f;
		boss.GetComponent<GenericIntroDoer>().TriggerSequence(m_robot);
	}

	private IEnumerator LaunchRecenter(Vector2 targetPosition)
	{
		float elapsed = 0f;
		float duration = 0.4f;
		Vector2 startPosition = GameManager.Instance.MainCameraController.OverridePosition;
		while (elapsed < duration)
		{
			elapsed += BraveTime.DeltaTime;
			float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
			GameManager.Instance.MainCameraController.OverridePosition = Vector2.Lerp(startPosition, targetPosition, t);
			yield return null;
		}
	}

	public void OnBossKilled(Transform bossTransform)
	{
		StartCoroutine(OnBossKilled_CR(bossTransform));
	}

	private IEnumerator OnBossKilled_CR(Transform bossTransform)
	{
		yield return new WaitForSeconds(2f);
		BossKillCam extantCam = UnityEngine.Object.FindObjectOfType<BossKillCam>();
		if ((bool)extantCam)
		{
			extantCam.ForceCancelSequence();
		}
		GameStatsManager.Instance.SetCharacterSpecificFlag(PlayableCharacters.Robot, CharacterSpecificGungeonFlags.KILLED_PAST, true);
		PlayerController m_robot = GameManager.Instance.PrimaryPlayer;
		PastCameraUtility.LockConversation(m_robot.CenterPosition);
		yield return null;
		EmperorBot.Interact(m_robot);
		GameManager.Instance.MainCameraController.OverridePosition = EmperorSprite.WorldCenter + new Vector2(0f, -3f);
		m_robot.ForceIdleFacePoint(Vector2.down);
		while (EmperorBot.IsTalking)
		{
			yield return null;
		}
		TurnRobotsOff();
		yield return new WaitForSeconds(2f);
		Vector2 idealPlayerPos = EmperorBot.transform.position + new Vector3(3.5f, -22f, 0f);
		if (m_robot.transform.position.y < EmperorBot.transform.position.y - 6f)
		{
			m_robot.transform.position = idealPlayerPos;
			m_robot.specRigidbody.Reinitialize();
			DeadlyDeadlyGoopManager.DelayedClearGoopsInRadius(m_robot.specRigidbody.UnitCenter, 1f);
		}
		PastCameraUtility.UnlockConversation();
		GameManager.Instance.MainCameraController.SetManualControl(false, false);
		GameManager.Instance.MainCameraController.SetManualControl(true);
		GameManager.Instance.MainCameraController.OverridePosition = m_robot.specRigidbody.HitboxPixelCollider.UnitCenter;
		PlayerController[] players = GameManager.Instance.AllPlayers;
		for (int i = 0; i < players.Length; i++)
		{
			players[i].CurrentInputState = PlayerInputState.NoInput;
		}
		for (int j = 0; j < 10; j++)
		{
			AIActor orLoadByGuid = EnemyDatabase.GetOrLoadByGuid(CritterIds[UnityEngine.Random.Range(0, CritterIds.Length)]);
			AIActor.Spawn(orLoadByGuid, m_robot.CenterPosition.ToIntVector2(VectorConversions.Floor) + new IntVector2(UnityEngine.Random.Range(-5, 5), UnityEngine.Random.Range(-5, 5)), m_robot.CurrentRoom, true);
		}
		yield return new WaitForSeconds(3f);
		m_robot.QueueSpecificAnimation("select_choose_long");
		yield return new WaitForSeconds(2f);
		Pixelator.Instance.FreezeFrame();
		BraveTime.RegisterTimeScaleMultiplier(0f, base.gameObject);
		float ela = 0f;
		while (ela < ConvictPastController.FREEZE_FRAME_DURATION)
		{
			ela += GameManager.INVARIANT_DELTA_TIME;
			yield return null;
		}
		BraveTime.ClearMultiplier(base.gameObject);
		PastCameraUtility.LockConversation(GameManager.Instance.PrimaryPlayer.CenterPosition);
		TimeTubeCreditsController ttcc = new TimeTubeCreditsController();
		Pixelator.Instance.FadeToColor(0.15f, Color.white, true, 0.15f);
		ttcc.ClearDebris();
		yield return StartCoroutine(ttcc.HandleTimeTubeCredits(GameManager.Instance.PrimaryPlayer.sprite.WorldCenter, false, null, -1));
		AmmonomiconController.Instance.OpenAmmonomicon(true, true);
	}

	private void DistributePoints()
	{
		Vector2 vector = (innerRectMin + innerRectMax) / 2f;
		for (int i = 0; i < validPrefixes.Length; i++)
		{
			m_points.Add(new List<Vector2>());
			m_ids.Add(new List<int>());
			m_activePoints.Add(new List<Vector2>());
			m_activeIds.Add(new List<int>());
			m_directionalAnimations.Add(new List<string>());
			m_directionalOffAnimations.Add(new List<string>());
			for (int j = 0; j < directionalAffixes.Length; j++)
			{
				m_directionalAnimations[i].Add(validPrefixes[i] + directionalAffixes[j]);
				m_directionalOffAnimations[i].Add(validPrefixes[i] + "_off" + directionalAffixes[j]);
			}
		}
		List<Vector2> list = new List<Vector2>();
		for (int k = 0; k < 1500; k++)
		{
			Vector2 normalized = UnityEngine.Random.insideUnitCircle.normalized;
			Vector2 vector2;
			for (vector2 = vector; BraveMathCollege.AABBContains(innerRectMin, innerRectMax, vector2); vector2 += normalized * UnityEngine.Random.Range(2f, 5f))
			{
			}
			if (!excludedRect.Contains(vector2))
			{
				list.Add(vector2);
			}
		}
		for (int l = 0; l < list.Count; l++)
		{
			for (int m = 0; m < list.Count; m++)
			{
				if (l != m)
				{
					float sqrMagnitude = (list[l] - list[m]).sqrMagnitude;
					if (sqrMagnitude < 0.25f)
					{
						list.RemoveAt(m);
						m--;
					}
				}
			}
		}
		for (int n = 0; n < list.Count; n++)
		{
			int index = UnityEngine.Random.Range(0, validPrefixes.Length);
			m_points[index].Add(list[n]);
			m_ids[index].Add(n);
		}
	}

	private tk2dSpriteAnimator GetRobotAtPosition(Vector2 point)
	{
		if (m_unusedRobots.Count > 0)
		{
			tk2dSpriteAnimator tk2dSpriteAnimator2 = m_unusedRobots[0];
			m_unusedRobots.RemoveAt(0);
			tk2dSpriteAnimator2.gameObject.SetActive(true);
			tk2dSpriteAnimator2.transform.position = point;
			return tk2dSpriteAnimator2;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(RobotPrefab, point, Quaternion.identity);
		return gameObject.GetComponent<tk2dSpriteAnimator>();
	}

	public void TurnRobotsOff()
	{
		RobotsOff = true;
		StartCoroutine(TurnRobotsOffCR());
	}

	private IEnumerator TurnRobotsOffCR()
	{
		yield return null;
		EmperorBot.aiAnimator.PlayUntilCancelled("EMP_eye_off");
		EmperorBot.transform.Find("pet").GetComponent<tk2dSpriteAnimator>().Play("EMP_pet_off");
		while (EmperorBot.spriteAnimator.IsPlaying("EMP_eye_off"))
		{
			yield return null;
		}
		EmperorBot.sprite.renderer.enabled = false;
	}

	private void LateUpdate()
	{
		if (!RobotsOff)
		{
			m_centerPoint = GameManager.Instance.PrimaryPlayer.CenterPosition;
		}
		Vector2 vector = GameManager.Instance.MainCameraController.MinVisiblePoint + new Vector2(-2f, -2f);
		Vector2 vector2 = GameManager.Instance.MainCameraController.MaxVisiblePoint + new Vector2(2f, 2f);
		for (int i = 0; i < m_points.Count; i++)
		{
			for (int j = 0; j < m_activePoints[i].Count; j++)
			{
				int num = m_activeIds[i][j];
				Vector2 vector3 = m_activePoints[i][j];
				if (vector.x > vector3.x || vector.y > vector3.y || vector2.x < vector3.x || vector2.y < vector3.y)
				{
					tk2dSpriteAnimator tk2dSpriteAnimator2 = m_extantRobots[num];
					tk2dSpriteAnimator2.gameObject.SetActive(false);
					m_unusedRobots.Add(tk2dSpriteAnimator2);
					m_extantRobots.Remove(num);
					m_activePoints[i].RemoveAt(j);
					m_activeIds[i].RemoveAt(j);
					j--;
				}
				else
				{
					if (Mathf.FloorToInt(Time.realtimeSinceStartup) % m_points.Count != i || Time.frameCount % m_activePoints[i].Count != j)
					{
						continue;
					}
					int index = BraveMathCollege.VectorToSextant(vector3 - m_centerPoint);
					string text = ((!RobotsOff) ? m_directionalAnimations[i][index] : m_directionalOffAnimations[i][index]);
					tk2dSpriteAnimator tk2dSpriteAnimator3 = m_extantRobots[num];
					if (!tk2dSpriteAnimator3.IsPlaying(text) && !m_offPoints.Contains(num))
					{
						if (RobotsOff)
						{
							m_offPoints.Add(num);
						}
						tk2dSpriteAnimator3.Play(text);
					}
				}
			}
			for (int k = 0; k < m_points[i].Count; k++)
			{
				int num2 = m_ids[i][k];
				Vector2 vector4 = m_points[i][k];
				if (m_extantRobots.ContainsKey(num2) || !(vector.x < vector4.x) || !(vector.y < vector4.y) || !(vector2.x > vector4.x) || !(vector2.y > vector4.y))
				{
					continue;
				}
				tk2dSpriteAnimator robotAtPosition = GetRobotAtPosition(vector4);
				m_extantRobots.Add(num2, robotAtPosition);
				m_activePoints[i].Add(vector4);
				m_activeIds[i].Add(num2);
				int index2 = BraveMathCollege.VectorToSextant(vector4 - m_centerPoint);
				if (!m_offPoints.Contains(num2))
				{
					robotAtPosition.Play((!RobotsOff) ? m_directionalAnimations[i][index2] : m_directionalOffAnimations[i][index2]);
					if (RobotsOff)
					{
						m_offPoints.Add(num2);
					}
				}
				else if (!robotAtPosition.IsPlaying(m_directionalOffAnimations[i][index2]))
				{
					robotAtPosition.Stop();
					robotAtPosition.sprite.SetSprite(robotAtPosition.GetClipByName(m_directionalOffAnimations[i][index2]).GetFrame(robotAtPosition.GetClipByName(m_directionalOffAnimations[i][index2]).frames.Length - 1).spriteId);
				}
				float num3 = BraveMathCollege.DistToRectangle(vector4, innerRectMin, innerRectMax - innerRectMin) * 1.5f;
				num3 -= UnityEngine.Random.value;
				int index3 = Mathf.Max(Mathf.Min(Mathf.FloorToInt(num3), 4), 0);
				robotAtPosition.sprite.usesOverrideMaterial = true;
				robotAtPosition.renderer.material = m_fadeMaterials[index3];
				robotAtPosition.sprite.UpdateZDepth();
			}
		}
	}
}
