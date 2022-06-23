using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/DraGun/RoarBehavior")]
public class DraGunRoarBehavior : BasicAttackBehavior
{
	public GameObject ShootPoint;

	public BulletScriptSelector BulletScript;

	private DraGunController m_dragun;

	private tk2dSpriteAnimator m_roarDummy;

	private BulletScriptSource m_bulletSource;

	private float m_timer;

	public override void Start()
	{
		base.Start();
		m_dragun = m_aiActor.GetComponent<DraGunController>();
		m_roarDummy = m_aiActor.transform.Find("RoarDummy").GetComponent<tk2dSpriteAnimator>();
	}

	public override void Upkeep()
	{
		base.Upkeep();
		DecrementTimer(ref m_timer);
	}

	public override BehaviorResult Update()
	{
		BehaviorResult behaviorResult = base.Update();
		if (behaviorResult != 0)
		{
			return behaviorResult;
		}
		if (!IsReady())
		{
			return BehaviorResult.Continue;
		}
		m_aiActor.ToggleRenderers(false);
		m_dragun.head.OverrideDesiredPosition = m_aiActor.transform.position + new Vector3(3.63f, 10.8f);
		m_roarDummy.gameObject.SetActive(true);
		m_roarDummy.GetComponent<Renderer>().enabled = true;
		m_roarDummy.Play("roar");
		Fire();
		m_updateEveryFrame = true;
		return BehaviorResult.RunContinuous;
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		base.ContinuousUpdate();
		if (!m_roarDummy.IsPlaying("roar"))
		{
			m_roarDummy.Play("blank");
			m_roarDummy.gameObject.SetActive(false);
			m_aiActor.ToggleRenderers(true);
			m_dragun.head.OverrideDesiredPosition = null;
			return ContinuousBehaviorResult.Finished;
		}
		return ContinuousBehaviorResult.Continue;
	}

	public override void EndContinuousUpdate()
	{
		base.EndContinuousUpdate();
		m_roarDummy.Play("blank");
		m_roarDummy.gameObject.SetActive(false);
		m_aiActor.ToggleRenderers(true);
		m_dragun.head.OverrideDesiredPosition = null;
		m_updateEveryFrame = false;
		UpdateCooldowns();
	}

	private void Fire()
	{
		if (!m_bulletSource)
		{
			m_bulletSource = ShootPoint.GetOrAddComponent<BulletScriptSource>();
		}
		m_bulletSource.BulletManager = m_aiActor.bulletBank;
		m_bulletSource.BulletScript = BulletScript;
		m_bulletSource.Initialize();
	}
}
