using System;
using UnityEngine;

[Serializable]
public class dfAnchorMargins
{
	[SerializeField]
	public float left;

	[SerializeField]
	public float top;

	[SerializeField]
	public float right;

	[SerializeField]
	public float bottom;

	public override string ToString()
	{
		return string.Format("[L:{0},T:{1},R:{2},B:{3}]", left, top, right, bottom);
	}
}
