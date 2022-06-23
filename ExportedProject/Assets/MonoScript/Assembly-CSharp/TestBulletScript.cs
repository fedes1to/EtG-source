public class TestBulletScript : BraveBehaviour
{
	public float fireDelay = 1f;

	private BulletScriptSource m_bulletSource;

	private float m_counter;

	public void Awake()
	{
		m_bulletSource = GetComponentInChildren<BulletScriptSource>();
	}

	private void Update()
	{
		if (m_bulletSource.IsEnded)
		{
			m_counter += BraveTime.DeltaTime;
			if (m_counter > fireDelay)
			{
				m_counter = 0f;
				m_bulletSource.Initialize();
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
