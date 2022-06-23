using UnityEngine;

namespace DaikonForge.Tween.Components
{
	public interface ITweenPropertyBase
	{
		GameObject Target { get; set; }

		string ComponentType { get; set; }

		string MemberName { get; set; }
	}
}
