using UnityEngine;

public class PoisonTrailChallengeModifier : ChallengeModifier
{
	public GoopDefinition Goop;

	public float GoopRadius = 1f;

	public float DampSmoothTime = 0.25f;

	private float MaxSmoothSpeed = 20f;

	private Vector2[] m_goopVelocities;

	private Vector2[] m_goopPoints;

	private void Update()
	{
		if (BraveTime.DeltaTime <= 0f)
		{
			return;
		}
		if (m_goopPoints == null || m_goopPoints.Length != GameManager.Instance.AllPlayers.Length)
		{
			InitializePoints();
		}
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			m_goopPoints[i] = Vector2.SmoothDamp(m_goopPoints[i], GameManager.Instance.AllPlayers[i].specRigidbody.UnitCenter, ref m_goopVelocities[i], DampSmoothTime, MaxSmoothSpeed, Time.deltaTime);
			if (!GameManager.Instance.AllPlayers[i].IsGhost)
			{
				DoGoop(m_goopPoints[i]);
			}
		}
	}

	private void InitializePoints()
	{
		m_goopPoints = new Vector2[GameManager.Instance.AllPlayers.Length];
		m_goopVelocities = new Vector2[GameManager.Instance.AllPlayers.Length];
		for (int i = 0; i < m_goopPoints.Length; i++)
		{
			m_goopPoints[i] = GameManager.Instance.AllPlayers[i].CenterPosition;
		}
	}

	private void DoGoop(Vector2 position)
	{
		if (!BossKillCam.BossDeathCamRunning)
		{
			DeadlyDeadlyGoopManager.GetGoopManagerForGoopType(Goop).AddGoopCircle(position, GoopRadius);
		}
	}
}
