using System.Collections.Generic;
using FullInspector;
using Pathfinding;
using UnityEngine;

public class SpectreTeleportBehavior : BasicAttackBehavior
{
	public enum ShadowSupport
	{
		None,
		Fade,
		Animate
	}

	private enum TeleportState
	{
		None,
		TeleportOut,
		Gone1,
		HauntIn,
		Haunt,
		HauntOut,
		Gone2,
		TeleportIn
	}

	private class SpecterInfo
	{
		public AIAnimator aiAnimator;

		public tk2dBaseSprite shadowSprite;

		public GameObject gameObject
		{
			get
			{
				return aiAnimator.gameObject;
			}
		}

		public Transform transform
		{
			get
			{
				return aiAnimator.transform;
			}
		}

		public Renderer renderer
		{
			get
			{
				return aiAnimator.renderer;
			}
		}

		public SpeculativeRigidbody specRigidbody
		{
			get
			{
				return aiAnimator.specRigidbody;
			}
		}

		public tk2dBaseSprite sprite
		{
			get
			{
				return aiAnimator.sprite;
			}
		}
	}

	public bool AttackableDuringAnimation;

	public bool AvoidWalls;

	public float GoneTime = 1f;

	public float HauntTime = 1f;

	public float HauntDistance = 5f;

	public List<AIAnimator> HauntCopies;

	[InspectorCategory("Attack")]
	public GameObject ShootPoint;

	[InspectorCategory("Attack")]
	public BulletScriptSelector hauntBulletScript;

	[InspectorCategory("Visuals")]
	public string teleportOutAnim = "teleport_out";

	[InspectorCategory("Visuals")]
	public string teleportInAttackAnim = "teleport_in";

	[InspectorCategory("Visuals")]
	public string teleportOutAttackAnim = "teleport_out";

	[InspectorCategory("Visuals")]
	public string teleportInAnim = "teleport_in";

	[InspectorCategory("Visuals")]
	public bool teleportRequiresTransparency;

	[InspectorCategory("Visuals")]
	public ShadowSupport shadowSupport;

	[InspectorCategory("Visuals")]
	[InspectorShowIf("ShowShadowAnimationNames")]
	public string shadowOutAnim;

	[InspectorCategory("Visuals")]
	[InspectorShowIf("ShowShadowAnimationNames")]
	public string shadowInAnim;

	private Shader m_cachedShader;

	private List<SpecterInfo> m_allSpectres;

	private BulletScriptSource m_bulletSource;

	private float m_timer;

	private float m_hauntAngle;

	private Vector2 m_centerOffset;

	private TeleportState m_state;

	private TeleportState State
	{
		get
		{
			return m_state;
		}
		set
		{
			EndState(m_state);
			m_state = value;
			BeginState(m_state);
		}
	}

	private bool ShowShadowAnimationNames()
	{
		return shadowSupport == ShadowSupport.Animate;
	}

	public override void Start()
	{
		base.Start();
		PhysicsEngine.Instance.OnPostRigidbodyMovement += OnPostRigidbodyMovement;
		for (int i = 0; i < HauntCopies.Count; i++)
		{
			HauntCopies[i].aiActor = m_aiActor;
			HauntCopies[i].healthHaver = m_aiActor.healthHaver;
		}
	}

	public override void Upkeep()
	{
		base.Upkeep();
		DecrementTimer(ref m_timer);
	}

