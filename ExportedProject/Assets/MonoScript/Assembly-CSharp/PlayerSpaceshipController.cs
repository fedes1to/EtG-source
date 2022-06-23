using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpaceshipController : PlayerController
{
	public Texture2D PaletteTex;

	public List<Transform> LaserShootPoints;

	public tk2dSpriteAnimation TimefallCorpseLibrary;

	[Header("Spaceship Controls")]
	public float LaserACooldown = 0.15f;

	public float MissileCooldown = 1f;

	private float m_aimAngle;

	private float m_fireCooldown;

	private float m_missileCooldown;

	private bool m_isFiring;

	public override bool IsFlying
	{
		get
		{
			return true;
		}
	}

	protected override bool CanDodgeRollWhileFlying
	{
		get
		{
			return true;
		}
	}

	public override void Start()
	{
		base.Start();
		if (PaletteTex != null)
		{
			base.sprite.usesOverrideMaterial = true;
			base.sprite.renderer.material.SetTexture("_PaletteTex", PaletteTex);
		}
		ToggleHandRenderers(false, "ships don't have hands");
		base.sprite.IsPerpendicular = false;
		base.sprite.HeightOffGround = 3f;
		base.sprite.UpdateZDepth();
	}

	public override void Update()
	{
		base.Update();
		if (!base.AcceptingNonMotionInput)
		{
			m_isFiring = false;
			m_shouldContinueFiring = false;
		}
		if (!base.IsDodgeRolling)
		{
			int num = BraveMathCollege.AngleToOctant(m_aimAngle);
			float num2 = (float)num * -45f;
			float aimAngle = m_aimAngle;
			float value = aimAngle - num2;
			value = value.Quantize(10f);
			base.sprite.transform.parent.rotation = Quaternion.Euler(0f, 0f, -90f + value);
		}
		if (m_isFiring && m_fireCooldown <= 0f)
		{
			FireProjectiles();
			m_fireCooldown = LaserACooldown;
		}
		m_missileCooldown -= BraveTime.DeltaTime;
		m_fireCooldown -= BraveTime.DeltaTime;
	}

	protected void FireMissileVolley()
	{
		if (!(m_missileCooldown <= 0f))
		{
			return;
		}
		for (int i = 0; i < LaserShootPoints.Count; i++)
		{
			for (int j = 0; j < 5; j++)
			{
				FireBullet(LaserShootPoints[i], Quaternion.Euler(0f, 0f, -90f + m_aimAngle + Mathf.Lerp(-20f, 20f, (float)j / 4f)) * Vector2.up, "missile");
			}
		}
		m_missileCooldown = MissileCooldown;
		if ((bool)base.CurrentItem)
		{
			float destroyTime = -1f;
			base.CurrentItem.timeCooldown = MissileCooldown;
			base.CurrentItem.Use(this, out destroyTime);
		}
	}

	protected override void CheckSpawnEmergencyCrate()
	{
	}

	protected void FireProjectiles()
	{
		for (int i = 0; i < LaserShootPoints.Count; i++)
		{
			FireBullet(LaserShootPoints[i], Quaternion.Euler(0f, 0f, -90f + m_aimAngle) * Vector2.up, "default");
		}
	}

	private void FireBullet(Transform shootPoint, Vector2 dirVec, string bulletType)
	{
		GameObject gameObject = base.bulletBank.CreateProjectileFromBank(shootPoint.position, BraveMathCollege.Atan2Degrees(dirVec.normalized), bulletType);
		Projectile component = gameObject.GetComponent<Projectile>();
		component.Owner = this;
		component.Shooter = base.specRigidbody;
		component.collidesWithPlayer = false;
		component.specRigidbody.RegisterSpecificCollisionException(base.specRigidbody);
	}

	protected override void Die(Vector2 finalDamageDirection)
	{
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.SINGLE_PLAYER || GameManager.Instance.NumberOfLivingPlayers == 0)
		{
			if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.CHARACTER_PAST)
			{
				GameManager.Instance.platformInterface.AchievementUnlock(Achievement.DIE_IN_PAST);
			}
			base.CurrentInputState = PlayerInputState.NoInput;
			if ((bool)CurrentGun)
			{
				CurrentGun.CeaseAttack();
			}
			Transform transform = GameManager.Instance.MainCameraController.transform;
			Vector3 position = transform.position;
			GameManager.Instance.MainCameraController.OverridePosition = position;
			GameManager.Instance.StartCoroutine(HandleDelayedEndGame());
		}
		base.gameObject.SetActive(false);
	}

	private IEnumerator HandleDelayedEndGame()
	{
		base.CurrentInputState = PlayerInputState.NoInput;
		yield return new WaitForSeconds(0.25f);
		HandleDeathPhotography();
		float ela = 0f;
		float dura = 4f;
		while (ela < dura)
		{
			ela += BraveTime.DeltaTime;
			yield return null;
		}
		Pixelator.Instance.CustomFade(0.6f, 0f, Color.white, Color.black, 0.1f, 0.5f);
		Pixelator.Instance.LerpToLetterbox(0.35f, 0.8f);
		GameStatsManager.Instance.RegisterStatChange(TrackedStats.NUMBER_DEATHS, 1f);
		AmmonomiconDeathPageController.LastKilledPlayerPrimary = base.IsPrimaryPlayer;
		GameManager.Instance.DoGameOver(base.healthHaver.lastIncurredDamageSource);
	}

	public override void ResurrectFromBossKill()
	{
		if (!base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(true);
		}
		Chest.ToggleCoopChests(false);
		base.healthHaver.FullHeal();
	}

	protected override string GetBaseAnimationName(Vector2 v, float unusedGunAngle, bool invertThresholds = false, bool forceTwoHands = false)
	{
		string result = string.Empty;
		switch (BraveMathCollege.AngleToOctant(m_aimAngle))
		{
		case 0:
			result = ((!m_isFiring) ? "idle_n" : "fire_n");
			break;
		case 1:
			result = ((!m_isFiring) ? "idle_ne" : "fire_ne");
			break;
		case 2:
			result = ((!m_isFiring) ? "idle_e" : "fire_e");
			break;
		case 3:
			result = ((!m_isFiring) ? "idle_se" : "fire_se");
			break;
		case 4:
			result = ((!m_isFiring) ? "idle_s" : "fire_s");
			break;
		case 5:
			result = ((!m_isFiring) ? "idle_sw" : "fire_sw");
			break;
		case 6:
			result = ((!m_isFiring) ? "idle_w" : "fire_w");
			break;
		case 7:
			result = ((!m_isFiring) ? "idle_nw" : "fire_nw");
			break;
		}
		return result;
	}

	protected override void PlayDodgeRollAnimation(Vector2 direction)
	{
		tk2dSpriteAnimationClip tk2dSpriteAnimationClip2 = null;
		direction.Normalize();
		if (m_dodgeRollState != 0)
		{
			float num = direction.ToAngle();
			int num2 = BraveMathCollege.AngleToOctant(m_aimAngle);
			float num3 = BraveMathCollege.ClampAngle180(num - m_aimAngle);
			string text = ((!(num3 >= 0f)) ? "dodgeroll_right_" : "dodgeroll_left_");
			switch (num2)
			{
			case 0:
				text += "n";
				break;
			case 1:
				text += "ne";
				break;
			case 2:
				text += "e";
				break;
			case 3:
				text += "se";
				break;
			case 4:
				text += "s";
				break;
			case 5:
				text += "sw";
				break;
			case 6:
				text += "w";
				break;
			case 7:
				text += "nw";
				break;
			}
			tk2dSpriteAnimationClip2 = base.spriteAnimator.GetClipByName(text);
		}
		if (tk2dSpriteAnimationClip2 != null)
		{
			float overrideFps = (float)tk2dSpriteAnimationClip2.frames.Length / rollStats.GetModifiedTime(this);
			base.spriteAnimator.Play(tk2dSpriteAnimationClip2, 0f, overrideFps);
			m_handlingQueuedAnimation = true;
		}
	}

	protected override void HandleFlipping(float gunAngle)
	{
	}

	protected override Vector2 HandlePlayerInput()
	{
		Vector2 vector = Vector2.zero;
		if (base.CurrentInputState != PlayerInputState.NoMovement)
		{
			vector = AdjustInputVector(m_activeActions.Move.Vector, BraveInput.MagnetAngles.movementCardinal, BraveInput.MagnetAngles.movementOrdinal);
		}
		if (vector.magnitude > 1f)
		{
			vector.Normalize();
		}
		HandleStartDodgeRoll(vector);
		CollisionData result = null;
		if (vector.x > 0.01f && PhysicsEngine.Instance.RigidbodyCast(base.specRigidbody, IntVector2.Right, out result, true, false))
		{
			vector.x = 0f;
		}
		CollisionData.Pool.Free(ref result);
		if (vector.x < -0.01f && PhysicsEngine.Instance.RigidbodyCast(base.specRigidbody, IntVector2.Left, out result, true, false))
		{
			vector.x = 0f;
		}
		CollisionData.Pool.Free(ref result);
		if (vector.y > 0.01f && PhysicsEngine.Instance.RigidbodyCast(base.specRigidbody, IntVector2.Up, out result, true, false))
		{
			vector.y = 0f;
		}
		CollisionData.Pool.Free(ref result);
		if (vector.y < -0.01f && PhysicsEngine.Instance.RigidbodyCast(base.specRigidbody, IntVector2.Down, out result, true, false))
		{
			vector.y = 0f;
		}
		CollisionData.Pool.Free(ref result);
		if (base.AcceptingNonMotionInput)
		{
			GameOptions.ControllerBlankControl controllerBlankControl = ((!base.IsPrimaryPlayer) ? GameManager.Options.additionalBlankControlTwo : GameManager.Options.additionalBlankControl);
			bool flag = controllerBlankControl == GameOptions.ControllerBlankControl.BOTH_STICKS_DOWN && m_activeActions.CheckBothSticksButton();
			if (Time.timeScale > 0f && (m_activeActions.BlankAction.WasPressed || flag))
			{
				DoConsumableBlank();
			}
			if (BraveInput.GetInstanceForPlayer(PlayerIDX).GetButtonDown(GungeonActions.GungeonActionType.UseItem))
			{
				FireMissileVolley();
				BraveInput.GetInstanceForPlayer(PlayerIDX).ConsumeButtonDown(GungeonActions.GungeonActionType.UseItem);
			}
		}
		if (base.AcceptingNonMotionInput || base.CurrentInputState == PlayerInputState.FoyerInputOnly)
		{
			Vector2 vector2 = DetermineAimPointInWorld();
			m_aimAngle = BraveMathCollege.Atan2Degrees(vector2 - base.CenterPosition);
		}
		if (m_isFiring && !BraveInput.GetInstanceForPlayer(PlayerIDX).GetButton(GungeonActions.GungeonActionType.Shoot))
		{
			m_isFiring = false;
			m_shouldContinueFiring = false;
		}
		if (base.SuppressThisClick)
		{
			while (BraveInput.GetInstanceForPlayer(PlayerIDX).GetButtonDown(GungeonActions.GungeonActionType.Shoot))
			{
				BraveInput.GetInstanceForPlayer(PlayerIDX).ConsumeButtonDown(GungeonActions.GungeonActionType.Shoot);
				if (BraveInput.GetInstanceForPlayer(PlayerIDX).GetButtonUp(GungeonActions.GungeonActionType.Shoot))
				{
					BraveInput.GetInstanceForPlayer(PlayerIDX).ConsumeButtonUp(GungeonActions.GungeonActionType.Shoot);
				}
			}
			if (!BraveInput.GetInstanceForPlayer(PlayerIDX).GetButton(GungeonActions.GungeonActionType.Shoot))
			{
				base.SuppressThisClick = false;
			}
		}
		else if (base.m_CanAttack && BraveInput.GetInstanceForPlayer(PlayerIDX).GetButtonDown(GungeonActions.GungeonActionType.Shoot))
		{
			bool flag2 = false;
			m_isFiring = true;
			flag2 = flag2 || true;
			m_shouldContinueFiring = true;
			if (flag2)
			{
				BraveInput.GetInstanceForPlayer(PlayerIDX).ConsumeButtonDown(GungeonActions.GungeonActionType.Shoot);
			}
		}
		else if (BraveInput.GetInstanceForPlayer(PlayerIDX).GetButtonUp(GungeonActions.GungeonActionType.Shoot))
		{
			m_isFiring = false;
			m_shouldContinueFiring = false;
			BraveInput.GetInstanceForPlayer(PlayerIDX).ConsumeButtonUp(GungeonActions.GungeonActionType.Shoot);
		}
		return vector;
	}
}
