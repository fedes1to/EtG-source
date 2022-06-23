using System;
using Dungeonator;
using UnityEngine;

public class SkyRocket : BraveBehaviour
{
	public enum SkyRocketState
	{
		Ascend,
		Hang,
		Descend
	}

	public float AscentTime = 1f;

	public AnimationCurve AscentCurve;

	public float HangTime = 1f;

	public float DescentTime = 1f;

	public AnimationCurve DescentCurve;

	public float MaxHeight = 30f;

	public string DownSprite = "rocket_white_red_down_001";

	public float Variance = 0.25f;

	public float LeadPercentage = 0.66f;

	public GameObject LandingTargetSprite;

	public bool DoExplosion = true;

	public ExplosionData ExplosionData;

	public bool IgnoreExplosionQueues;

	public VFXPool SpawnVfx;

	public GameObject SpawnObject;

	private float MaxSpriteHeight = 10f;

	[NonSerialized]
	public SpeculativeRigidbody Target;

	[NonSerialized]
	public Vector2 TargetVector2;

	private Vector3 m_startPosition;

	private float m_startHeight;

	private Vector3 m_targetLandPosition;

	private float m_timer;

	private float m_totalDuration;

	private GameObject m_landingTarget;

	private SkyRocketState m_state;

	public void Start()
	{
		base.sprite = GetComponentInChildren<tk2dSprite>();
		m_timer = AscentTime;
		m_startPosition = base.transform.position;
		m_startHeight = base.sprite.HeightOffGround;
		base.sprite = GetComponentInChildren<tk2dSprite>();
		base.spriteAnimator = GetComponentInChildren<tk2dSpriteAnimator>();
		if (AscentTime == 0f)
		{
			base.transform.position = m_startPosition + new Vector3(0f, MaxHeight, 0f);
		}
	}

	public void Update()
	{
		m_timer -= BraveTime.DeltaTime;
		if (m_state == SkyRocketState.Ascend)
		{
			float num = AscentCurve.Evaluate(1f - Mathf.Clamp01(m_timer / AscentTime));
			float y = num * MaxHeight;
			base.transform.position = m_startPosition + new Vector3(0f, y, 0f);
			base.sprite.HeightOffGround = m_startHeight + num * MaxSpriteHeight;
			if (m_timer <= 0f)
			{
				m_timer = HangTime;
				m_state = SkyRocketState.Hang;
				if ((bool)base.sprite.attachParent)
				{
					base.sprite.attachParent.DetachRenderer(base.sprite);
				}
				if (TargetVector2 != Vector2.zero)
				{
					m_targetLandPosition = TargetVector2;
				}
				else
				{
					Vector2 vector = new Vector2(UnityEngine.Random.Range(0f - Variance, Variance), UnityEngine.Random.Range(0f - Variance, Variance));
					bool flag = UnityEngine.Random.value < LeadPercentage;
					m_targetLandPosition = Target.UnitCenter + vector;
					if ((bool)Target)
					{
						PlayerController playerController = Target.gameActor as PlayerController;
						if (flag && (bool)playerController)
						{
							Vector2 vector2 = ((!playerController) ? Target.Velocity : playerController.AverageVelocity);
							m_targetLandPosition += (Vector3)vector2 * (HangTime + DescentTime);
						}
					}
					IntVector2 pos = Target.UnitCenter.ToIntVector2(VectorConversions.Floor);
					RoomHandler roomFromPosition = GameManager.Instance.Dungeon.data.GetRoomFromPosition(pos);
					if (roomFromPosition != null)
					{
						m_targetLandPosition = Vector2Extensions.Clamp(m_targetLandPosition, (roomFromPosition.area.basePosition + IntVector2.One).ToVector2(), (roomFromPosition.area.basePosition + roomFromPosition.area.dimensions - IntVector2.One).ToVector2());
					}
				}
				m_landingTarget = SpawnManager.SpawnVFX(LandingTargetSprite, m_targetLandPosition, Quaternion.identity);
				m_landingTarget.GetComponentInChildren<tk2dSprite>().UpdateZDepth();
				tk2dSpriteAnimator componentInChildren = m_landingTarget.GetComponentInChildren<tk2dSpriteAnimator>();
				componentInChildren.Play(componentInChildren.DefaultClip, 0f, (float)componentInChildren.DefaultClip.frames.Length / (HangTime + DescentTime));
			}
		}
		else if (m_state == SkyRocketState.Hang)
		{
			base.transform.position = m_targetLandPosition + new Vector3(0f, MaxHeight, 0f);
			if (m_timer <= 0f)
			{
				m_timer = DescentTime;
				m_state = SkyRocketState.Descend;
				base.transform.localEulerAngles = base.transform.localEulerAngles + new Vector3(0f, 0f, 180f);
				if (!string.IsNullOrEmpty(DownSprite))
				{
					base.sprite.SetSprite(DownSprite);
				}
			}
		}
		else if (m_state == SkyRocketState.Descend)
		{
			float num2 = 1f - DescentCurve.Evaluate(1f - Mathf.Clamp01(m_timer / DescentTime));
			float y2 = MaxHeight - num2 * MaxHeight;
			base.transform.position = m_targetLandPosition + new Vector3(0f, y2, 0f);
			base.sprite.HeightOffGround = m_startHeight + (MaxSpriteHeight - num2 * MaxSpriteHeight);
			if (m_timer <= 0f)
			{
				base.transform.position = m_targetLandPosition;
				if (DoExplosion)
				{
					Vector3 targetLandPosition = m_targetLandPosition;
					ExplosionData explosionData = ExplosionData;
					Vector2 zero = Vector2.zero;
					bool ignoreExplosionQueues = IgnoreExplosionQueues;
					Exploder.Explode(targetLandPosition, explosionData, zero, null, ignoreExplosionQueues);
				}
				SpawnVfx.SpawnAtPosition(base.transform.position);
				if ((bool)SpawnObject)
				{
					UnityEngine.Object.Instantiate(SpawnObject, base.transform.position, Quaternion.identity);
				}
				SpawnManager.Despawn(m_landingTarget);
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}
		base.sprite.HeightOffGround = 2f;
		base.sprite.UpdateZDepth();
	}

	public void DieInAir()
	{
		if (m_state != SkyRocketState.Descend || !(m_timer < 0.5f))
		{
			if ((bool)m_landingTarget)
			{
				SpawnManager.Despawn(m_landingTarget);
			}
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