	public override BehaviorResult Update()
	{
		base.Update();
		if (m_allSpectres == null)
		{
			m_allSpectres = new List<SpecterInfo>(HauntCopies.Count + 1);
			tk2dBaseSprite component = m_aiActor.ShadowObject.GetComponent<tk2dBaseSprite>();
			m_allSpectres.Add(new SpecterInfo
			{
				aiAnimator = m_aiAnimator,
				shadowSprite = component
			});
			for (int i = 0; i < HauntCopies.Count; i++)
			{
				tk2dBaseSprite tk2dBaseSprite2 = Object.Instantiate(component);
				tk2dBaseSprite2.transform.parent = HauntCopies[i].transform;
				tk2dBaseSprite2.transform.localPosition = component.transform.localPosition;
				HauntCopies[i].sprite.AttachRenderer(tk2dBaseSprite2);
				tk2dBaseSprite2.UpdateZDepth();
				m_allSpectres.Add(new SpecterInfo
				{
					aiAnimator = HauntCopies[i],
					shadowSprite = tk2dBaseSprite2
				});
			}
		}
		if (!IsReady())
		{
			return BehaviorResult.Continue;
		}
		if (!m_aiActor.TargetRigidbody)
		{
			return BehaviorResult.Continue;
		}
		State = TeleportState.TeleportOut;
		m_updateEveryFrame = true;
		return BehaviorResult.RunContinuous;
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		base.ContinuousUpdate();
		if (State == TeleportState.TeleportOut)
		{
			if (shadowSupport == ShadowSupport.Fade)
			{
				for (int i = 0; i < m_allSpectres.Count; i++)
				{
					if ((bool)m_allSpectres[i].shadowSprite)
					{
						m_allSpectres[i].shadowSprite.color = m_allSpectres[i].shadowSprite.color.WithAlpha(1f - m_aiAnimator.CurrentClipProgress);
					}
				}
			}
			if (!m_aiAnimator.IsPlaying(teleportOutAnim))
			{
				State = ((!(GoneTime > 0f)) ? TeleportState.HauntIn : TeleportState.Gone1);
			}
		}
		else if (State == TeleportState.Gone1)
		{
			if (m_timer <= 0f)
			{
				State = TeleportState.HauntIn;
			}
		}
		else if (State == TeleportState.HauntIn)
		{
			if (shadowSupport == ShadowSupport.Fade)
			{
				for (int j = 0; j < m_allSpectres.Count; j++)
				{
					if ((bool)m_allSpectres[j].shadowSprite)
					{
						m_allSpectres[j].shadowSprite.color = m_allSpectres[j].shadowSprite.color.WithAlpha(m_aiAnimator.CurrentClipProgress);
					}
				}
			}
			m_aiShooter.ToggleGunAndHandRenderers(false, "SpectreTeleportBehavior");
			if (!m_aiAnimator.IsPlaying(teleportInAttackAnim))
			{
				State = TeleportState.Haunt;
			}
		}
		else if (State == TeleportState.Haunt)
		{
			if (m_timer <= 0f)
			{
				State = TeleportState.HauntOut;
			}
		}
		else if (State == TeleportState.HauntOut)
		{
			if (shadowSupport == ShadowSupport.Fade)
			{
				for (int k = 0; k < m_allSpectres.Count; k++)
				{
					if ((bool)m_allSpectres[k].shadowSprite)
					{
						m_allSpectres[k].shadowSprite.color = m_allSpectres[k].shadowSprite.color.WithAlpha(1f - m_aiAnimator.CurrentClipProgress);
					}
				}
			}
			if (!m_aiAnimator.IsPlaying(teleportOutAttackAnim))
			{
				State = ((!(GoneTime > 0f)) ? TeleportState.TeleportIn : TeleportState.Gone2);
			}
		}
		else if (State == TeleportState.Gone2)
		{
			if (m_timer <= 0f)
			{
				State = TeleportState.TeleportIn;
			}
		}
		else if (State == TeleportState.TeleportIn)
		{
			if (shadowSupport == ShadowSupport.Fade)
			{
				for (int l = 0; l < m_allSpectres.Count; l++)
				{
					if ((bool)m_allSpectres[l].shadowSprite)
					{
						m_allSpectres[l].shadowSprite.color = m_allSpectres[l].shadowSprite.color.WithAlpha(m_aiAnimator.CurrentClipProgress);
					}
				}
			}
			m_aiShooter.ToggleGunAndHandRenderers(false, "SpectreTeleportBehavior");
			if (!m_aiAnimator.IsPlaying(teleportInAnim))
			{
				State = TeleportState.None;
				return ContinuousBehaviorResult.Finished;
			}
		}
		return ContinuousBehaviorResult.Continue;
	}

