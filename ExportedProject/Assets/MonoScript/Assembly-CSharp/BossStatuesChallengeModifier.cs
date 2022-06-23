using Dungeonator;
using UnityEngine;

public class BossStatuesChallengeModifier : ChallengeModifier
{
	public GoopDefinition Goop;

	public int chessSquaresX = 4;

	public int chessSquaresY = 4;

	public float InitialDelayTime = 4f;

	public float AdditionalDelayTime = 1.5f;

	private RoomHandler m_room;

	private DeadlyDeadlyGoopManager m_manager;

	private bool m_firstQuadrant;

	private float m_timer;

	private void Start()
	{
		m_room = GameManager.Instance.PrimaryPlayer.CurrentRoom;
		m_timer = InitialDelayTime;
		m_manager = DeadlyDeadlyGoopManager.GetGoopManagerForGoopType(Goop);
	}

	private void Update()
	{
		m_timer -= BraveTime.DeltaTime;
		if (!(m_timer <= 0f))
		{
			return;
		}
		m_timer = Goop.lifespan + AdditionalDelayTime;
		Vector2 vector = new Vector2((float)m_room.area.dimensions.x / (1f * (float)chessSquaresX), (float)m_room.area.dimensions.y / (1f * (float)chessSquaresY));
		for (int i = 0; i < chessSquaresX; i++)
		{
			for (int j = 0; j < chessSquaresY; j++)
			{
				Vector2 vector2 = m_room.area.basePosition.ToVector2() + new Vector2(vector.x * (float)i, vector.y * (float)j);
				Vector2 max = vector2 + vector;
				int num = (i + j) % 2;
				if ((num == 1 && m_firstQuadrant) || (num == 0 && !m_firstQuadrant))
				{
					m_manager.TimedAddGoopRect(vector2, max, 0.5f);
				}
			}
		}
		m_firstQuadrant = !m_firstQuadrant;
	}
}
