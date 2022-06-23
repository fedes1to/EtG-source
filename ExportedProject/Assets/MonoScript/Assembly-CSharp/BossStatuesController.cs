using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossStatuesController : BraveBehaviour
{
	public List<BossStatueController> allStatues;

	public float groundedTime = 0.15f;

	public float moveSpeed = 5f;

	public float transitionMoveSpeed = 10f;

	public float moveHopHeight = 1.5f;

	public float moveHopTime = 0.33f;

	public float attackHopHeight = 1.5f;

	public float attackHopTime = 0.33f;

	private Vector2 m_patternCenter;

	public Vector2 PatternCenter
	{
		get
		{
			return m_patternCenter;
		}
	}

	public int NumLivingStatues { get; set; }

	public float MoveHopSpeed { get; set; }

	public float MoveGravity { get; set; }

	public float AttackHopSpeed { get; set; }

	public float AttackGravity { get; set; }

	public float? OverrideMoveSpeed { get; set; }

	public float CurrentMoveSpeed
	{
		get
		{
			if (IsTransitioning)
			{
				return transitionMoveSpeed;
			}
			if (OverrideMoveSpeed.HasValue)
			{
				return OverrideMoveSpeed.Value;
			}
			return moveSpeed;
		}
	}

	public bool IsTransitioning { get; set; }

	public void Awake()
	{
		for (int i = 0; i < allStatues.Count; i++)
		{
			allStatues[i].healthHaver.OnPreDeath += OnStatueDeath;
		}
		NumLivingStatues = allStatues.Count;
		base.bulletBank.CollidesWithEnemies = false;
		m_patternCenter = base.transform.position.XY() + new Vector2((float)base.dungeonPlaceable.placeableWidth / 2f, (float)base.dungeonPlaceable.placeableHeight / 2f);
		RecalculateHopSpeeds();
		if (TurboModeController.IsActive)
		{
			moveSpeed *= TurboModeController.sEnemyMovementSpeedMultiplier;
		}
	}

	public void Update()
	{
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public void RecalculateHopSpeeds()
	{
		MoveHopSpeed = 2f * (moveHopHeight / (0.5f * moveHopTime));
		MoveGravity = (0f - MoveHopSpeed) / (0.5f * moveHopTime);
		AttackHopSpeed = 2f * (attackHopHeight / (0.5f * attackHopTime));
		AttackGravity = (0f - AttackHopSpeed) / (0.5f * attackHopTime);
	}

	public float GetEffectiveMoveSpeed(float speed)
	{
		float num = moveHopTime + groundedTime;
		return speed * (moveHopTime / num);
	}

	public void ClearBullets(Vector2 centerPoint)
	{
		StartCoroutine(HandleSilence(centerPoint, 30f, 30f));
	}

	private IEnumerator HandleSilence(Vector2 centerPoint, float expandSpeed, float maxRadius)
	{
		float currentRadius = 0f;
		while (currentRadius < maxRadius)
		{
			currentRadius += expandSpeed * BraveTime.DeltaTime;
			SilencerInstance.DestroyBulletsInRange(centerPoint, currentRadius, true, false);
			yield return null;
		}
	}

	private void OnStatueDeath(Vector2 finalDeathDir)
	{
		for (int i = 0; i < allStatues.Count; i++)
		{
			if ((bool)allStatues[i] && !allStatues[i].healthHaver.IsDead)
			{
				allStatues[i].LevelUp();
			}
		}
		NumLivingStatues--;
		if (NumLivingStatues == 0)
		{
			EncounterTrackable component = GetComponent<EncounterTrackable>();
			if (component != null)
			{
				GameStatsManager.Instance.HandleEncounteredObject(component);
			}
			GameStatsManager.Instance.SetFlag(GungeonFlags.BOSSKILLED_STATUES, true);
		}
	}
}