	public override void EndContinuousUpdate()
	{
		base.EndContinuousUpdate();
		m_updateEveryFrame = false;
		UpdateCooldowns();
	}

	public override bool IsOverridable()
	{
		return false;
	}

	public override void OnActorPreDeath()
	{
		for (int i = 0; i < HauntCopies.Count; i++)
		{
			HauntCopies[i].PlayUntilCancelled("die", true);
		}
		PhysicsEngine.Instance.OnPostRigidbodyMovement -= OnPostRigidbodyMovement;
		base.OnActorPreDeath();
	}

	public void OnPostRigidbodyMovement()
	{
		if (State == TeleportState.HauntIn || State == TeleportState.Haunt || State == TeleportState.HauntOut)
		{
			Vector2 unitCenter = m_aiActor.TargetRigidbody.GetUnitCenter(ColliderType.HitBox);
			float num = 360f / (float)m_allSpectres.Count;
			for (int i = 0; i < m_allSpectres.Count; i++)
			{
				Vector2 vector = BraveMathCollege.DegreesToVector(m_hauntAngle + (float)i * num, HauntDistance);
				m_allSpectres[i].transform.position = unitCenter + vector + m_centerOffset;
				m_allSpectres[i].specRigidbody.Reinitialize();
			}
		}
	}

	private void BeginState(TeleportState state)
	{
		switch (state)
		{
		case TeleportState.TeleportOut:
		case TeleportState.HauntOut:
		{
			if (teleportRequiresTransparency)
			{
				m_cachedShader = m_aiActor.renderer.material.shader;
			}
			for (int m = 0; m < m_allSpectres.Count; m++)
			{
				if (teleportRequiresTransparency)
				{
					m_allSpectres[m].sprite.usesOverrideMaterial = true;
					m_allSpectres[m].renderer.material.shader = ShaderCache.Acquire("Brave/LitBlendUber");
				}
				string name = ((state != TeleportState.TeleportOut) ? teleportOutAttackAnim : teleportOutAnim);
				m_allSpectres[m].aiAnimator.PlayUntilCancelled(name, true);
				if (shadowSupport == ShadowSupport.Animate && (bool)m_allSpectres[m].shadowSprite)
				{
					m_allSpectres[m].shadowSprite.spriteAnimator.PlayAndForceTime(shadowOutAnim, m_aiAnimator.CurrentClipLength);
				}
				if (!AttackableDuringAnimation)
				{
					m_allSpectres[m].specRigidbody.CollideWithOthers = false;
				}
			}
			m_aiShooter.ToggleGunAndHandRenderers(false, "SpectreTeleportBehavior");
			m_aiActor.ClearPath();
			break;
		}
		case TeleportState.Gone1:
		case TeleportState.Gone2:
		{
			m_timer = GoneTime;
			for (int k = 0; k < m_allSpectres.Count; k++)
			{
				m_allSpectres[k].specRigidbody.CollideWithOthers = false;
				m_allSpectres[k].sprite.renderer.enabled = false;
			}
			break;
		}
		case TeleportState.Haunt:
		{
			Fire();
			m_timer = HauntTime;
			for (int l = 0; l < m_allSpectres.Count; l++)
			{
				m_allSpectres[l].specRigidbody.CollideWithOthers = true;
				m_allSpectres[l].specRigidbody.CollideWithTileMap = false;
				m_allSpectres[l].aiAnimator.LockFacingDirection = true;
				m_allSpectres[l].aiAnimator.FacingDirection = -90f;
			}
			break;
		}
		case TeleportState.HauntIn:
		case TeleportState.TeleportIn:
		{
			if (state == TeleportState.TeleportIn)
			{
				DoTeleport();
				m_aiAnimator.PlayUntilFinished(teleportInAnim, true);
			}
			else
			{
				Vector2 unitCenter = m_aiActor.TargetRigidbody.GetUnitCenter(ColliderType.HitBox);
				m_hauntAngle = Random.Range(0, 360);
				m_centerOffset = m_aiActor.transform.position.XY() - m_aiActor.specRigidbody.GetUnitCenter(ColliderType.HitBox);
				float num = 360f / (float)m_allSpectres.Count;
				for (int i = 0; i < m_allSpectres.Count; i++)
				{
					Vector2 vector = BraveMathCollege.DegreesToVector(m_hauntAngle + (float)i * num, HauntDistance);
					if (i > 0)
					{
						m_allSpectres[i].gameObject.SetActive(true);
						m_allSpectres[i].specRigidbody.enabled = true;
					}
					m_allSpectres[i].transform.position = unitCenter + vector + m_centerOffset;
					m_allSpectres[i].specRigidbody.Reinitialize();
					m_allSpectres[i].aiAnimator.PlayUntilFinished(teleportInAttackAnim, true);
				}
			}
			for (int j = 0; j < m_allSpectres.Count; j++)
			{
				if ((bool)m_allSpectres[j].shadowSprite)
				{
					if (shadowSupport == ShadowSupport.Animate)
					{
						m_allSpectres[j].shadowSprite.spriteAnimator.PlayAndForceTime(shadowInAnim, m_aiAnimator.CurrentClipLength);
					}
					m_allSpectres[j].shadowSprite.renderer.enabled = true;
				}
				if (AttackableDuringAnimation)
				{
					m_allSpectres[j].specRigidbody.CollideWithOthers = true;
				}
				m_allSpectres[j].sprite.renderer.enabled = true;
				SpriteOutlineManager.ToggleOutlineRenderers(m_allSpectres[j].sprite, true);
			}
			break;
		}
		}
	}

