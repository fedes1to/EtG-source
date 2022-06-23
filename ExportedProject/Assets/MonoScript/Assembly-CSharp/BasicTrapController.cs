using System;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;
using UnityEngine.Serialization;

public class BasicTrapController : TrapController, IPlaceConfigurable
{
	public enum TriggerMethod
	{
		SpecRigidbody,
		PlaceableFootprint,
		Timer,
		Script
	}

	public enum DamageMethod
	{
		SpecRigidbody,
		PlaceableFootprint,
		OnTrigger
	}

	protected enum State
	{
		Ready,
		Triggered,
		Active,
		Resetting
	}

	[Serializable]
	public class PlaceableFootprintBuffer
	{
		public int left;

		public int bottom;

		public int right;

		public int top;
	}

	public TriggerMethod triggerMethod;

	[DwarfConfigurable]
	[ShowInInspectorIf("triggerMethod", 2, false)]
	public float triggerTimerDelay = 1f;

	[ShowInInspectorIf("triggerMethod", 2, false)]
	[DwarfConfigurable]
	public float triggerTimerDelay1;

	[ShowInInspectorIf("triggerMethod", 2, false)]
	[DwarfConfigurable]
	public float triggerTimerOffset;

	public PlaceableFootprintBuffer footprintBuffer;

	public bool damagesFlyingPlayers;

	public bool triggerOnBlank;

	public bool triggerOnExplosion;

	[Header("Animations")]
	public bool animateChildren;

	[CheckAnimation(null)]
	public string triggerAnimName;

	public float triggerDelay;

	[CheckAnimation(null)]
	public string activeAnimName;

	public List<SpriteAnimatorKiller> activeVfx;

	public float activeTime;

	[CheckAnimation(null)]
	public string resetAnimName;

	public float resetDelay;

	[Header("Damage")]
	public DamageMethod damageMethod;

	[FormerlySerializedAs("activeDamage")]
	public float damage;

	public CoreDamageTypes damageTypes;

	[Header("Goop Interactions")]
	public bool IgnitesGoop;

	[NonSerialized]
	public float LocalTimeScale = 1f;

	private RoomHandler m_parentRoom;

	private State m_state;

	protected float m_stateTimer;

	protected float m_triggerTimer;

	protected float m_disabledTimer;

	protected IntVector2 m_cachedPosition;

	protected IntVector2 m_cachedPixelMin;

	protected IntVector2 m_cachedPixelMax;

	protected tk2dSpriteAnimator[] m_childrenAnimators;

	protected List<float> m_triggerTimerDelayArray;

	protected int m_triggerTimerDelayIndex;

	protected State state
	{
		get
		{
			return m_state;
		}
		set
		{
			if (m_state != value)
			{
				EndState(m_state);
				m_state = value;
				BeginState(m_state);
			}
		}
	}

	public virtual void Awake()
	{
		if ((bool)base.specRigidbody)
		{
			SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
			speculativeRigidbody.OnTriggerCollision = (SpeculativeRigidbody.OnTriggerDelegate)Delegate.Combine(speculativeRigidbody.OnTriggerCollision, new SpeculativeRigidbody.OnTriggerDelegate(OnTriggerCollision));
		}
		if (animateChildren)
		{
			m_childrenAnimators = GetComponentsInChildren<tk2dSpriteAnimator>();
		}
		if (triggerOnBlank || triggerOnExplosion)
		{
			StaticReferenceManager.AllTriggeredTraps.Add(this);
		}
	}

	public override void Start()
	{
		m_parentRoom = GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(base.transform.position.IntXY());
		m_cachedPosition = base.transform.position.IntXY(VectorConversions.Floor);
		m_cachedPixelMin = m_cachedPosition * PhysicsEngine.Instance.PixelsPerUnit + new IntVector2(footprintBuffer.left, footprintBuffer.bottom);
		m_cachedPixelMax = (m_cachedPosition + new IntVector2(placeableWidth, placeableHeight)) * PhysicsEngine.Instance.PixelsPerUnit - IntVector2.One - new IntVector2(footprintBuffer.right, footprintBuffer.top);
		if (triggerMethod == TriggerMethod.Timer)
		{
			m_triggerTimerDelayArray = new List<float>();
			if (triggerTimerDelay != 0f)
			{
				m_triggerTimerDelayArray.Add(triggerTimerDelay);
			}
			if (triggerTimerDelay1 != 0f)
			{
				m_triggerTimerDelayArray.Add(triggerTimerDelay1);
			}
			if (m_triggerTimerDelayArray.Count == 0)
			{
				m_triggerTimerDelayArray.Add(0f);
			}
			m_triggerTimer = triggerTimerOffset;
		}
		for (int i = 0; i < activeVfx.Count; i++)
		{
			if ((bool)activeVfx[i])
			{
				activeVfx[i].onlyDisable = true;
				activeVfx[i].Disable();
			}
		}
		base.Start();
	}

