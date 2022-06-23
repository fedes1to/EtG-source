using System.Collections.Generic;
using Dungeonator;
using UnityEngine;
using UnityEngine.Serialization;

public class RaidenBeamController : BeamController
{
	public enum TargetType
	{
		Screen = 10,
		Room = 20
	}

	private class Bone
	{
		public Vector2 pos;

		public Vector2 normal;

		public Bone(Vector2 pos)
		{
			this.pos = pos;
		}
	}

	[FormerlySerializedAs("animation")]
	public string beamAnimation;

	public bool usesStartAnimation;

	public string startAnimation;

	public tk2dBaseSprite ImpactRenderer;

	[CheckAnimation(null)]
	public string EnemyImpactAnimation;

	[CheckAnimation(null)]
	public string BossImpactAnimation;

	[CheckAnimation(null)]
	public string OtherImpactAnimation;

	public TargetType targetType = TargetType.Screen;

	public int maxTargets = -1;

	public float endRampHeight;

	public int endRampSteps;

	[HideInInspector]
	public bool FlipUvsY;

	[HideInInspector]
	public bool SelectRandomTarget;

	private List<AIActor> s_enemiesInRoom = new List<AIActor>();

	private tk2dTiledSprite m_sprite;

	private tk2dSpriteAnimationClip m_startAnimationClip;

	private tk2dSpriteAnimationClip m_animationClip;

	private bool m_isCurrentlyFiring = true;

	private bool m_audioPlaying;

	private List<AIActor> m_targets = new List<AIActor>();

	private SpeculativeRigidbody m_hitRigidbody;

	private int m_spriteSubtileWidth;

	private LinkedList<Bone> m_bones = new LinkedList<Bone>();

	private Vector2 m_minBonePosition;

	private Vector2 m_maxBonePosition;

	private bool m_isDirty;

	private float m_globalTimer;

	private const int c_segmentCount = 20;

	private const int c_bonePixelLength = 4;

	private const float c_boneUnitLength = 0.25f;

	private const float c_trailHeightOffset = 0.5f;

	private float m_projectileScale = 1f;

	public override bool ShouldUseAmmo
	{
		get
		{
			return true;
		}
	}

	public float RampHeightOffset { get; set; }

	public void Start()
	{
		base.transform.parent = SpawnManager.Instance.VFX;
		base.transform.rotation = Quaternion.identity;
		base.transform.position = Vector3.zero;
		m_sprite = GetComponent<tk2dTiledSprite>();
		m_sprite.OverrideGetTiledSpriteGeomDesc = GetTiledSpriteGeomDesc;
		m_sprite.OverrideSetTiledSpriteGeom = SetTiledSpriteGeom;
		tk2dSpriteDefinition currentSpriteDef = m_sprite.GetCurrentSpriteDef();
		m_spriteSubtileWidth = Mathf.RoundToInt(currentSpriteDef.untrimmedBoundsDataExtents.x / currentSpriteDef.texelSize.x) / 4;
		if (usesStartAnimation)
		{
			m_startAnimationClip = base.spriteAnimator.GetClipByName(startAnimation);
		}
		m_animationClip = base.spriteAnimator.GetClipByName(beamAnimation);
		PlayerController playerController = base.projectile.Owner as PlayerController;
		if ((bool)playerController)
		{
			m_projectileScale = playerController.BulletScaleModifier;
		}
		if ((bool)ImpactRenderer)
		{
			ImpactRenderer.transform.localScale = new Vector3(m_projectileScale, m_projectileScale, 1f);
		}
	}

