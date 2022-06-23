using System;
using Dungeonator;
using UnityEngine;

public class BrazierController : DungeonPlaceableBehaviour, IPlayerInteractable, IPlaceConfigurable
{
	public DebrisDirectionalAnimationInfo directionalAnimationInfo;

	public GoopDefinition goop;

	[DwarfConfigurable]
	public float goopLength = 6f;

	[DwarfConfigurable]
	public float goopWidth = 2f;

	[DwarfConfigurable]
	public float goopTime = 1f;

	public string BreakAnimName;

	public DebrisDirectionalAnimationInfo directionalBreakAnims;

	private float m_accumParticleCount;

	private bool m_flipped;

	private float m_flipTime = -1f;

	private Vector2 m_cachedFlipVector;

	public float GetDistanceToPoint(Vector2 point)
	{
		Bounds bounds = base.sprite.GetBounds();
		bounds.SetMinMax(bounds.min + base.sprite.transform.position, bounds.max + base.sprite.transform.position);
		float num = Mathf.Max(Mathf.Min(point.x, bounds.max.x), bounds.min.x);
		float num2 = Mathf.Max(Mathf.Min(point.y, bounds.max.y), bounds.min.y);
		return Mathf.Sqrt((point.x - num) * (point.x - num) + (point.y - num2) * (point.y - num2));
	}

	public float GetOverrideMaxDistance()
	{
		return -1f;
	}

	private void Start()
	{
		SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
		speculativeRigidbody.OnPreRigidbodyCollision = (SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate)Delegate.Combine(speculativeRigidbody.OnPreRigidbodyCollision, new SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate(HandlePreCollision));
	}

	private void HandlePreCollision(SpeculativeRigidbody myRigidbody, PixelCollider myPixelCollider, SpeculativeRigidbody otherRigidbody, PixelCollider otherPixelCollider)
	{
		if ((bool)otherRigidbody.gameActor && otherRigidbody.gameActor is PlayerController)
		{
			OnPlayerCollision(otherRigidbody.gameActor as PlayerController);
		}
	}

	private void OnPlayerCollision(PlayerController p)
	{
		if (p != null && (p.IsDodgeRolling || m_flipped) && (p.IsDodgeRolling || !(Time.realtimeSinceStartup - m_flipTime < 0.25f)))
		{
			if (m_flipped)
			{
				base.spriteAnimator.Play(directionalBreakAnims.GetAnimationForVector(m_cachedFlipVector));
			}
			else
			{
				base.spriteAnimator.Play(BreakAnimName);
			}
			base.sprite.IsPerpendicular = false;
			base.sprite.HeightOffGround = -1.25f;
			base.sprite.UpdateZDepth();
			base.specRigidbody.enabled = false;
			base.transform.position.GetAbsoluteRoom().DeregisterInteractable(this);
			SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
			speculativeRigidbody.OnPreRigidbodyCollision = (SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate)Delegate.Remove(speculativeRigidbody.OnPreRigidbodyCollision, new SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate(HandlePreCollision));
		}
	}

	private void Update()
	{
		if (GameManager.Options.ShaderQuality != 0 && GameManager.Options.ShaderQuality != GameOptions.GenericHighMedLowOption.VERY_LOW && !m_flipped && base.specRigidbody.enabled)
		{
			m_accumParticleCount += BraveTime.DeltaTime * 10f;
			if (m_accumParticleCount > 1f)
			{
				int num = Mathf.FloorToInt(m_accumParticleCount);
				m_accumParticleCount -= num;
				GlobalSparksDoer.DoRandomParticleBurst(num, base.specRigidbody.UnitBottomLeft.ToVector3ZisY(), base.specRigidbody.UnitTopRight.ToVector3ZisY(), Vector3.up, 120f, 0.5f, null, null, null, GlobalSparksDoer.SparksType.EMBERS_SWIRLING);
			}
		}
	}

	public void OnEnteredRange(PlayerController interactor)
	{
		if ((bool)this)
		{
			SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.white);
		}
	}

	public void OnExitRange(PlayerController interactor)
	{
		if ((bool)this)
		{
			SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite);
		}
	}

	public void Interact(PlayerController interactor)
	{
		m_flipped = true;
		Vector2 normalized = (base.specRigidbody.UnitCenter - interactor.specRigidbody.UnitCenter).normalized;
		GameManager.Instance.Dungeon.GetRoomFromPosition(base.transform.position.IntXY()).DeregisterInteractable(this);
		m_cachedFlipVector = normalized;
		base.spriteAnimator.Play(directionalAnimationInfo.GetAnimationForVector(normalized));
		Vector2 normalized2 = BraveUtility.GetMajorAxis(normalized).normalized;
		Vector2 vector = base.specRigidbody.UnitCenter + normalized2;
		if (GameManager.Options.ShaderQuality != 0 && GameManager.Options.ShaderQuality != GameOptions.GenericHighMedLowOption.VERY_LOW)
		{
			GlobalSparksDoer.DoRandomParticleBurst(UnityEngine.Random.Range(25, 40), base.specRigidbody.UnitBottomLeft.ToVector3ZisY(), base.specRigidbody.UnitTopRight.ToVector3ZisY(), Vector3.up, 120f, 0.5f, null, null, null, GlobalSparksDoer.SparksType.EMBERS_SWIRLING);
		}
		DeadlyDeadlyGoopManager.GetGoopManagerForGoopType(goop).TimedAddGoopLine(vector, vector + normalized2 * goopLength, goopWidth / 2f, goopTime);
		m_flipTime = Time.realtimeSinceStartup;
		DeadlyDeadlyGoopManager.IgniteGoopsCircle(vector, 1.5f);
	}

	public string GetAnimationState(PlayerController interactor, out bool shouldBeFlipped)
	{
		shouldBeFlipped = false;
		Vector2 vec = base.specRigidbody.UnitCenter - interactor.specRigidbody.UnitCenter;
		switch (DungeonData.GetCardinalFromVector2(vec))
		{
		case DungeonData.Direction.NORTH:
			return "tablekick_up";
		case DungeonData.Direction.EAST:
			return "tablekick_right";
		case DungeonData.Direction.SOUTH:
			return "tablekick_down";
		case DungeonData.Direction.WEST:
			shouldBeFlipped = true;
			return "tablekick_right";
		default:
			return "tablekick_right";
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public void ConfigureOnPlacement(RoomHandler room)
	{
		base.PlacedPosition = base.transform.position.IntXY(VectorConversions.Floor);
		for (int i = base.PlacedPosition.x; i < base.PlacedPosition.x + 2; i++)
		{
			for (int j = base.PlacedPosition.y; j < base.PlacedPosition.y + 2; j++)
			{
				GameManager.Instance.Dungeon.data[i, j].isOccupied = true;
			}
		}
	}
}
