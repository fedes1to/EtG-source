using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Chancebulon/Dice1")]
public class ChancebulonDice1 : Script
{
	public class ExpandingBullet : Bullet
	{
		private const int SingleFaceShowTime = 13;

		private ChancebulonDice1 m_parent;

		private Vector2 m_offset;

		private int? m_numeralIndex;

		private int m_currentNumeral;

		public ExpandingBullet(ChancebulonDice1 parent, Vector2 offset, int? numeralIndex = null)
		{
			m_parent = parent;
			m_offset = offset;
			m_numeralIndex = numeralIndex;
		}

		protected override IEnumerator Top()
		{
			base.ManualControl = true;
			Vector2 centerPosition = base.Position;
			for (int i = 0; i < 15; i++)
			{
				UpdateVelocity();
				centerPosition += Velocity / 60f;
				Vector2 actualOffset2 = Vector2.Lerp(Vector2.zero, m_offset, (float)i / 14f);
				actualOffset2 = actualOffset2.Rotate(3f * (float)i);
				base.Position = centerPosition + actualOffset2;
				yield return Wait(1);
			}
			Direction = m_parent.aimDirection;
			Speed = 10f;
			for (int j = 0; j < 300; j++)
			{
				UpdateVelocity();
				centerPosition += Velocity / 60f;
				if (m_numeralIndex.HasValue && j % 13 == 0 && j != 0)
				{
					m_currentNumeral = (m_currentNumeral + 1) % 6;
					switch (m_currentNumeral)
					{
					case 5:
						m_offset = new Vector2(0f, 0f);
						break;
					case 0:
					{
						int? numeralIndex15 = m_numeralIndex;
						if (numeralIndex15.HasValue && numeralIndex15.GetValueOrDefault() < 3)
						{
							m_offset = new Vector2(-0.7f, 0.7f);
						}
						else
						{
							m_offset = new Vector2(0.7f, -0.7f);
						}
						break;
					}
					case 1:
					{
						int? numeralIndex6 = m_numeralIndex;
						if (numeralIndex6.HasValue && numeralIndex6.GetValueOrDefault() < 2)
						{
							m_offset = new Vector2(-0.7f, 0.7f);
							break;
						}
						int? numeralIndex7 = m_numeralIndex;
						if (numeralIndex7.HasValue && numeralIndex7.GetValueOrDefault() < 4)
						{
							m_offset = new Vector2(0f, 0f);
						}
						else
						{
							m_offset = new Vector2(0.7f, -0.7f);
						}
						break;
					}
					case 3:
					{
						int? numeralIndex12 = m_numeralIndex;
						if (numeralIndex12.HasValue && numeralIndex12.GetValueOrDefault() < 2)
						{
							m_offset = new Vector2(-0.6f, -0.6f);
							break;
						}
						int? numeralIndex13 = m_numeralIndex;
						if (numeralIndex13.HasValue && numeralIndex13.GetValueOrDefault() < 3)
						{
							m_offset = new Vector2(-0.6f, 0.6f);
							break;
						}
						int? numeralIndex14 = m_numeralIndex;
						if (numeralIndex14.HasValue && numeralIndex14.GetValueOrDefault() < 4)
						{
							m_offset = new Vector2(0.6f, -0.6f);
						}
						else
						{
							m_offset = new Vector2(0.6f, 0.6f);
						}
						break;
					}
					case 2:
					{
						int? numeralIndex8 = m_numeralIndex;
						if (numeralIndex8.HasValue && numeralIndex8.GetValueOrDefault() < 1)
						{
							m_offset = new Vector2(-0.6f, -0.6f);
							break;
						}
						int? numeralIndex9 = m_numeralIndex;
						if (numeralIndex9.HasValue && numeralIndex9.GetValueOrDefault() < 2)
						{
							m_offset = new Vector2(-0.6f, 0.6f);
							break;
						}
						int? numeralIndex10 = m_numeralIndex;
						if (numeralIndex10.HasValue && numeralIndex10.GetValueOrDefault() < 3)
						{
							m_offset = new Vector2(0f, 0f);
							break;
						}
						int? numeralIndex11 = m_numeralIndex;
						if (numeralIndex11.HasValue && numeralIndex11.GetValueOrDefault() < 4)
						{
							m_offset = new Vector2(0.6f, -0.6f);
						}
						else
						{
							m_offset = new Vector2(0.6f, 0.6f);
						}
						break;
					}
					case 4:
					{
						int? numeralIndex = m_numeralIndex;
						if (numeralIndex.HasValue && numeralIndex.GetValueOrDefault() < 1)
						{
							m_offset = new Vector2(-0.6f, -0.6f);
							break;
						}
						int? numeralIndex2 = m_numeralIndex;
						if (numeralIndex2.HasValue && numeralIndex2.GetValueOrDefault() < 2)
						{
							m_offset = new Vector2(-0.6f, 0f);
							break;
						}
						int? numeralIndex3 = m_numeralIndex;
						if (numeralIndex3.HasValue && numeralIndex3.GetValueOrDefault() < 3)
						{
							m_offset = new Vector2(-0.6f, 0.6f);
							break;
						}
						int? numeralIndex4 = m_numeralIndex;
						if (numeralIndex4.HasValue && numeralIndex4.GetValueOrDefault() < 4)
						{
							m_offset = new Vector2(0.6f, -0.6f);
							break;
						}
						int? numeralIndex5 = m_numeralIndex;
						if (numeralIndex5.HasValue && numeralIndex5.GetValueOrDefault() < 5)
						{
							m_offset = new Vector2(0.6f, 0f);
						}
						else
						{
							m_offset = new Vector2(0.6f, 0.6f);
						}
						break;
					}
					}
				}
				base.Position = centerPosition + m_offset.Rotate(3f * (float)(15 + j));
				yield return Wait(1);
			}
			Vanish();
		}
	}