	public void Update()
	{
		m_globalTimer += BraveTime.DeltaTime;
		for (int num = m_targets.Count - 1; num >= 0; num--)
		{
			AIActor aIActor = m_targets[num];
			if (!aIActor || !aIActor.healthHaver || aIActor.healthHaver.IsDead)
			{
				m_targets.RemoveAt(num);
			}
		}
		m_hitRigidbody = null;
		HandleBeamFrame(base.Origin, base.Direction, m_isCurrentlyFiring);
		if (m_targets == null || m_targets.Count == 0)
		{
			if (GameManager.AUDIO_ENABLED && m_audioPlaying)
			{
				m_audioPlaying = false;
				AkSoundEngine.PostEvent("Stop_WPN_loop_01", base.gameObject);
			}
		}
		else if (GameManager.AUDIO_ENABLED && !m_audioPlaying)
		{
			m_audioPlaying = true;
			AkSoundEngine.PostEvent("Play_WPN_shot_01", base.gameObject);
		}
		float num2 = base.projectile.baseData.damage + base.DamageModifier;
		PlayerController playerController = base.projectile.Owner as PlayerController;
		if ((bool)playerController)
		{
			num2 *= playerController.stats.GetStatValue(PlayerStats.StatType.RateOfFire);
			num2 *= playerController.stats.GetStatValue(PlayerStats.StatType.Damage);
		}
		if (base.ChanceBasedShadowBullet)
		{
			num2 *= 2f;
		}
		string value = OtherImpactAnimation;
		if (m_targets != null && m_targets.Count > 0)
		{
			foreach (AIActor target in m_targets)
			{
				if ((bool)target && (bool)target.healthHaver)
				{
					value = ((string.IsNullOrEmpty(BossImpactAnimation) || !target.healthHaver.IsBoss) ? EnemyImpactAnimation : BossImpactAnimation);
					if (target.healthHaver.IsBoss && (bool)base.projectile)
					{
						num2 *= base.projectile.BossDamageMultiplier;
					}
					if ((bool)base.projectile && base.projectile.BlackPhantomDamageMultiplier != 1f && target.IsBlackPhantom)
					{
						num2 *= base.projectile.BlackPhantomDamageMultiplier;
					}
					target.healthHaver.ApplyDamage(num2 * BraveTime.DeltaTime, Vector2.zero, base.Owner.ActorName);
				}
			}
		}
		if ((bool)m_hitRigidbody)
		{
			if ((bool)m_hitRigidbody.minorBreakable)
			{
				m_hitRigidbody.minorBreakable.Break(base.Direction);
			}
			if ((bool)m_hitRigidbody.majorBreakable)
			{
				m_hitRigidbody.majorBreakable.ApplyDamage(num2 * BraveTime.DeltaTime, base.Direction, false);
			}
		}
		if ((bool)ImpactRenderer && (bool)ImpactRenderer.spriteAnimator && !string.IsNullOrEmpty(value))
		{
			ImpactRenderer.spriteAnimator.Play(value);
		}
	}