	public virtual void Update()
	{
		if (Time.timeScale != 0f && GameManager.Instance.PlayerIsNearRoom(m_parentRoom))
		{
			m_stateTimer = Mathf.Max(0f, m_stateTimer - BraveTime.DeltaTime) * LocalTimeScale;
			m_triggerTimer -= BraveTime.DeltaTime * LocalTimeScale;
			m_disabledTimer = Mathf.Max(0f, m_disabledTimer - BraveTime.DeltaTime * LocalTimeScale);
			if (triggerMethod == TriggerMethod.Timer && m_triggerTimer < 0f)
			{
				TriggerTrap(null);
			}
			UpdateState();
		}
	}

	protected override void OnDestroy()
	{
		if (triggerOnBlank || triggerOnExplosion)
		{
			StaticReferenceManager.AllTriggeredTraps.Remove(this);
		}
		base.OnDestroy();
	}

	public override GameObject InstantiateObject(RoomHandler targetRoom, IntVector2 loc, bool deferConfiguration = false)
	{
		return base.InstantiateObject(targetRoom, loc, deferConfiguration);
	}

	private void OnTriggerCollision(SpeculativeRigidbody rigidbody, SpeculativeRigidbody source, CollisionData collisionData)
	{
		PlayerController component = rigidbody.GetComponent<PlayerController>();
		if ((bool)component)
		{
			bool flag = component.spriteAnimator.QueryGroundedFrame() && !component.IsFlying;
			if (triggerMethod == TriggerMethod.SpecRigidbody && m_state == State.Ready && flag)
			{
				TriggerTrap(rigidbody);
			}
			if (damageMethod == DamageMethod.SpecRigidbody && m_state == State.Active && (flag || damagesFlyingPlayers))
			{
				Damage(rigidbody);
			}
		}
	}

	public void Trigger()
	{
		TriggerTrap(null);
	}

	protected virtual void TriggerTrap(SpeculativeRigidbody target)
	{
		if (!(m_disabledTimer > 0f) && m_state == State.Ready)
		{
			state = State.Triggered;
			if (damageMethod == DamageMethod.OnTrigger)
			{
				Damage(target);
			}
		}
	}

