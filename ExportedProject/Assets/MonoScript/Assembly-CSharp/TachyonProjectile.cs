using Dungeonator;
using UnityEngine;

public class TachyonProjectile : Projectile
{
	public float ProjectileRadius = 0.3125f;

	public VFXPool SpawnVFX;

	private RoomHandler m_room;

	public override void Start()
	{
		base.Start();
		Vector2 unitPosition = base.specRigidbody.Position.UnitPosition;
		Vector2 vector = FindExpectedEndPoint();
		baseData.range = Vector2.Distance(vector, base.transform.position.XY());
		base.transform.position = vector.ToVector3ZisY();
		base.specRigidbody.Reinitialize();
		base.Direction = (vector - unitPosition).normalized;
		SendInDirection(base.Direction * -1f, true);
		m_distanceElapsed = 0f;
		base.LastPosition = base.transform.position;
		SpawnVFX.SpawnAtPosition(vector.ToVector3ZisY());
	}

	public override void Update()
	{
		base.Update();
		Vector2 unitCenter = base.specRigidbody.UnitCenter;
		if (unitCenter.GetAbsoluteRoom() != m_room)
		{
			DieInAir();
		}
	}

	protected Vector2 FindExpectedEndPoint()
	{
		Dungeon dungeon = GameManager.Instance.Dungeon;
		Vector2 unitCenter = base.specRigidbody.UnitCenter;
		Vector2 b = unitCenter + base.Direction.normalized * baseData.range;
		m_room = unitCenter.GetAbsoluteRoom();
		bool flag = false;
		Vector2 vector = unitCenter;
		IntVector2 intVector = vector.ToIntVector2(VectorConversions.Floor);
		if (dungeon.data.CheckInBoundsAndValid(intVector))
		{
			flag = dungeon.data[intVector].isExitCell;
		}
		float num = b.x - unitCenter.x;
		float num2 = b.y - unitCenter.y;
		float num3 = Mathf.Sign(b.x - unitCenter.x);
		float num4 = Mathf.Sign(b.y - unitCenter.y);
		bool flag2 = num3 > 0f;
		bool flag3 = num4 > 0f;
		int num5 = 0;
		while (Vector2.Distance(vector, b) > 0.1f && num5 < 10000)
		{
			num5++;
			float num6 = Mathf.Abs((((!flag2) ? Mathf.Floor(vector.x) : Mathf.Ceil(vector.x)) - vector.x) / num);
			float num7 = Mathf.Abs((((!flag3) ? Mathf.Floor(vector.y) : Mathf.Ceil(vector.y)) - vector.y) / num2);
			int num8 = Mathf.FloorToInt(vector.x);
			int num9 = Mathf.FloorToInt(vector.y);
			IntVector2 intVector2 = new IntVector2(num8, num9);
			bool flag4 = false;
			if (dungeon.data.CheckInBoundsAndValid(intVector2))
			{
				CellData cellData = dungeon.data[intVector2];
				if (cellData.nearestRoom != m_room || cellData.isExitCell != flag)
				{
					break;
				}
				if (cellData.type != CellType.WALL)
				{
					flag4 = true;
				}
				if (flag4)
				{
					intVector = intVector2;
				}
				if (num6 < num7)
				{
					num8++;
					vector.x += num6 * num + 0.1f * Mathf.Sign(num);
					vector.y += num6 * num2 + 0.1f * Mathf.Sign(num2);
				}
				else
				{
					num9++;
					vector.x += num7 * num + 0.1f * Mathf.Sign(num);
					vector.y += num7 * num2 + 0.1f * Mathf.Sign(num2);
				}
				continue;
			}
			break;
		}
		return intVector.ToCenterVector2();
	}

	protected override void Move()
	{
		base.Move();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
