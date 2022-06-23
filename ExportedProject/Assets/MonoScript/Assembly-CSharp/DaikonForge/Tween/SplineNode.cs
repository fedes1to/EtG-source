using UnityEngine;

namespace DaikonForge.Tween
{
	[ExecuteInEditMode]
	public class SplineNode : MonoBehaviour
	{
		public void OnDestroy()
		{
			if (!Application.isPlaying && !(base.transform.parent == null))
			{
				SplineObject component = base.transform.parent.GetComponent<SplineObject>();
				if (!(component == null))
				{
					component.ControlPoints.Remove(base.transform);
				}
			}
		}
	}
}
