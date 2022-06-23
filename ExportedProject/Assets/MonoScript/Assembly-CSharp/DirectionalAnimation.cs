using System;
using System.Globalization;
using System.Linq;
using UnityEngine;

[Serializable]
public class DirectionalAnimation
{
	public enum DirectionType
	{
		None,
		Single,
		TwoWayHorizontal,
		TwoWayVertical,
		FourWay,
		SixWay,
		EightWay,
		SixteenWay,
		SixteenWayTemp,
		FourWayCardinal,
		EightWayOrdinal
	}

	public class SingleAnimation
	{
		public string suffix;

		public float minAngle;

		public float maxAngle;

		public float artAngle;

		public int? mirrorIndex;

		public SingleAnimation(string suffix, float minAngle, float maxAngle, float artAngle, int? mirrorIndex = null)
		{
			this.suffix = suffix;
			this.minAngle = minAngle;
			this.maxAngle = maxAngle;
			this.artAngle = artAngle;
			this.mirrorIndex = mirrorIndex;
		}
	}

	public enum FlipType
	{
		None,
		Flip,
		Unused,
		Mirror
	}

	public class Info
	{
		public string name;

		public bool flipped;

		public float artAngle;

		public void SetAll(string name, bool flipped, float artAngle)
		{
			this.name = name;
			this.flipped = flipped;
			this.artAngle = artAngle;
		}
	}

	public const float s_BACKFACING_ANGLE_MAX = 155f;

	public const float s_BACKFACING_ANGLE_MIN = 25f;

	public const float s_BACKWARDS_ANGLE_MAX = 120f;

	public const float s_BACKWARDS_ANGLE_MIN = 60f;

	public const float s_FORWARDS_ANGLE_MAX = -60f;

	public const float s_FORWARDS_ANGLE_MIN = -120f;

	public const float c_AngleBuffer = 2.5f;

	public static SingleAnimation[][] m_combined = new SingleAnimation[11][]
	{
		new SingleAnimation[0],
		new SingleAnimation[1]
		{
			new SingleAnimation(string.Empty, 0f, 360f, -90f)
		},
		new SingleAnimation[2]
		{
			new SingleAnimation("right", -90f, 90f, 0f, 1),
			new SingleAnimation("left", 90f, 270f, 180f, 0)
		},
		new SingleAnimation[2]
		{
			new SingleAnimation("back", 0f, 180f, 90f),
			new SingleAnimation("front", 180f, 360f, -90f)
		},
		new SingleAnimation[4]
		{
			new SingleAnimation("back_right", 25f, 90f, 45f, 3),
			new SingleAnimation("front_right", -90f, 25f, -45f, 2),
			new SingleAnimation("front_left", 155f, 270f, -135f, 1),
			new SingleAnimation("back_left", 90f, 155f, 135f, 0)
		},
		new SingleAnimation[6]
		{
			new SingleAnimation("back", 60f, 120f, 90f),
			new SingleAnimation("back_right", 25f, 60f, 45f, 5),
			new SingleAnimation("front_right", -60f, 25f, -45f, 4),
			new SingleAnimation("front", 240f, 300f, -90f),
			new SingleAnimation("front_left", 155f, 240f, -135f, 2),
			new SingleAnimation("back_left", 120f, 155f, 135f, 1)
		},
		new SingleAnimation[8]
		{
			new SingleAnimation("back", 67.5f, 112.5f, 90f),
			new SingleAnimation("back_right", 22.5f, 67.5f, 45f, 7),
			new SingleAnimation("right", -22.5f, 22.5f, 0f, 6),
			new SingleAnimation("front_right", 292.5f, 337.5f, -45f, 5),
			new SingleAnimation("front", 247.5f, 292.5f, -90f),
			new SingleAnimation("front_left", 202.5f, 247.5f, -135f, 3),
			new SingleAnimation("left", 157.5f, 202.5f, 180f, 2),
			new SingleAnimation("back_left", 112.5f, 157.5f, 135f, 1)
		},
		new SingleAnimation[16]
		{
			new SingleAnimation("north", 78.75f, 101.25f, 90f),
			new SingleAnimation("north_northeast", 56.25f, 78.75f, 67.5f, 15),
			new SingleAnimation("northeast", 33.75f, 56.25f, 45f, 14),
			new SingleAnimation("east_northeast", 11.25f, 33.75f, 22.5f, 13),
			new SingleAnimation("east", -11.25f, 11.25f, 0f, 12),
			new SingleAnimation("east_southeast", 326.25f, 348.75f, -22.5f, 11),
			new SingleAnimation("southeast", 303.75f, 326.25f, -45f, 10),
			new SingleAnimation("south_southeast", 281.25f, 303.75f, -67.5f, 9),
			new SingleAnimation("south", 258.75f, 281.25f, -90f),
			new SingleAnimation("south_southwest", 236.25f, 258.75f, -112.5f, 7),
			new SingleAnimation("southwest", 213.75f, 236.25f, -135f, 6),
			new SingleAnimation("west_southwest", 191.25f, 213.75f, -157.5f, 5),
			new SingleAnimation("west", 168.75f, 191.25f, 180f, 4),
			new SingleAnimation("west_northwest", 146.25f, 168.75f, 157.5f, 3),
			new SingleAnimation("northwest", 123.75f, 146.25f, 135f, 2),
			new SingleAnimation("north_northwest", 101.25f, 123.75f, 112.5f, 1)
		},
		new SingleAnimation[16]
		{
			new SingleAnimation("north", 78.75f, 101.25f, 90f),
			new SingleAnimation("north_northeast", 56.25f, 78.75f, 67.5f, 15),
			new SingleAnimation("northeast", 33.75f, 56.25f, 45f, 14),
			new SingleAnimation("east_northeast", 11.25f, 33.75f, 22.5f, 13),
			new SingleAnimation("east", 348.75f, 11.25f, 0f, 12),
			new SingleAnimation("east_southeast", 326.25f, 348.75f, -22.5f, 11),
			new SingleAnimation("southeast", 303.75f, 326.25f, -45f, 10),
			new SingleAnimation("south_southeast", 281.25f, 303.75f, -67.5f, 9),
			new SingleAnimation("south", 258.75f, 281.25f, -90f),
			new SingleAnimation("south_southwest", 236.25f, 258.75f, -112.5f, 7),
			new SingleAnimation("southwest", 213.75f, 236.25f, -135f, 6),
			new SingleAnimation("west_southwest", 191.25f, 213.75f, -157.5f, 5),
			new SingleAnimation("west", 168.75f, 191.25f, 180f, 4),
			new SingleAnimation("west_northwest", 146.25f, 168.75f, 157.5f, 3),
			new SingleAnimation("northwest", 123.75f, 146.25f, 135f, 2),
			new SingleAnimation("north_northwest", 101.25f, 123.75f, 112.5f, 1)
		},
		new SingleAnimation[4]
		{
			new SingleAnimation("north", 45f, 135f, 90f),
			new SingleAnimation("east", -45f, 45f, 0f, 3),
			new SingleAnimation("south", 225f, 315f, -90f),
			new SingleAnimation("west", 135f, 225f, 180f, 1)
		},
		new SingleAnimation[8]
		{
			new SingleAnimation("north", 67.5f, 112.5f, 90f),
			new SingleAnimation("northeast", 22.5f, 67.5f, 45f, 7),
			new SingleAnimation("east", -22.5f, 22.5f, 0f, 6),
			new SingleAnimation("southeast", 292.5f, 337.5f, -45f, 5),
			new SingleAnimation("south", 247.5f, 292.5f, -90f),
			new SingleAnimation("southwest", 202.5f, 247.5f, -135f, 3),
			new SingleAnimation("west", 157.5f, 202.5f, 180f, 2),
			new SingleAnimation("northwest", 112.5f, 157.5f, 135f, 1)
		}
	};