	public void LateUpdate()
	{
		if (m_isDirty)
		{
			m_minBonePosition = new Vector2(float.MaxValue, float.MaxValue);
			m_maxBonePosition = new Vector2(float.MinValue, float.MinValue);
			for (LinkedListNode<Bone> linkedListNode = m_bones.First; linkedListNode != null; linkedListNode = linkedListNode.Next)
			{
				m_minBonePosition = Vector2.Min(m_minBonePosition, linkedListNode.Value.pos);
				m_maxBonePosition = Vector2.Max(m_maxBonePosition, linkedListNode.Value.pos);
			}
			Vector2 vector = new Vector2(m_minBonePosition.x, m_minBonePosition.y) - base.transform.position.XY();
			base.transform.position = new Vector3(m_minBonePosition.x, m_minBonePosition.y);
			m_sprite.HeightOffGround = 0.5f;
			ImpactRenderer.transform.position -= vector.ToVector3ZUp();
			m_sprite.ForceBuild();
			m_sprite.UpdateZDepth();
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public void HandleBeamFrame(Vector2 barrelPosition, Vector2 direction, bool isCurrentlyFiring)
	{
		if (base.Owner is PlayerController)
		{
			HandleChanceTick();
		}
		if (targetType == TargetType.Screen)
		{
			m_targets.Clear();
			List<AIActor> allEnemies = StaticReferenceManager.AllEnemies;
			for (int i = 0; i < allEnemies.Count; i++)
			{
				AIActor aIActor = allEnemies[i];
				if (aIActor.IsNormalEnemy && aIActor.renderer.isVisible && aIActor.healthHaver.IsAlive && !aIActor.IsGone)
				{
					m_targets.Add(aIActor);
				}
				if (maxTargets > 0 && m_targets.Count >= maxTargets)
				{
					break;
				}
			}
		}
		else if (maxTargets <= 0 || m_targets.Count < maxTargets)
		{
			RoomHandler absoluteRoomFromPosition = GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(barrelPosition.ToIntVector2(VectorConversions.Floor));
			absoluteRoomFromPosition.GetActiveEnemies(RoomHandler.ActiveEnemyType.All, ref s_enemiesInRoom);
			if (SelectRandomTarget)
			{
				s_enemiesInRoom = s_enemiesInRoom.Shuffle();
			}
			else
			{
				s_enemiesInRoom.Sort((AIActor a, AIActor b) => Vector2.Distance(barrelPosition, a.CenterPosition).CompareTo(Vector2.Distance(barrelPosition, b.CenterPosition)));
			}
			for (int j = 0; j < s_enemiesInRoom.Count; j++)
			{
				AIActor aIActor2 = s_enemiesInRoom[j];
				if (aIActor2.IsNormalEnemy && aIActor2.renderer.isVisible && aIActor2.healthHaver.IsAlive && !aIActor2.IsGone)
				{
					m_targets.Add(aIActor2);
				}
				if (maxTargets > 0 && m_targets.Count >= maxTargets)
				{
					break;
				}
			}
		}
		m_bones.Clear();
		Vector3? vector = null;
		if (m_targets.Count > 0)
		{
			Vector3 vector2 = direction.normalized * 5f;
			Vector3 vector3 = barrelPosition;
			Vector3 vector4 = vector3 + vector2;
			vector2 = Quaternion.Euler(0f, 0f, 180f) * vector2;
			Vector3 vector5 = m_targets[0].specRigidbody.HitboxPixelCollider.UnitCenter;
			Vector3 vector6 = vector5 + vector2;
			vector2 = Quaternion.Euler(0f, 0f, 180f) * vector2;
			DrawBezierCurve(vector3, vector4, vector6, vector5);
			for (int k = 0; k < m_targets.Count - 1; k++)
			{
				vector3 = m_targets[k].specRigidbody.HitboxPixelCollider.UnitCenter;
				vector4 = vector3 + vector2;
				vector2 = Quaternion.Euler(0f, 0f, 90f) * vector2;
				vector5 = m_targets[k + 1].specRigidbody.HitboxPixelCollider.UnitCenter;
				vector6 = vector5 + vector2;
				vector2 = Quaternion.Euler(0f, 0f, 180f) * vector2;
				DrawBezierCurve(vector3, vector4, vector6, vector5);
			}
			if ((bool)ImpactRenderer)
			{
				ImpactRenderer.renderer.enabled = false;
			}
		}
		else
		{
			Vector3 vector7 = Quaternion.Euler(0f, 0f, Mathf.PingPong(Time.realtimeSinceStartup * 15f, 60f) - 30f) * direction.normalized * 5f;
			Vector3 vector8 = barrelPosition;
			Vector3 vector9 = vector8 + vector7;
			vector7 = Quaternion.Euler(0f, 0f, 180f) * vector7;
			int mask = CollisionLayerMatrix.GetMask(CollisionLayer.Projectile);
			mask |= CollisionMask.LayerToMask(CollisionLayer.BeamBlocker);
			mask &= ~CollisionMask.LayerToMask(CollisionLayer.PlayerCollider, CollisionLayer.PlayerHitBox);
			PhysicsEngine instance = PhysicsEngine.Instance;
			Vector2 unitOrigin = vector8;
			Vector2 direction2 = direction;
			float dist = 30f;
			bool collideWithTiles = true;
			bool collideWithRigidbodies = true;
			int rayMask = mask;
			CollisionLayer? sourceLayer = null;
			bool collideWithTriggers = false;
			SpeculativeRigidbody[] ignoreRigidbodies = GetIgnoreRigidbodies();
			RaycastResult result;
			bool flag = instance.RaycastWithIgnores(unitOrigin, direction2, dist, out result, collideWithTiles, collideWithRigidbodies, rayMask, sourceLayer, collideWithTriggers, null, ignoreRigidbodies);
			Vector3 vector10 = vector8 + (direction.normalized * 30f).ToVector3ZUp();
			if (flag)
			{
				vector10 = result.Contact;
				m_hitRigidbody = result.SpeculativeRigidbody;
			}
			RaycastResult.Pool.Free(ref result);
			vector = vector10;
			Vector3 vector11 = vector10 + vector7;
			vector7 = Quaternion.Euler(0f, 0f, 180f) * vector7;
			DrawBezierCurve(vector8, vector9, vector11, vector10);
			if ((bool)ImpactRenderer)
			{
				ImpactRenderer.renderer.enabled = false;
			}
		}
		LinkedListNode<Bone> linkedListNode = m_bones.First;
		while (linkedListNode != null && linkedListNode != m_bones.Last)
		{
			linkedListNode.Value.normal = (Quaternion.Euler(0f, 0f, 90f) * (linkedListNode.Next.Value.pos - linkedListNode.Value.pos)).normalized;
			linkedListNode = linkedListNode.Next;
		}
		if (m_bones.Count > 0)
		{
			m_bones.Last.Value.normal = m_bones.Last.Previous.Value.normal;
		}
		m_isDirty = true;
		if ((bool)ImpactRenderer)
		{
			if (m_targets.Count == 0)
			{
				ImpactRenderer.renderer.enabled = true;
				ImpactRenderer.transform.position = ((!vector.HasValue) ? (base.Gun.CurrentOwner as PlayerController).unadjustedAimPoint.XY() : vector.Value.XY());
				ImpactRenderer.IsPerpendicular = false;
			}
			else
			{
				ImpactRenderer.renderer.enabled = true;
				ImpactRenderer.transform.position = m_targets[m_targets.Count - 1].CenterPosition;
				ImpactRenderer.IsPerpendicular = true;
			}
			ImpactRenderer.HeightOffGround = 6f;
			ImpactRenderer.UpdateZDepth();
		}
	}

	public override void LateUpdatePosition(Vector3 origin)
	{
	}

	public override void CeaseAttack()
	{
		DestroyBeam();
	}

	public override void DestroyBeam()
	{
		Object.Destroy(base.gameObject);
	}

	public override void AdjustPlayerBeamTint(Color targetTintColor, int priority, float lerpTime = 0f)
	{
	}

	private void DrawBezierCurve(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
	{
		Vector3 vector = BraveMathCollege.CalculateBezierPoint(0f, p0, p1, p2, p3);
		float num = 0f;
		for (int i = 1; i <= 20; i++)
		{
			float t = (float)i / 20f;
			Vector2 vector2 = BraveMathCollege.CalculateBezierPoint(t, p0, p1, p2, p3);
			num += Vector2.Distance(vector, vector2);
			vector = vector2;
		}
		float num2 = num / 0.25f;
		vector = BraveMathCollege.CalculateBezierPoint(0f, p0, p1, p2, p3);
		if (m_bones.Count == 0)
		{
			m_bones.AddLast(new Bone(vector));
		}
		for (int j = 1; (float)j <= num2; j++)
		{
			float t2 = (float)j / num2;
			Vector3 vector3 = BraveMathCollege.CalculateBezierPoint(t2, p0, p1, p2, p3);
			m_bones.AddLast(new Bone(vector3));
		}
	}

	public void GetTiledSpriteGeomDesc(out int numVertices, out int numIndices, tk2dSpriteDefinition spriteDef, Vector2 dimensions)
	{
		int num = Mathf.Max(m_bones.Count - 1, 0);
		numVertices = num * 4;
		numIndices = num * 6;
	}

	public void SetTiledSpriteGeom(Vector3[] pos, Vector2[] uv, int offset, out Vector3 boundsCenter, out Vector3 boundsExtents, tk2dSpriteDefinition spriteDef, Vector3 scale, Vector2 dimensions, tk2dBaseSprite.Anchor anchor, float colliderOffsetZ, float colliderExtentZ)
	{
		int num = Mathf.RoundToInt(spriteDef.untrimmedBoundsDataExtents.x / spriteDef.texelSize.x);
		int num2 = num / 4;
		int num3 = Mathf.Max(m_bones.Count - 1, 0);
		int num4 = Mathf.CeilToInt((float)num3 / (float)num2);
		boundsCenter = (m_minBonePosition + m_maxBonePosition) / 2f;
		boundsExtents = (m_maxBonePosition - m_minBonePosition) / 2f;
		int num5 = 0;
		LinkedListNode<Bone> linkedListNode = m_bones.First;
		int num6 = 0;
		for (int i = 0; i < num4; i++)
		{
			int num7 = 0;
			int num8 = num2 - 1;
			if (i == num4 - 1 && num3 % num2 != 0)
			{
				num8 = num3 % num2 - 1;
			}
			tk2dSpriteDefinition tk2dSpriteDefinition2 = spriteDef;
			if (usesStartAnimation && i == 0)
			{
				int num9 = Mathf.FloorToInt(Mathf.Repeat(m_globalTimer * m_startAnimationClip.fps, m_startAnimationClip.frames.Length));
				tk2dSpriteDefinition2 = m_sprite.Collection.spriteDefinitions[m_startAnimationClip.frames[num9].spriteId];
			}
			else
			{
				int num10 = Mathf.FloorToInt(Mathf.Repeat(m_globalTimer * m_animationClip.fps, m_animationClip.frames.Length));
				tk2dSpriteDefinition2 = m_sprite.Collection.spriteDefinitions[m_animationClip.frames[num10].spriteId];
			}
			float num11 = 0f;
			for (int j = num7; j <= num8; j++)
			{
				float num12 = 1f;
				if (i == num4 - 1 && j == num8)
				{
					num12 = Vector2.Distance(linkedListNode.Next.Value.pos, linkedListNode.Value.pos);
				}
				float num13 = 0f;
				if (endRampHeight != 0f)
				{
				}
				int num14 = offset + num6;
				pos[num14++] = (linkedListNode.Value.pos + linkedListNode.Value.normal * (tk2dSpriteDefinition2.position0.y * m_projectileScale) - m_minBonePosition).ToVector3ZUp(0f - num13);
				pos[num14++] = (linkedListNode.Next.Value.pos + linkedListNode.Next.Value.normal * (tk2dSpriteDefinition2.position1.y * m_projectileScale) - m_minBonePosition).ToVector3ZUp(0f - num13);
				pos[num14++] = (linkedListNode.Value.pos + linkedListNode.Value.normal * (tk2dSpriteDefinition2.position2.y * m_projectileScale) - m_minBonePosition).ToVector3ZUp(0f - num13);
				pos[num14++] = (linkedListNode.Next.Value.pos + linkedListNode.Next.Value.normal * (tk2dSpriteDefinition2.position3.y * m_projectileScale) - m_minBonePosition).ToVector3ZUp(0f - num13);
				Vector2 vector = Vector2.Lerp(tk2dSpriteDefinition2.uvs[0], tk2dSpriteDefinition2.uvs[1], num11);
				Vector2 vector2 = Vector2.Lerp(tk2dSpriteDefinition2.uvs[2], tk2dSpriteDefinition2.uvs[3], num11 + num12 / (float)num2);
				num14 = offset + num6;
				if (tk2dSpriteDefinition2.flipped == tk2dSpriteDefinition.FlipMode.Tk2d)
				{
					uv[num14++] = new Vector2(vector.x, vector.y);
					uv[num14++] = new Vector2(vector.x, vector2.y);
					uv[num14++] = new Vector2(vector2.x, vector.y);
					uv[num14++] = new Vector2(vector2.x, vector2.y);
				}
				else if (tk2dSpriteDefinition2.flipped == tk2dSpriteDefinition.FlipMode.TPackerCW)
				{
					uv[num14++] = new Vector2(vector.x, vector.y);
					uv[num14++] = new Vector2(vector2.x, vector.y);
					uv[num14++] = new Vector2(vector.x, vector2.y);
					uv[num14++] = new Vector2(vector2.x, vector2.y);
				}
				else
				{
					uv[num14++] = new Vector2(vector.x, vector.y);
					uv[num14++] = new Vector2(vector2.x, vector.y);
					uv[num14++] = new Vector2(vector.x, vector2.y);
					uv[num14++] = new Vector2(vector2.x, vector2.y);
				}
				if (FlipUvsY)
				{
					Vector2 vector3 = uv[num14 - 4];
					uv[num14 - 4] = uv[num14 - 2];
					uv[num14 - 2] = vector3;
					vector3 = uv[num14 - 3];
					uv[num14 - 3] = uv[num14 - 1];
					uv[num14 - 1] = vector3;
				}
				num6 += 4;
				num11 += num12 / (float)m_spriteSubtileWidth;
				if (linkedListNode != null)
				{
					linkedListNode = linkedListNode.Next;
				}
				num5++;
			}
		}
	}
}
