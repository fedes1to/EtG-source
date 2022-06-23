using System;
using Dungeonator;
using UnityEngine;

public class CorpseSpawnController : BraveBehaviour
{
	private enum State
	{
		None,
		Prespawn,
		Spawning
	}

	[CheckDirectionalAnimation(null)]
	public string PrespawnAnim = "corpse_prespawn";

	[CheckDirectionalAnimation(null)]
	public string SpawnAnim = "corpse_spawn";

	[EnemyIdentifier]
	public string EnemyGuid;

	public Vector2 LeftSpawnOffset;

	public Vector2 RightSpawnOffset;

	public float PrespawnTime = 5f;

	public float AdditionalPrespawnTime = 5f;

	public bool CancelOnRoomClear = true;

	private State m_state;

	private bool m_isRight;

	private RoomHandler m_room;

	public void Start()
	{
		tk2dSpriteAnimator obj = base.spriteAnimator;
		obj.AnimationEventTriggered = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>)Delegate.Combine(obj.AnimationEventTriggered, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>(AnimationEventTriggered));
	}

	public void Update()
	{
		if (m_state == State.Prespawn)
		{
			if (!base.aiAnimator.IsPlaying(PrespawnAnim))
			{
				base.aiAnimator.PlayUntilFinished(SpawnAnim);
				m_state = State.Spawning;
			}
		}
		else if (m_state == State.Spawning && !base.aiAnimator.IsPlaying(SpawnAnim))
		{
			Vector2 vector = base.transform.position;
			Vector2 vector2 = ((!m_isRight) ? LeftSpawnOffset : RightSpawnOffset);
			AIActor.Spawn(EnemyDatabase.GetOrLoadByGuid(EnemyGuid), vector + vector2, GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(vector.ToIntVector2()), true);
			UnityEngine.Object.Destroy(base.gameObject);
		}
		if (m_state != 0 && CancelOnRoomClear && m_room.GetActiveEnemiesCount(RoomHandler.ActiveEnemyType.RoomClear) == 0)
		{
			m_state = State.None;
			base.aiAnimator.PlayUntilCancelled(PrespawnAnim);
			base.aiAnimator.enabled = false;
			base.spriteAnimator.enabled = false;
			base.sprite.OverrideMaterialMode = tk2dBaseSprite.SpriteMaterialOverrideMode.OVERRIDE_MATERIAL_COMPLEX;
			GetComponent<DebrisObject>().FadeToOverrideColor(new Color(0f, 0f, 0f, 0.6f), 0.25f);
			GetComponent<Renderer>().material.shader = ShaderCache.Acquire("Brave/LitTk2dCustomFalloffTiltedCutoutFastPixelShadow");
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public void Init(AIActor aiActor)
	{
		m_room = aiActor.ParentRoom;
		float num = PrespawnTime;
		CorpseSpawnController[] array = UnityEngine.Object.FindObjectsOfType<CorpseSpawnController>();
		if (array != null && array.Length > 1)
		{
			num += (float)(array.Length - 1) * AdditionalPrespawnTime;
		}
		m_isRight = !aiActor.sprite.CurrentSprite.name.Contains("left");
		base.aiAnimator.FacingDirection = ((!m_isRight) ? 180 : 0);
		base.aiAnimator.PlayForDuration(PrespawnAnim, num);
		m_state = State.Prespawn;
	}

	private void AnimationEventTriggered(tk2dSpriteAnimator animator, tk2dSpriteAnimationClip clip, int frame)
	{
		if (m_state != 0 && clip.GetFrame(frame).eventInfo == "perp")
		{
			base.sprite.IsPerpendicular = true;
		}
	}
}