	public DirectionType Type;

	public string Prefix;

	public string[] AnimNames;

	public FlipType[] Flipped;

	private Info m_info = new Info();

	[NonSerialized]
	private int m_lastAnimIndex = -1;

	[NonSerialized]
	private int m_previousEighthIndex = -1;

	[NonSerialized]
	private float m_tempCooldown;

	[NonSerialized]
	private int m_tempIndex;

	public bool HasAnimation
	{
		get
		{
			return Type != DirectionType.None;
		}
	}

	public Info GetInfo(Vector2 dir, bool frameUpdate = false)
	{
		return GetInfo(dir.ToAngle(), frameUpdate);
	}

	public Info GetInfo(float angleDegrees, bool frameUpdate = false)
	{
		if (float.IsNaN(angleDegrees))
		{
			Debug.LogWarning("Warning: NaN Animation Angle!");
			angleDegrees = 0f;
		}
		if (Type == DirectionType.SixteenWayTemp)
		{
			return GetInfoSixteenWayTemp(angleDegrees, frameUpdate);
		}
		angleDegrees = BraveMathCollege.ClampAngle360(angleDegrees);
		SingleAnimation[] array = m_combined[(int)Type];
		if (m_lastAnimIndex != -1 && (m_lastAnimIndex < 0 || m_lastAnimIndex >= array.Length))
		{
			m_lastAnimIndex = -1;
		}
		if (m_lastAnimIndex != -1)
		{
			float minAngle = array[m_lastAnimIndex].minAngle;
			if (minAngle < 0f && angleDegrees >= minAngle + 360f - 2.5f)
			{
				return GetInfo(m_lastAnimIndex);
			}
			float maxAngle = array[m_lastAnimIndex].maxAngle;
			if (angleDegrees >= minAngle - 2.5f && angleDegrees <= maxAngle + 2.5f)
			{
				return GetInfo(m_lastAnimIndex);
			}
		}
		for (int i = 0; i < array.Length; i++)
		{
			if (Type != DirectionType.Single && Flipped[i] == FlipType.Unused)
			{
				continue;
			}
			float minAngle2 = array[i].minAngle;
			if (minAngle2 < 0f && angleDegrees >= minAngle2 + 360f)
			{
				if (frameUpdate)
				{
					m_lastAnimIndex = i;
				}
				return GetInfo(i);
			}
			float maxAngle2 = array[i].maxAngle;
			if (angleDegrees >= minAngle2 && angleDegrees <= maxAngle2)
			{
				if (frameUpdate)
				{
					m_lastAnimIndex = i;
				}
				return GetInfo(i);
			}
		}
		int num = -1;
		float num2 = float.MaxValue;
		for (int j = 0; j < array.Length; j++)
		{
			if (Type == DirectionType.Single || Flipped[j] != FlipType.Unused)
			{
				float num3 = Mathf.Min(BraveMathCollege.AbsAngleBetween(angleDegrees, array[j].minAngle), BraveMathCollege.AbsAngleBetween(angleDegrees, array[j].maxAngle));
				if (num3 < num2)
				{
					num = j;
					num2 = num3;
				}
			}
		}
		if (num >= 0)
		{
			if (frameUpdate)
			{
				m_lastAnimIndex = num;
			}
			return GetInfo(num);
		}
		return null;
	}

