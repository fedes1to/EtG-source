using UnityEngine;

namespace Kvant
{
	[AddComponentMenu("Kvant/Tunnel Scroller")]
	[RequireComponent(typeof(Tunnel))]
	public class TunnelScroller : MonoBehaviour
	{
		[SerializeField]
		private float _speed;

		[SerializeField]
		private float _zRotationSpeed;

		private Transform m_transform;

		public float speed
		{
			get
			{
				return _speed;
			}
			set
			{
				_speed = value;
			}
		}

		private void Update()
		{
			if (m_transform == null)
			{
				m_transform = base.transform;
			}
			m_transform.Rotate(0f, 0f, _zRotationSpeed * BraveTime.DeltaTime);
			GetComponent<Tunnel>().offset += _speed * BraveTime.DeltaTime;
		}
	}
}
