using UnityEngine;

public class PowderSkullParticleController : BraveBehaviour
{
	public AIAnimator ParentAnimator;

	public Transform RotationChild;

	public float VelocityFraction = 0.7f;

	private float m_rotationChildInitialRotation;

	private ParticleSystem m_system;

	private ParticleSystem.Particle[] m_particles;

	private Vector3 m_curPosition;

	private Vector3 m_lastPosition;

	public void Start()
	{
		m_lastPosition = base.transform.position;
		m_system = GetComponent<ParticleSystem>();
		if (m_particles == null)
		{
			m_particles = new ParticleSystem.Particle[m_system.maxParticles];
		}
		if (RotationChild != null)
		{
			m_rotationChildInitialRotation = RotationChild.localEulerAngles.x;
		}
	}

	public void LateUpdate()
	{
		m_curPosition = base.transform.position;
		if (RotationChild != null && ParentAnimator != null)
		{
			int num = BraveMathCollege.AngleToOctant(ParentAnimator.FacingDirection);
			RotationChild.localRotation = Quaternion.Euler(m_rotationChildInitialRotation + (float)(num * 45), 0f, 0f);
		}
		Vector3 vector = m_curPosition - m_lastPosition;
		if (BraveTime.DeltaTime > 0f && vector != Vector3.zero)
		{
			int particles = m_system.GetParticles(m_particles);
			for (int i = 0; i < particles; i++)
			{
				m_particles[i].position += vector * VelocityFraction;
			}
			m_system.SetParticles(m_particles, particles);
		}
		m_lastPosition = m_curPosition;
	}
}