	private Info GetInfoSixteenWayTemp(float angleDegrees, bool frameUpdate)
	{
		angleDegrees = BraveMathCollege.ClampAngle360(angleDegrees);
		if (frameUpdate)
		{
			m_tempCooldown -= BraveTime.DeltaTime;
		}
		int num;
		if (m_tempCooldown > 0f)
		{
			num = m_tempIndex;
		}
		else
		{
			float num2 = BraveMathCollege.ClampAngle360(0f - angleDegrees + 90f + 22.5f);
			int num3 = (int)(num2 / 45f);
			num = num3 * 2;
			if (num3 != -1 && m_previousEighthIndex != -1)
			{
				if ((num3 == 0 && m_previousEighthIndex == 7) || (num3 == 7 && m_previousEighthIndex == 0))
				{
					num = (m_tempIndex = 15);
					m_tempCooldown = 0.1f;
				}
				else if (num3 == m_previousEighthIndex + 1)
				{
					num = (m_tempIndex = m_previousEighthIndex * 2 + 1);
					m_tempCooldown = 0.1f;
				}
				else if (num3 == m_previousEighthIndex - 1)
				{
					num = (m_tempIndex = m_previousEighthIndex * 2 - 1);
					m_tempCooldown = 0.1f;
				}
			}
			m_previousEighthIndex = num3;
		}
		if (Flipped[num] != FlipType.Unused)
		{
			return GetInfo(num);
		}
		int num4 = num + (((float)(num % 1) > 0.5f) ? 1 : (-1));
		num4 = (num4 + AnimNames.Count()) % AnimNames.Count();
		return GetInfo(num4);
	}

	public Info GetInfo(int index)
	{
		if (Type == DirectionType.Single && index == 0)
		{
			m_info.SetAll(Prefix, false, -90f);
			return m_info;
		}
		if (index > Flipped.Length - 1)
		{
			Debug.LogError("shit");
		}
		if (Flipped[index] == FlipType.Mirror)
		{
			index = GetMirrorIndex(Type, index);
			m_info.SetAll(GetName(index), true, GetArtAngle(index));
			return m_info;
		}
		m_info.SetAll(GetName(index), Flipped[index] == FlipType.Flip, GetArtAngle(index));
		return m_info;
	}

	public int GetNumAnimations()
	{
		return GetNumAnimations(Type);
	}

	private string GetName(int index)
	{
		if (string.IsNullOrEmpty(AnimNames[index]))
		{
			AnimNames[index] = GetDefaultName(Prefix, Type, index);
		}
		return AnimNames[index];
	}

	private float GetArtAngle(int index)
	{
		return m_combined[(int)Type][index].artAngle;
	}

	public static int GetNumAnimations(DirectionType type)
	{
		return m_combined[(int)type].Length;
	}

	public static string GetLabel(DirectionType type, int index)
	{
		string str = m_combined[(int)type][index].suffix.Replace('_', ' ');
		return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str);
	}

	public static string GetSuffix(DirectionType type, int index)
	{
		return m_combined[(int)type][index].suffix;
	}

	public static string GetDefaultName(string prefix, DirectionType type, int index)
	{
		if (type == DirectionType.Single)
		{
			return prefix;
		}
		if (prefix.Contains("{0}"))
		{
			return string.Format(prefix, GetSuffix(type, index));
		}
		return prefix + "_" + GetSuffix(type, index);
	}

	public static bool HasMirror(DirectionType type, int index)
	{
		return m_combined[(int)type][index].mirrorIndex.HasValue;
	}

	public static int GetMirrorIndex(DirectionType type, int index)
	{
		return m_combined[(int)type][index].mirrorIndex.Value;
	}
}
