using Dungeonator;
using UnityEngine;

public class GatlingGullCrowController : BraveBehaviour
{
	public bool useFacePoint;

	public Vector2 facePoint;

	public bool destroyOnArrival;

	protected Vector2 m_currentPosition;

	protected float m_speed;

	protected float m_currentSpeed;

	public Vector2 CurrentTargetPosition { get; set; }

	public float CurrentTargetHeight { get; set; }

	private void Start()
	{
		base.spriteAnimator.ClipFps = base.spriteAnimator.ClipFps * Random.Range(0.7f, 1.4f);
		m_currentPosition = base.transform.position.XY();
		m_speed = Random.Range(7f, 10f);
		base.sprite.UpdateZDepth();
	}

	private void Update()
	{
		if (destroyOnArrival && (GameManager.Instance.CurrentGameMode == GameManager.GameMode.BOSSRUSH || GameManager.Instance.CurrentGameMode == GameManager.GameMode.SUPERBOSSRUSH))
		{
			IntVector2 intVector = base.transform.position.IntXY(VectorConversions.Floor);
			if (!GameManager.Instance.Dungeon.data.CheckInBoundsAndValid(intVector) || GameManager.Instance.Dungeon.data[intVector].type == CellType.WALL)
			{
				Object.Destroy(base.gameObject);
				return;
			}
		}
		if (base.sprite.HeightOffGround != CurrentTargetHeight)
		{
			float num = CurrentTargetHeight - base.sprite.HeightOffGround;
			float num2 = Mathf.Sign(num) * 3f * BraveTime.DeltaTime;
			if (Mathf.Abs(num2) > Mathf.Abs(num))
			{
				num2 = num;
			}
			base.sprite.HeightOffGround += num2;
		}
		if (m_currentPosition != CurrentTargetPosition)
		{
			if (m_currentSpeed < m_speed)
			{
				m_currentSpeed = Mathf.Clamp(m_currentSpeed + 4f * BraveTime.DeltaTime, 0f, m_speed);
			}
			Vector2 vector = CurrentTargetPosition - m_currentPosition;
			base.sprite.FlipX = ((!useFacePoint) ? (vector.x < 0f) : ((facePoint - m_currentPosition).x < 0f));
			float magnitude = vector.magnitude;
			float num3 = Mathf.Clamp(m_currentSpeed * BraveTime.DeltaTime, 0f, magnitude);
			m_currentPosition += num3 * vector.normalized;
			base.transform.position = m_currentPosition.ToVector3ZUp();
			base.sprite.UpdateZDepth();
		}
		else
		{
			if (destroyOnArrival)
			{
				Object.Destroy(base.gameObject);
			}
			m_currentSpeed = 0f;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