	protected bool ArePlayersNearby()
	{
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			if ((bool)GameManager.Instance.AllPlayers[i] && GameManager.Instance.AllPlayers[i].CurrentRoom == m_parentRoom)
			{
				return true;
			}
		}
		return false;
	}

	protected bool ArePlayersSortOfNearby()
	{
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			if ((bool)GameManager.Instance.AllPlayers[i] && GameManager.Instance.AllPlayers[i].CurrentRoom != null && GameManager.Instance.AllPlayers[i].CurrentRoom.connectedRooms != null && GameManager.Instance.AllPlayers[i].CurrentRoom.connectedRooms.Contains(m_parentRoom))
			{
				return true;
			}
		}
		return false;
	}

	protected virtual void BeginState(State newState)
	{
		bool flag = ArePlayersNearby();
		bool flag2 = flag || ArePlayersSortOfNearby();
		if (m_state == State.Triggered)
		{
			PlayAnimation(triggerAnimName);
			m_stateTimer = triggerDelay;
			if (triggerMethod == TriggerMethod.Timer)
			{
				m_triggerTimer += GetNextTriggerTimerDelay();
			}
			if (m_stateTimer == 0f)
			{
				state = State.Active;
			}
			if (flag)
			{
				AkSoundEngine.PostEvent("Play_ENV_trap_trigger", base.gameObject);
			}
		}
		else if (m_state == State.Active)
		{
			PlayAnimation(activeAnimName);
			if (flag2)
			{
				SpawnVfx(activeVfx);
			}
			m_stateTimer = activeTime;
			if (m_stateTimer == 0f)
			{
				state = State.Resetting;
			}
			if (flag)
			{
				AkSoundEngine.PostEvent("Play_ENV_trap_active", base.gameObject);
			}
		}
		else if (m_state == State.Resetting)
		{
			PlayAnimation(resetAnimName);
			m_stateTimer = resetDelay;
			if (m_stateTimer == 0f)
			{
				state = State.Ready;
			}
			if (flag)
			{
				AkSoundEngine.PostEvent("Play_ENV_trap_reset", base.gameObject);
			}
		}
	}

	protected virtual void UpdateState()
	{
		if (m_state == State.Ready)
		{
			if (triggerMethod != TriggerMethod.PlaceableFootprint)
			{
				return;
			}
			SpeculativeRigidbody playerRigidbodyInFootprint = GetPlayerRigidbodyInFootprint();
			if ((bool)playerRigidbodyInFootprint)
			{
				bool flag = playerRigidbodyInFootprint.spriteAnimator.QueryGroundedFrame();
				if (playerRigidbodyInFootprint.gameActor != null)
				{
					flag = flag && !playerRigidbodyInFootprint.gameActor.IsFlying;
				}
				if (flag)
				{
					TriggerTrap(null);
				}
			}
		}
		else if (m_state == State.Triggered)
		{
			if (m_stateTimer == 0f)
			{
				state = State.Active;
			}
		}
		else if (m_state == State.Active)
		{
			if (damageMethod == DamageMethod.PlaceableFootprint)
			{
				SpeculativeRigidbody playerRigidbodyInFootprint2 = GetPlayerRigidbodyInFootprint();
				if ((bool)playerRigidbodyInFootprint2)
				{
					bool flag2 = playerRigidbodyInFootprint2.spriteAnimator.QueryGroundedFrame();
					if (playerRigidbodyInFootprint2.gameActor != null)
					{
						flag2 = flag2 && !playerRigidbodyInFootprint2.gameActor.IsFlying;
					}
					if (flag2 || damagesFlyingPlayers)
					{
						Damage(playerRigidbodyInFootprint2);
					}
				}
			}
			if (IgnitesGoop)
			{
				DeadlyDeadlyGoopManager.IgniteGoopsCircle(base.sprite.WorldCenter, 1f);
			}
			if (m_stateTimer == 0f)
			{
				state = State.Resetting;
			}
		}
		else if (m_state == State.Resetting && m_stateTimer == 0f)
		{
			state = State.Ready;
		}
	}

	protected virtual void EndState(State newState)
	{
	}

	public void TemporarilyDisableTrap(float disableTime)
	{
		m_disabledTimer = Mathf.Max(disableTime, m_disabledTimer);
	}

	public Vector2 CenterPoint()
	{
		if ((bool)base.specRigidbody)
		{
			return base.specRigidbody.UnitCenter;
		}
		if (triggerMethod == TriggerMethod.PlaceableFootprint)
		{
			return new Vector2(m_cachedPixelMin.x + m_cachedPixelMax.x, m_cachedPixelMin.y + m_cachedPixelMax.y) / 32f;
		}
		return base.transform.position;
	}

	protected virtual void PlayAnimation(string animationName)
	{
		if (string.IsNullOrEmpty(animationName))
		{
			return;
		}
		if (animateChildren)
		{
			if (m_childrenAnimators == null)
			{
				return;
			}
			for (int i = 0; i < m_childrenAnimators.Length; i++)
			{
				if (base.spriteAnimator != m_childrenAnimators[i])
				{
					m_childrenAnimators[i].Play(animationName);
				}
			}
		}
		else
		{
			base.spriteAnimator.Play(animationName);
		}
	}

	protected virtual void SpawnVfx(List<SpriteAnimatorKiller> vfx)
	{
		for (int i = 0; i < vfx.Count; i++)
		{
			if ((bool)vfx[i])
			{
				vfx[i].Restart();
			}
		}
	}

	protected virtual SpeculativeRigidbody GetPlayerRigidbodyInFootprint()
	{
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			PlayerController playerController = GameManager.Instance.AllPlayers[i];
			if (!(playerController == null))
			{
				PixelCollider primaryPixelCollider = playerController.specRigidbody.PrimaryPixelCollider;
				if (primaryPixelCollider != null && m_cachedPixelMin.x <= primaryPixelCollider.MaxX && m_cachedPixelMax.x >= primaryPixelCollider.MinX && m_cachedPixelMin.y <= primaryPixelCollider.MaxY && m_cachedPixelMax.y >= primaryPixelCollider.MinY)
				{
					return playerController.specRigidbody;
				}
			}
		}
		return null;
	}

	protected virtual void Damage(SpeculativeRigidbody rigidbody)
	{
		if (damage > 0f && (bool)rigidbody && (bool)rigidbody.healthHaver && rigidbody.healthHaver.IsVulnerable && (!rigidbody.gameActor || !rigidbody.gameActor.IsFalling))
		{
			rigidbody.healthHaver.ApplyDamage(damage, Vector2.zero, StringTableManager.GetEnemiesString("#TRAP"), damageTypes);
		}
	}

	protected float GetNextTriggerTimerDelay()
	{
		float result = m_triggerTimerDelayArray[m_triggerTimerDelayIndex];
		m_triggerTimerDelayIndex = (m_triggerTimerDelayIndex + 1) % m_triggerTimerDelayArray.Count;
		return result;
	}

	public void ConfigureOnPlacement(RoomHandler room)
	{
		IntVector2 intVector = base.transform.position.IntXY(VectorConversions.Floor);
		for (int i = 0; i < placeableWidth; i++)
		{
			for (int j = 0; j < placeableHeight; j++)
			{
				IntVector2 key = new IntVector2(i, j) + intVector;
				GameManager.Instance.Dungeon.data[key].cellVisualData.containsObjectSpaceStamp = true;
				GameManager.Instance.Dungeon.data[key].cellVisualData.containsWallSpaceStamp = true;
			}
		}
		room.ForcePreventChannels = true;
	}
}
