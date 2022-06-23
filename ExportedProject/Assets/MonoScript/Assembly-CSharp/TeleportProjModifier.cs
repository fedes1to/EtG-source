using System;
using System.Collections;
using Dungeonator;
using UnityEngine;

public class TeleportProjModifier : BraveBehaviour
{
	public enum TeleportTrigger
	{
		AngleToTarget = 10,
		DistanceFromTarget = 20
	}

	public enum TeleportType
	{
		BackToSpawn = 10,
		BehindTarget = 20
	}

	public TeleportTrigger trigger = TeleportTrigger.AngleToTarget;

	[ShowInInspectorIf("ShowMMinAngleToTeleport", true)]
	public float minAngleToTeleport = 70f;

	[ShowInInspectorIf("ShowDistToTeleport", true)]
	public float distToTeleport = 3f;

	public TeleportType type = TeleportType.BackToSpawn;

	[ShowInInspectorIf("ShowBehindTargetDistance", true)]
	public float behindTargetDistance = 5f;

	public int numTeleports;

	public float teleportPauseTime;

	public float leadAmount;

	public float teleportCooldown;

	public VFXPool teleportVfx;

	private SpeculativeRigidbody m_targetRigidbody;

	private Vector3 m_startingPos;

	private bool m_isTeleporting;

	private float m_cooldown;

	public event Action OnTeleport;

	private bool ShowMMinAngleToTeleport()
	{
		return trigger == TeleportTrigger.AngleToTarget;
	}

	private bool ShowDistToTeleport()
	{
		return trigger == TeleportTrigger.DistanceFromTarget;
	}

	private bool ShowBehindTargetDistance()
	{
		return type == TeleportType.BehindTarget;
	}

	public void Start()
	{
		if (!base.sprite)
		{
			base.sprite = GetComponentInChildren<tk2dSprite>();
		}
		if ((bool)base.projectile && base.projectile.Owner is AIActor)
		{
			m_targetRigidbody = (base.projectile.Owner as AIActor).TargetRigidbody;
		}
		if (!m_targetRigidbody)
		{
			base.enabled = false;
		}
		else
		{
			m_startingPos = base.transform.position;
		}
	}

	public void Update()
	{
		if (!m_isTeleporting)
		{
			if (m_cooldown > 0f)
			{
				m_cooldown -= BraveTime.DeltaTime;
			}
			else if (numTeleports > 0 && ShouldTeleport())
			{
				StartCoroutine(DoTeleport());
			}
		}
	}

	protected override void OnDestroy()
	{
		StopAllCoroutines();
		base.OnDestroy();
	}

	private bool ShouldTeleport()
	{
		Vector2 unitCenter = m_targetRigidbody.GetUnitCenter(ColliderType.HitBox);
		if (trigger == TeleportTrigger.AngleToTarget)
		{
			float a = (unitCenter - base.specRigidbody.UnitCenter).ToAngle();
			float b = base.specRigidbody.Velocity.ToAngle();
			return BraveMathCollege.AbsAngleBetween(a, b) > minAngleToTeleport;
		}
		if (trigger == TeleportTrigger.DistanceFromTarget)
		{
			return Vector2.Distance(unitCenter, base.specRigidbody.UnitCenter) < distToTeleport;
		}
		return false;
	}

	private Vector2 GetTeleportPosition()
	{
		if (type == TeleportType.BackToSpawn)
		{
			return m_startingPos;
		}
		if (type == TeleportType.BehindTarget && (bool)m_targetRigidbody && (bool)m_targetRigidbody.gameActor)
		{
			Vector2 unitCenter = m_targetRigidbody.GetUnitCenter(ColliderType.HitBox);
			float facingDirection = m_targetRigidbody.gameActor.FacingDirection;
			Dungeon dungeon = GameManager.Instance.Dungeon;
			for (int i = 0; i < 18; i++)
			{
				Vector2 vector = unitCenter + BraveMathCollege.DegreesToVector(facingDirection + 180f + (float)(i * 20), behindTargetDistance);
				if (!dungeon.CellExists(vector) || !dungeon.data.isWall((int)vector.x, (int)vector.y))
				{
					return vector;
				}
				vector = unitCenter + BraveMathCollege.DegreesToVector(facingDirection + 180f + (float)(i * -20), behindTargetDistance);
				if (!dungeon.CellExists(vector) || !dungeon.data.isWall((int)vector.x, (int)vector.y))
				{
					return vector;
				}
			}
		}
		return m_startingPos;
	}

	private IEnumerator DoTeleport()
	{
		VFXPool vFXPool = teleportVfx;
		Vector3 position = base.specRigidbody.UnitCenter;
		Transform parent = base.transform;
		vFXPool.SpawnAtPosition(position, 0f, parent);
		if (teleportPauseTime > 0f)
		{
			m_isTeleporting = true;
			base.sprite.renderer.enabled = false;
			base.projectile.enabled = false;
			base.specRigidbody.enabled = false;
			if ((bool)base.projectile.braveBulletScript)
			{
				base.projectile.braveBulletScript.enabled = false;
			}
			yield return new WaitForSeconds(teleportPauseTime);
			if (!this || !m_targetRigidbody)
			{
				yield break;
			}
			m_isTeleporting = false;
			base.sprite.renderer.enabled = true;
			base.projectile.enabled = true;
			base.specRigidbody.enabled = true;
			if ((bool)base.projectile.braveBulletScript)
			{
				base.projectile.braveBulletScript.enabled = true;
			}
		}
		Vector2 newPosition = GetTeleportPosition();
		base.transform.position = newPosition;
		base.specRigidbody.Reinitialize();
		VFXPool vFXPool2 = teleportVfx;
		position = base.specRigidbody.UnitCenter;
		parent = base.transform;
		vFXPool2.SpawnAtPosition(position, 0f, parent);
		Vector2 firingCenter = base.specRigidbody.UnitCenter;
		Vector2 targetCenter = m_targetRigidbody.specRigidbody.GetUnitCenter(ColliderType.HitBox);
		PlayerController targetPlayer = m_targetRigidbody.gameActor as PlayerController;
		if (leadAmount > 0f && (bool)targetPlayer)
		{
			Vector2 targetVelocity = ((!targetPlayer) ? m_targetRigidbody.Velocity : targetPlayer.AverageVelocity);
			Vector2 predictedPosition = BraveMathCollege.GetPredictedPosition(targetCenter, targetVelocity, firingCenter, base.projectile.Speed);
			targetCenter = Vector2.Lerp(targetCenter, predictedPosition, leadAmount);
		}
		base.projectile.SendInDirection(targetCenter - firingCenter, true);
		if ((bool)base.projectile.braveBulletScript && base.projectile.braveBulletScript.bullet != null)
		{
			base.projectile.braveBulletScript.bullet.Position = newPosition;
			base.projectile.braveBulletScript.bullet.Direction = (targetCenter - newPosition).ToAngle();
		}
		numTeleports--;
		m_cooldown = teleportCooldown;
		if (this.OnTeleport != null)
		{
			this.OnTeleport();
		}
	}
}
