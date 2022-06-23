using System.Collections.Generic;
using Dungeonator;
using UnityEngine;
using UnityEngine.Serialization;

public class ReverseBeamController : BeamController
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

	public float endRampHeight;

	public int endRampSteps;

	[HideInInspector]
	public bool FlipUvsY;

	[Header("Particles")]
	public bool UsesDispersalParticles;

	[ShowInInspectorIf("UsesDispersalParticles", true)]
	public bool OnlyParticlesOnDestruction;

	[ShowInInspectorIf("UsesDispersalParticles", true)]
	public float DispersalDensity = 3f;

	[ShowInInspectorIf("UsesDispersalParticles", true)]
	public float DispersalMinCoherency = 0.2f;

	[ShowInInspectorIf("UsesDispersalParticles", true)]
	public float DispersalMaxCoherency = 1f;

	[ShowInInspectorIf("UsesDispersalParticles", true)]
	public GameObject DispersalParticleSystemPrefab;

	[ShowInInspectorIf("UsesDispersalParticles", true)]
	public float DispersalExtraImpactFactor = 1f;

	[ShowInInspectorIf("UsesDispersalParticles", true)]
	public int ParticleSkipCount = 20;

	private ParticleSystem m_dispersalParticles;

	private List<AIActor> s_enemiesInRoom = new List<AIActor>();

	private float m_elapsed;

	private tk2dTiledSprite m_sprite;

	private tk2dSpriteAnimationClip m_startAnimationClip;

	private tk2dSpriteAnimationClip m_animationClip;

	private bool m_isCurrentlyFiring = true;

	private bool m_audioPlaying;

	private AIActor m_target;

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
		if (UsesDispersalParticles && m_dispersalParticles == null)
		{
			m_dispersalParticles = GlobalDispersalParticleManager.GetSystemForPrefab(DispersalParticleSystemPrefab);
		}
	}

	public void Update()
	{
		m_globalTimer += BraveTime.DeltaTime;
		if (!m_target || !m_target.healthHaver || m_target.healthHaver.IsDead)
		{
			m_target = null;
		}
		m_hitRigidbody = null;
		HandleBeamFrame(base.Origin, base.Direction, m_isCurrentlyFiring);
		if (m_target == null)
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
		float num = base.projectile.baseData.damage + base.DamageModifier;
		PlayerController playerController = base.projectile.Owner as PlayerController;
		if ((bool)playerController)
		{
			num *= playerController.stats.GetStatValue(PlayerStats.StatType.RateOfFire);
			num *= playerController.stats.GetStatValue(PlayerStats.StatType.Damage);
		}
		if (base.ChanceBasedShadowBullet)
		{
			num *= 2f;
		}
		string value = OtherImpactAnimation;
		if (m_target != null && m_elapsed >= 1f && (bool)m_target.healthHaver)
		{
			value = ((string.IsNullOrEmpty(BossImpactAnimation) || !m_target.healthHaver.IsBoss) ? EnemyImpactAnimation : BossImpactAnimation);
			if (m_target.healthHaver.IsBoss && (bool)base.projectile)
			{
				num *= base.projectile.BossDamageMultiplier;
			}
			if ((bool)base.projectile && base.projectile.BlackPhantomDamageMultiplier != 1f && m_target.IsBlackPhantom)
			{
				num *= base.projectile.BlackPhantomDamageMultiplier;
			}
			m_target.healthHaver.ApplyDamage(num * BraveTime.DeltaTime, Vector2.zero, base.Owner.ActorName);
		}
		if ((bool)m_hitRigidbody)
		{
			if ((bool)m_hitRigidbody.minorBreakable)
			{
				m_hitRigidbody.minorBreakable.Break(base.Direction);
			}
			if ((bool)m_hitRigidbody.majorBreakable)
			{
				m_hitRigidbody.majorBreakable.ApplyDamage(num * BraveTime.DeltaTime, base.Direction, false);
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
			if ((bool)ImpactRenderer)
			{
				ImpactRenderer.transform.position -= vector.ToVector3ZUp();
			}
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
		m_elapsed += BraveTime.DeltaTime;
		AIActor target = m_target;
		if (targetType == TargetType.Screen)
		{
			if (m_target == null)
			{
				m_elapsed = 0f;
				List<AIActor> allEnemies = StaticReferenceManager.AllEnemies;
				for (int i = 0; i < allEnemies.Count; i++)
				{
					AIActor aIActor = allEnemies[i];
					if (aIActor.IsNormalEnemy && aIActor.renderer.isVisible && aIActor.healthHaver.IsAlive && !aIActor.IsGone)
					{
						m_target = aIActor;
						break;
					}
				}
			}
		}
		else if (m_target == null)
		{
			m_elapsed = 0f;
			RoomHandler absoluteRoomFromPosition = GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(barrelPosition.ToIntVector2(VectorConversions.Floor));
			absoluteRoomFromPosition.GetActiveEnemies(RoomHandler.ActiveEnemyType.All, ref s_enemiesInRoom);
			s_enemiesInRoom.Sort((AIActor a, AIActor b) => Vector2.Distance(barrelPosition, a.CenterPosition).CompareTo(Vector2.Distance(barrelPosition, b.CenterPosition)));
			for (int j = 0; j < s_enemiesInRoom.Count; j++)
			{
				AIActor aIActor2 = s_enemiesInRoom[j];
				if (aIActor2.IsNormalEnemy && aIActor2.renderer.isVisible && aIActor2.healthHaver.IsAlive && !aIActor2.IsGone)
				{
					m_target = aIActor2;
					break;
				}
			}
		}
		if (m_target != target && UsesDispersalParticles && OnlyParticlesOnDestruction)
		{
			for (LinkedListNode<Bone> linkedListNode = m_bones.First; linkedListNode != null; linkedListNode = linkedListNode.Next)
			{
				DoDispersalParticles(linkedListNode, 1, true);
			}
		}
		m_bones.Clear();
		Vector3? vector = null;
		float num = 3f;
		float num2 = 100f;
		float num3 = num2 / 2f;
		if ((bool)m_target)
		{
			Vector3 vector2 = direction.normalized * 5f;
			vector2 = Quaternion.Euler(0f, 0f, Mathf.PingPong(Time.realtimeSinceStartup * 147.14f, num2) - num3) * direction.normalized * num;
			Vector3 vector3 = barrelPosition;
			Vector3 vector4 = vector3 + vector2;
			vector2 = Quaternion.Euler(0f, 0f, Mathf.PingPong(Time.realtimeSinceStartup * 172.63f, num2) - num3) * -direction.normalized * num;
			Vector3 vector5 = m_target.specRigidbody.HitboxPixelCollider.UnitCenter;
			Vector3 vector6 = vector5 + vector2;
			float percentComplete = Mathf.Clamp01(m_elapsed);
			DrawBezierCurve(vector3, vector4, vector6, vector5, percentComplete);
			if ((bool)ImpactRenderer)
			{
				ImpactRenderer.renderer.enabled = false;
			}
		}
		else
		{
			Vector3 vector7 = Quaternion.Euler(0f, 0f, Mathf.PingPong(Time.realtimeSinceStartup * 147.14f, num2) - num3) * direction.normalized * num;
			Vector3 vector8 = barrelPosition;
			Vector3 vector9 = vector8 + vector7;
			vector7 = Quaternion.Euler(0f, 0f, Mathf.PingPong(Time.realtimeSinceStartup * 172.63f, num2) - num3) * -direction.normalized * num;
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
			DrawBezierCurve(vector8, vector9, vector11, vector10, 1f);
			if ((bool)ImpactRenderer)
			{
				ImpactRenderer.renderer.enabled = false;
			}
		}
		LinkedListNode<Bone> linkedListNode2 = m_bones.First;
		while (linkedListNode2 != null && linkedListNode2 != m_bones.Last)
		{
			linkedListNode2.Value.normal = (Quaternion.Euler(0f, 0f, 90f) * (linkedListNode2.Next.Value.pos - linkedListNode2.Value.pos)).normalized;
			linkedListNode2 = linkedListNode2.Next;
		}
		if (m_bones.Count > 1)
		{
			m_bones.Last.Value.normal = m_bones.Last.Previous.Value.normal;
		}
		m_isDirty = true;
		if ((bool)ImpactRenderer)
		{
			if (!m_target)
			{
				ImpactRenderer.renderer.enabled = true;
				ImpactRenderer.transform.position = ((!vector.HasValue) ? (base.Gun.CurrentOwner as PlayerController).unadjustedAimPoint.XY() : vector.Value.XY());
				ImpactRenderer.IsPerpendicular = false;
			}
			else
			{
				ImpactRenderer.renderer.enabled = true;
				ImpactRenderer.transform.position = m_target.CenterPosition;
				ImpactRenderer.IsPerpendicular = true;
			}
			ImpactRenderer.HeightOffGround = 6f;
			ImpactRenderer.UpdateZDepth();
		}
		if (!UsesDispersalParticles)
		{
			return;
		}
		int particleSkipCount = ParticleSkipCount;
		LinkedListNode<Bone> linkedListNode3 = m_bones.First;
		int num4 = Random.Range(0, particleSkipCount);
		while (linkedListNode3 != null)
		{
			num4++;
			if (num4 != particleSkipCount)
			{
				linkedListNode3 = linkedListNode3.Next;
				continue;
			}
			num4 = 0;
			DoDispersalParticles(linkedListNode3, 1, true);
			linkedListNode3 = linkedListNode3.Next;
		}
	}

	private Vector2 GetBonePosition(Bone bone)
	{
		return bone.pos;
	}

	private void DoDispersalParticles(LinkedListNode<Bone> boneNode, int subtilesPerTile, bool didImpact)
	{
		if (!UsesDispersalParticles || boneNode.Value == null || boneNode.Next == null || boneNode.Next.Value == null)
		{
			return;
		}
		bool flag = boneNode == m_bones.First;
		Vector2 bonePosition = GetBonePosition(boneNode.Value);
		Vector3 a = bonePosition.ToVector3ZUp(bonePosition.y);
		LinkedListNode<Bone> next = boneNode.Next;
		Vector2 bonePosition2 = GetBonePosition(next.Value);
		Vector3 b = bonePosition2.ToVector3ZUp(bonePosition2.y);
		bool flag2 = next == m_bones.Last && didImpact;
		float num = ((!flag && !flag2) ? 1 : 3);
		int num2 = 1;
		if (flag2)
		{
			num2 = Mathf.CeilToInt((float)num2 * DispersalExtraImpactFactor);
		}
		for (int i = 0; i < num2; i++)
		{
			float t = (float)i / (float)num2;
			if (flag)
			{
				t = Mathf.Lerp(0f, 0.5f, t);
			}
			if (flag2)
			{
				t = Mathf.Lerp(0.5f, 1f, t);
			}
			Vector3 position = Vector3.Lerp(a, b, t);
			float num3 = Mathf.PerlinNoise(position.x / 3f, position.y / 3f);
			Vector3 a2 = Quaternion.Euler(0f, 0f, num3 * 360f) * Vector3.right;
			Vector3 vector = Vector3.Lerp(a2, Random.insideUnitSphere, Random.Range(DispersalMinCoherency, DispersalMaxCoherency));
			ParticleSystem.EmitParams emitParams = default(ParticleSystem.EmitParams);
			emitParams.position = position;
			emitParams.velocity = vector * m_dispersalParticles.startSpeed;
			emitParams.startSize = m_dispersalParticles.startSize;
			emitParams.startLifetime = m_dispersalParticles.startLifetime;
			emitParams.startColor = m_dispersalParticles.startColor;
			ParticleSystem.EmitParams emitParams2 = emitParams;
			m_dispersalParticles.Emit(emitParams2, 1);
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
		if (UsesDispersalParticles && OnlyParticlesOnDestruction)
		{
			for (LinkedListNode<Bone> linkedListNode = m_bones.First; linkedListNode != null; linkedListNode = linkedListNode.Next)
			{
				DoDispersalParticles(linkedListNode, 1, true);
			}
		}
		Object.Destroy(base.gameObject);
	}

	public override void AdjustPlayerBeamTint(Color targetTintColor, int priority, float lerpTime = 0f)
	{
	}

	private void DrawBezierCurve(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float percentComplete)
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
			m_bones.AddLast(new Bone(BraveMathCollege.CalculateBezierPoint(1f - percentComplete, p0, p1, p2, p3)));
		}
		for (int j = 1; (float)j <= num2; j++)
		{
			float num3 = (float)j / num2;
			Vector3 vector3 = BraveMathCollege.CalculateBezierPoint(num3, p0, p1, p2, p3);
			if (num3 > 1f - percentComplete)
			{
				m_bones.AddLast(new Bone(vector3));
			}
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