	private void EndState(TeleportState state)
	{
		switch (state)
		{
		case TeleportState.TeleportOut:
		case TeleportState.HauntOut:
		{
			for (int j = 0; j < m_allSpectres.Count; j++)
			{
				if ((bool)m_allSpectres[j].shadowSprite)
				{
					m_allSpectres[j].renderer.enabled = false;
				}
				SpriteOutlineManager.ToggleOutlineRenderers(m_allSpectres[j].sprite, false);
			}
			if (state == TeleportState.HauntOut)
			{
				for (int k = 1; k < m_allSpectres.Count; k++)
				{
					m_allSpectres[k].gameObject.SetActive(false);
					m_allSpectres[k].specRigidbody.enabled = false;
				}
			}
			break;
		}
		case TeleportState.HauntIn:
		case TeleportState.TeleportIn:
		{
			for (int l = 0; l < m_allSpectres.Count; l++)
			{
				if (teleportRequiresTransparency)
				{
					m_allSpectres[l].sprite.usesOverrideMaterial = false;
					m_allSpectres[l].renderer.material.shader = m_cachedShader;
				}
				if (shadowSupport == ShadowSupport.Fade && (bool)m_allSpectres[l].shadowSprite)
				{
					m_allSpectres[l].shadowSprite.color = m_allSpectres[l].shadowSprite.color.WithAlpha(1f);
				}
				m_allSpectres[l].specRigidbody.CollideWithOthers = true;
			}
			if (state == TeleportState.TeleportIn)
			{
				m_aiShooter.ToggleGunAndHandRenderers(true, "SpectreTeleportBehavior");
			}
			break;
		}
		case TeleportState.Haunt:
		{
			for (int i = 0; i < m_allSpectres.Count; i++)
			{
				m_allSpectres[i].specRigidbody.CollideWithTileMap = true;
				m_allSpectres[i].aiAnimator.LockFacingDirection = false;
			}
			break;
		}
		}
	}

