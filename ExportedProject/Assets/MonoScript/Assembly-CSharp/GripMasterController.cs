using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class GripMasterController : BraveBehaviour, IPlaceConfigurable, IHasDwarfConfigurables
{
	[DwarfConfigurable]
	public bool Grip_StartAsEnemy;

	[DwarfConfigurable]
	public bool Grip_EndOnEnemiesCleared = true;

	[DwarfConfigurable]
	public int Grip_EndAfterNumAttacks = -1;

	[DwarfConfigurable]
	public int Grip_OverrideRoomsToSendBackward = -1;

	[Header("Become Enemy Stuff")]
	public DungeonPrerequisite BecomeEnemeyPrereq;

	public float BecomeEnemyChance = 0.5f;

	public int MinRoomWidth = 20;

	public int MinRoomHeight = 15;

	private bool m_isAttacking;

	private bool m_shouldBecomeEnemy;

	private int m_numTimesAttacked;

	private bool m_isEnemy;

	private DungeonData.Direction m_facingDirection;

	private float m_turnTimer;

	private List<AIActor> m_activeEnemies = new List<AIActor>();

	public bool IsAttacking
	{
		set
		{
			m_isAttacking = value;
			base.aiActor.IgnoreForRoomClear = Grip_EndOnEnemiesCleared && !m_shouldBecomeEnemy && !m_isAttacking;
		}
	}

	public void Start()
	{
		base.specRigidbody.CollideWithOthers = false;
		base.aiActor.IsGone = true;
		if (Grip_StartAsEnemy)
		{
			m_shouldBecomeEnemy = true;
			End(true);
		}
		else
		{
			if (ShouldBecomeEnemy())
			{
				m_shouldBecomeEnemy = true;
			}
			base.aiActor.IgnoreForRoomClear = Grip_EndOnEnemiesCleared && !m_shouldBecomeEnemy;
		}
		if (Grip_EndAfterNumAttacks < 0 && !Grip_EndOnEnemiesCleared)
		{
			Debug.LogErrorFormat("Gripmaster was told to last forever! ({0})", base.aiActor.ParentRoom.GetRoomName());
			Grip_EndOnEnemiesCleared = true;
		}
		if (!m_isEnemy)
		{
			base.gameObject.SetLayerRecursively(LayerMask.NameToLayer("Unoccluded"));
		}
	}

	public void Update()
	{
		if (!m_isEnemy)
		{
			if (!base.healthHaver.IsAlive || !base.aiAnimator.IsIdle())
			{
				return;
			}
			if (Grip_EndAfterNumAttacks > 0 && m_numTimesAttacked >= Grip_EndAfterNumAttacks)
			{
				End();
			}
			if (!Grip_EndOnEnemiesCleared)
			{
				return;
			}
			if (m_shouldBecomeEnemy)
			{
				base.aiActor.ParentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.RoomClear, ref m_activeEnemies);
				bool flag = false;
				for (int i = 0; i < m_activeEnemies.Count; i++)
				{
					if ((bool)m_activeEnemies[i] && !m_activeEnemies[i].healthHaver.PreventAllDamage)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					End();
				}
			}
			else if (base.aiActor.ParentRoom.GetActiveEnemiesCount(RoomHandler.ActiveEnemyType.RoomClear) <= 0)
			{
				End();
			}
			return;
		}
		if (base.healthHaver.IsAlive && base.aiAnimator.IsIdle() && m_turnTimer <= 0f && (bool)base.aiActor.TargetRigidbody)
		{
			Vector2 vec = base.aiActor.TargetRigidbody.GetUnitCenter(ColliderType.HitBox) - base.specRigidbody.GetUnitCenter(ColliderType.HitBox);
			DungeonData.Direction direction = DungeonData.GetCardinalFromVector2(vec);
			if (direction == DungeonData.Direction.NORTH)
			{
				direction = DungeonData.Direction.SOUTH;
			}
			if (m_facingDirection != direction)
			{
				string text = ((m_facingDirection == DungeonData.Direction.WEST) ? ((direction != DungeonData.Direction.EAST) ? "red_trans_west_south" : "red_trans_west_east") : ((m_facingDirection != DungeonData.Direction.SOUTH) ? ((direction != DungeonData.Direction.SOUTH) ? "red_trans_east_west" : "red_trans_east_south") : ((direction != DungeonData.Direction.WEST) ? "red_trans_south_east" : "red_trans_south_west")));
				base.aiAnimator.PlayUntilFinished(text);
				base.aiAnimator.AnimatedFacingDirection = DungeonData.GetAngleFromDirection(direction);
				m_facingDirection = direction;
				m_turnTimer = 1f;
				base.behaviorSpeculator.AttackCooldown = Mathf.Max(base.behaviorSpeculator.AttackCooldown, base.aiAnimator.CurrentClipLength);
			}
		}
		m_turnTimer = Mathf.Max(0f, m_turnTimer - base.aiActor.LocalDeltaTime);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public void OnAttack()
	{
		m_numTimesAttacked++;
	}

	public void ConfigureOnPlacement(RoomHandler room)
	{
		base.aiActor.IgnoreForRoomClear = Grip_EndOnEnemiesCleared;
	}

	public void End(bool skipAnim = false)
	{
		if (m_shouldBecomeEnemy)
		{
			base.healthHaver.PreventAllDamage = false;
			base.specRigidbody.CollideWithOthers = true;
			base.specRigidbody.PixelColliders[0].ManualOffsetY = 28;
			base.specRigidbody.ForceRegenerate();
			base.aiActor.IsGone = false;
			base.aiAnimator.IdleAnimation.Type = DirectionalAnimation.DirectionType.FourWayCardinal;
			base.aiAnimator.IdleAnimation.AnimNames = new string[4] { "red_idle_south", "red_idle_east", "red_idle_south", "red_idle_west" };
			base.aiAnimator.IdleAnimation.Flipped = new DirectionalAnimation.FlipType[4]
			{
				DirectionalAnimation.FlipType.None,
				DirectionalAnimation.FlipType.None,
				DirectionalAnimation.FlipType.None,
				DirectionalAnimation.FlipType.None
			};
			base.aiAnimator.OtherAnimations.Find((AIAnimator.NamedDirectionalAnimation a) => a.name == "death").anim.Prefix = "red_die";
			base.aiAnimator.UseAnimatedFacingDirection = true;
			base.aiAnimator.FacingDirection = -90f;
			m_facingDirection = DungeonData.Direction.SOUTH;
			if (!skipAnim)
			{
				base.aiAnimator.PlayUntilFinished("transform");
				base.behaviorSpeculator.GlobalCooldown = Mathf.Max(base.behaviorSpeculator.AttackCooldown, base.aiAnimator.CurrentClipLength);
				m_turnTimer = base.aiAnimator.CurrentClipLength;
				base.aiActor.MoveToSafeSpot(base.aiAnimator.CurrentClipLength);
			}
			AttackBehaviorGroup attackBehaviorGroup = base.behaviorSpeculator.AttackBehaviorGroup;
			attackBehaviorGroup.AttackBehaviors[0].Probability = 0f;
			attackBehaviorGroup.AttackBehaviors[1].Probability = 1f;
			base.gameObject.SetLayerRecursively(LayerMask.NameToLayer("FG_Reflection"));
			m_isEnemy = true;
		}
		else
		{
			base.healthHaver.ApplyDamage(10000f, Vector2.zero, "Grip Master Finished", CoreDamageTypes.None, DamageCategory.Unstoppable, true);
		}
	}

	private bool ShouldBecomeEnemy()
	{
		if (!BecomeEnemeyPrereq.CheckConditionsFulfilled())
		{
			return false;
		}
		RoomHandler parentRoom = base.aiActor.ParentRoom;
		if (parentRoom != null && (parentRoom.area.dimensions.x < MinRoomWidth || parentRoom.area.dimensions.y < MinRoomHeight))
		{
			return false;
		}
		return Random.value < BecomeEnemyChance;
	}
}