	public const float Radius = 2f;

	public const int GrowTime = 15;

	public const float RotationSpeed = 180f;

	public const float BulletSpeed = 10f;

	public float aimDirection { get; private set; }

	protected override IEnumerator Top()
	{
		base.EndOnBlank = true;
		FireSquare();
		aimDirection = base.AimDirection;
		yield return Wait(15);
		float distanceToTarget = (BulletManager.PlayerPosition() - base.Position).magnitude;
		if (distanceToTarget > 4.5f)
		{
			aimDirection = GetAimDirection(1f, 10f);
		}
	}

	private void FireSquare()
	{
		Vector2 vector = new Vector2(2.2f, 0f).Rotate(45f);
		Vector2 vector2 = new Vector2(2.2f, 0f).Rotate(135f);
		Vector2 vector3 = new Vector2(2.2f, 0f).Rotate(225f);
		Vector2 vector4 = new Vector2(2.2f, 0f).Rotate(-45f);
		FireExpandingLine(vector, vector2, 5);
		FireExpandingLine(vector2, vector3, 5);
		FireExpandingLine(vector3, vector4, 5);
		FireExpandingLine(vector4, vector, 5);
		Fire(new ExpandingBullet(this, new Vector2(0f, 0f), 0));
		Fire(new ExpandingBullet(this, new Vector2(0f, 0f), 1));
		Fire(new ExpandingBullet(this, new Vector2(0f, 0f), 2));
		Fire(new ExpandingBullet(this, new Vector2(0f, 0f), 3));
		Fire(new ExpandingBullet(this, new Vector2(0f, 0f), 4));
		Fire(new ExpandingBullet(this, new Vector2(0f, 0f), 5));
	}

	private void FireExpandingLine(Vector2 start, Vector2 end, int numBullets)
	{
		for (int i = 0; i < numBullets; i++)
		{
			Fire(new ExpandingBullet(this, Vector2.Lerp(start, end, (float)i / ((float)numBullets - 1f))));
		}
	}
}