	private void Fire()
	{
		if (!m_bulletSource)
		{
			m_bulletSource = ShootPoint.GetOrAddComponent<BulletScriptSource>();
		}
		m_bulletSource.BulletManager = m_aiActor.bulletBank;
		m_bulletSource.BulletScript = hauntBulletScript;
		m_bulletSource.Initialize();
	}

	private void DoTeleport()
	{
		IntVector2? targetCenter = null;
		if ((bool)m_aiActor.TargetRigidbody)
		{
			targetCenter = m_aiActor.TargetRigidbody.UnitCenter.ToIntVector2(VectorConversions.Floor);
		}
		Vector2 vector = BraveUtility.ViewportToWorldpoint(new Vector2(0f, 0f), ViewportType.Gameplay);
		Vector2 vector2 = BraveUtility.ViewportToWorldpoint(new Vector2(1f, 1f), ViewportType.Gameplay);
		IntVector2 bottomLeft = vector.ToIntVector2(VectorConversions.Ceil);
		IntVector2 topRight = vector2.ToIntVector2(VectorConversions.Floor) - IntVector2.One;
		CellValidator cellValidator = delegate(IntVector2 c)
		{
			for (int i = 0; i < m_aiActor.Clearance.x; i++)
			{
				for (int j = 0; j < m_aiActor.Clearance.y; j++)
				{
					if (GameManager.Instance.Dungeon.data.isTopWall(c.x + i, c.y + j))
					{
						return false;
					}
					if (State == TeleportState.TeleportIn && targetCenter.HasValue && IntVector2.DistanceSquared(targetCenter.Value, c.x + i, c.y + j) < 16f)
					{
						return false;
					}
					if (State == TeleportState.HauntIn && targetCenter.HasValue && IntVector2.DistanceSquared(targetCenter.Value, c.x + i, c.y + j) > 4f)
					{
						return false;
					}
				}
			}
			if (c.x < bottomLeft.x || c.y < bottomLeft.y || c.x + m_aiActor.Clearance.x - 1 > topRight.x || c.y + m_aiActor.Clearance.y - 1 > topRight.y)
			{
				return false;
			}
			if (AvoidWalls)
			{
				int num = -1;
				int k;
				for (k = -1; k < m_aiActor.Clearance.y + 1; k++)
				{
					if (GameManager.Instance.Dungeon.data.isWall(c.x + num, c.y + k))
					{
						return false;
					}
				}
				num = m_aiActor.Clearance.x;
				for (k = -1; k < m_aiActor.Clearance.y + 1; k++)
				{
					if (GameManager.Instance.Dungeon.data.isWall(c.x + num, c.y + k))
					{
						return false;
					}
				}
				k = -1;
				for (num = -1; num < m_aiActor.Clearance.x + 1; num++)
				{
					if (GameManager.Instance.Dungeon.data.isWall(c.x + num, c.y + k))
					{
						return false;
					}
				}
				k = m_aiActor.Clearance.y;
				for (num = -1; num < m_aiActor.Clearance.x + 1; num++)
				{
					if (GameManager.Instance.Dungeon.data.isWall(c.x + num, c.y + k))
					{
						return false;
					}
				}
			}
			return true;
		};
		Vector2 vector3 = m_aiActor.specRigidbody.UnitCenter - m_aiActor.transform.position.XY();
		IntVector2? randomAvailableCell = m_aiActor.ParentRoom.GetRandomAvailableCell(m_aiActor.Clearance, m_aiActor.PathableTiles, false, cellValidator);
		if (randomAvailableCell.HasValue)
		{
			m_aiActor.transform.position = Pathfinder.GetClearanceOffset(randomAvailableCell.Value, m_aiActor.Clearance) - vector3;
			m_aiActor.specRigidbody.Reinitialize();
		}
		else
		{
			Debug.LogWarning("TELEPORT FAILED!", m_aiActor);
		}
	}
}
