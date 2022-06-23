using System;
using System.Collections;
using Dungeonator;
using UnityEngine;

public class FloorShockwaveChallengeModifier : ChallengeModifier
{
	public GameObject EyesVFX;

	public float NearRadius = 5f;

	public float FarRadius = 9f;

	public float StoneDuration = 3.5f;

	public float TimeBetweenGaze = 8f;

	[NonSerialized]
	public bool Preprocessed;

	private RoomHandler m_room;

	private float m_waveTimer = 5f;

	private IEnumerator Start()
	{
		m_room = GameManager.Instance.PrimaryPlayer.CurrentRoom;
		yield return null;
		if (!ChallengeManager.Instance)
		{
			yield break;
		}
		for (int i = 0; i < ChallengeManager.Instance.ActiveChallenges.Count; i++)
		{
			if (ChallengeManager.Instance.ActiveChallenges[i] is CircleBurstChallengeModifier)
			{
				float num = TimeBetweenGaze;
				if (!Preprocessed)
				{
					CircleBurstChallengeModifier circleBurstChallengeModifier = ChallengeManager.Instance.ActiveChallenges[i] as CircleBurstChallengeModifier;
					float num2 = Mathf.Max(TimeBetweenGaze, circleBurstChallengeModifier.TimeBetweenWaves);
					num = (circleBurstChallengeModifier.TimeBetweenWaves = (TimeBetweenGaze = num2 * 1.25f));
					Preprocessed = true;
					circleBurstChallengeModifier.Preprocessed = true;
				}
				m_waveTimer = num * 0.25f;
			}
		}
	}

	private void Update()
	{
		m_waveTimer -= BraveTime.DeltaTime;
		if (m_waveTimer <= 0f)
		{
			m_waveTimer = TimeBetweenGaze;
			IntVector2? appropriateSpawnPointForChallengeBurst = CircleBurstChallengeModifier.GetAppropriateSpawnPointForChallengeBurst(m_room, NearRadius, FarRadius);
			if (appropriateSpawnPointForChallengeBurst.HasValue)
			{
				ChallengeManager.Instance.StartCoroutine(LaunchWave(appropriateSpawnPointForChallengeBurst.Value.ToCenterVector2()));
			}
		}
	}

	private IEnumerator LaunchWave(Vector2 startPoint)
	{
		float m_prevWaveDist = 0f;
		float distortionMaxRadius = 20f;
		float distortionDuration = 1.5f;
		float distortionIntensity = 0.5f;
		float distortionThickness = 0.04f;
		GameObject instanceVFX = SpawnManager.SpawnVFX(EyesVFX, startPoint.ToVector3ZUp() + new Vector3(-3.1875f, -3f, 0f), Quaternion.identity);
		tk2dSprite instanceSprite = instanceVFX.GetComponent<tk2dSprite>();
		float elapsedTime = 0f;
		while ((bool)instanceVFX && instanceVFX.activeSelf)
		{
			elapsedTime += BraveTime.DeltaTime;
			if ((bool)instanceSprite)
			{
				instanceSprite.PlaceAtPositionByAnchor(startPoint, tk2dBaseSprite.Anchor.MiddleCenter);
			}
			if (elapsedTime > 0.75f)
			{
				AkSoundEngine.PostEvent("Play_ENM_gorgun_gaze_01", instanceVFX.gameObject);
				elapsedTime -= 1000f;
			}
			yield return null;
		}
		Exploder.DoDistortionWave(startPoint, distortionIntensity, distortionThickness, distortionMaxRadius, distortionDuration);
		float waveRemaining = distortionDuration - BraveTime.DeltaTime;
		while (waveRemaining > 0f)
		{
			waveRemaining -= BraveTime.DeltaTime;
			float waveDist = BraveMathCollege.LinearToSmoothStepInterpolate(0f, distortionMaxRadius, 1f - waveRemaining / distortionDuration);
			for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
			{
				PlayerController playerController = GameManager.Instance.AllPlayers[i];
				if (playerController.healthHaver.IsDead || playerController.spriteAnimator.QueryInvulnerabilityFrame() || !playerController.healthHaver.IsVulnerable)
				{
					continue;
				}
				Vector2 unitCenter = playerController.specRigidbody.GetUnitCenter(ColliderType.HitBox);
				float num = Vector2.Distance(unitCenter, startPoint);
				if (!(num < m_prevWaveDist - 0.25f) && !(num > waveDist + 0.25f))
				{
					float b = (unitCenter - startPoint).ToAngle();
					if (!(BraveMathCollege.AbsAngleBetween(playerController.FacingDirection, b) < 60f))
					{
						playerController.CurrentStoneGunTimer = StoneDuration;
					}
				}
			}
			m_prevWaveDist = waveDist;
			yield return null;
		}
	}
}
