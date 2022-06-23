using UnityEngine;

public class BodyPartController : BraveBehaviour
{
	public enum AimFromType
	{
		Transform = 10,
		ActorHitBoxCenter = 20
	}

	public AIActor specifyActor;

	public bool hasOutlines;

	public bool faceTarget;

	[ShowInInspectorIf("faceTarget", true)]
	public float faceTargetTurnSpeed = -1f;

	[ShowInInspectorIf("faceTarget", true)]
	public AimFromType aimFrom = AimFromType.Transform;

	public bool autoDepth = true;

	public bool redirectHealthHaver;

	public bool independentFlashOnDamage;

	public int myPixelCollider = -1;

	protected AIActor m_body;

	private float m_heightOffBody;

	private bool m_bodyFound;

	public bool OverrideFacingDirection { get; set; }

	public virtual void Awake()
	{
		if ((bool)specifyActor)
		{
			m_body = specifyActor;
		}
		if (!m_body)
		{
			m_body = base.aiActor;
		}
		if (!m_body && (bool)base.transform.parent)
		{
			m_body = base.transform.parent.GetComponent<AIActor>();
		}
		if ((bool)m_body)
		{
			if (independentFlashOnDamage)
			{
				m_body.healthHaver.RegisterBodySprite(base.sprite, true, myPixelCollider);
			}
			else
			{
				m_body.healthHaver.RegisterBodySprite(base.sprite);
			}
			m_bodyFound = true;
		}
	}

	public virtual void Start()
	{
		m_heightOffBody = base.sprite.HeightOffGround;
		if (hasOutlines)
		{
			SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.black, base.sprite.HeightOffGround + 0.1f);
			if ((bool)m_body)
			{
				ObjectVisibilityManager component = m_body.GetComponent<ObjectVisibilityManager>();
				if ((bool)component)
				{
					component.ResetRenderersList();
				}
			}
		}
		if (!m_bodyFound && (bool)m_body)
		{
			m_body.healthHaver.RegisterBodySprite(base.sprite);
			m_bodyFound = true;
		}
		if (!base.specRigidbody)
		{
			base.specRigidbody = m_body.specRigidbody;
		}
		if ((faceTarget & (bool)base.aiAnimator) && (bool)m_body.aiAnimator)
		{
			base.aiAnimator.LockFacingDirection = true;
			base.aiAnimator.FacingDirection = m_body.aiAnimator.FacingDirection;
		}
		if ((bool)base.specRigidbody && redirectHealthHaver)
		{
			base.specRigidbody.healthHaver = m_body.healthHaver;
		}
	}

	public virtual void Update()
	{
		float angle;
		if (!OverrideFacingDirection && faceTarget && TryGetAimAngle(out angle))
		{
			if (faceTargetTurnSpeed > 0f)
			{
				float current = ((!base.aiAnimator) ? base.transform.eulerAngles.z : base.aiAnimator.FacingDirection);
				angle = Mathf.MoveTowardsAngle(current, angle, faceTargetTurnSpeed * BraveTime.DeltaTime);
			}
			if ((bool)base.aiAnimator)
			{
				base.aiAnimator.LockFacingDirection = true;
				base.aiAnimator.FacingDirection = angle;
			}
			else
			{
				base.transform.rotation = Quaternion.Euler(0f, 0f, angle);
			}
		}
		if (autoDepth && (bool)base.aiAnimator)
		{
			float num = BraveMathCollege.ClampAngle180(m_body.aiAnimator.FacingDirection);
			float num2 = BraveMathCollege.ClampAngle180(base.aiAnimator.FacingDirection);
			bool flag = ((num <= 155f && num >= 25f && num2 <= 155f && num2 >= 25f) ? true : false);
			base.sprite.HeightOffGround = ((!flag) ? m_heightOffBody : (0f - m_heightOffBody));
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	protected virtual bool TryGetAimAngle(out float angle)
	{
		angle = 0f;
		if ((bool)m_body.TargetRigidbody)
		{
			Vector2 unitCenter = m_body.TargetRigidbody.GetUnitCenter(ColliderType.HitBox);
			Vector2 vector = base.transform.position.XY();
			if (aimFrom == AimFromType.ActorHitBoxCenter)
			{
				vector = m_body.specRigidbody.GetUnitCenter(ColliderType.HitBox);
			}
			angle = (unitCenter - vector).ToAngle();
			return true;
		}
		if ((bool)m_body.aiAnimator)
		{
			angle = m_body.aiAnimator.FacingDirection;
			return true;
		}
		return false;
	}
}
