using Brave.BulletScript;
using UnityEngine;

public class BulletScriptSource : BraveBehaviour
{
	public BulletScriptSelector BulletScript;

	private int m_lastUpdatedFrame = -1;

	public Bullet RootBullet { get; set; }

	public AIBulletBank BulletManager { get; set; }

	public bool FreezeTopPosition { get; set; }

	public bool IsEnded
	{
		get
		{
			return RootBullet == null || RootBullet.IsEnded;
		}
	}

	public void Awake()
	{
		StaticReferenceManager.AllBulletScriptSources.Add(this);
	}

	public void Start()
	{
		if (!BulletManager)
		{
			BulletManager = base.bulletBank;
		}
		if (RootBullet == null)
		{
			Initialize();
		}
	}

	public void Update()
	{
		if (RootBullet != null && !RootBullet.IsEnded && m_lastUpdatedFrame != Time.frameCount)
		{
			if (!FreezeTopPosition)
			{
				RootBullet.Position = base.transform.position.XY();
				RootBullet.Direction = base.transform.rotation.eulerAngles.z;
			}
			RootBullet.TimeScale = BulletManager.TimeScale;
			RootBullet.FrameUpdate();
			m_lastUpdatedFrame = Time.frameCount;
		}
	}

	protected override void OnDestroy()
	{
		StaticReferenceManager.AllBulletScriptSources.Remove(this);
		base.OnDestroy();
	}

	public void ForceStop()
	{
		if (RootBullet != null)
		{
			RootBullet.ForceEnd();
			RootBullet.Destroyed = true;
			RootBullet = null;
		}
	}

	public void Initialize()
	{
		RootBullet = BulletScript.CreateInstance();
		if (RootBullet != null)
		{
			RootBullet.BulletManager = BulletManager;
			RootBullet.RootTransform = base.transform;
			RootBullet.Position = base.transform.position.XY();
			RootBullet.Direction = base.transform.rotation.eulerAngles.z;
			RootBullet.Initialize();
		}
		Update();
	}
}
