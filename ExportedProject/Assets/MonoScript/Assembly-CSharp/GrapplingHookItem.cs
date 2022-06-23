using System;
using UnityEngine;

public class GrapplingHookItem : PlayerItem
{
	public GameObject GrapplePrefab;

	public float GrappleSpeed = 10f;

	public float GrappleRetractSpeed = 10f;

	public float DamageToEnemies = 10f;

	public float EnemyKnockbackForce = 10f;

	private GrappleModule m_grappleModule;

	private void Awake()
	{
		InitializeModule();
	}

	private void InitializeModule()
	{
		m_grappleModule = new GrappleModule();
		m_grappleModule.GrapplePrefab = GrapplePrefab;
		m_grappleModule.GrappleSpeed = GrappleSpeed;
		m_grappleModule.GrappleRetractSpeed = GrappleRetractSpeed;
		m_grappleModule.DamageToEnemies = DamageToEnemies;
		m_grappleModule.EnemyKnockbackForce = EnemyKnockbackForce;
		m_grappleModule.sourceGameObject = base.gameObject;
		GrappleModule grappleModule = m_grappleModule;
		grappleModule.FinishedCallback = (Action)Delegate.Combine(grappleModule.FinishedCallback, new Action(GrappleEndedNaturally));
	}

	protected override void DoEffect(PlayerController user)
	{
		AkSoundEngine.PostEvent("Play_OBJ_hook_shot_01", base.gameObject);
		base.IsCurrentlyActive = true;
		m_grappleModule.Trigger(user);
	}

	protected void GrappleEndedNaturally()
	{
		base.IsCurrentlyActive = false;
	}

	protected override void DoActiveEffect(PlayerController user)
	{
		m_grappleModule.MarkDone();
	}

	protected override void OnPreDrop(PlayerController user)
	{
		m_grappleModule.ClearExtantGrapple();
		base.IsCurrentlyActive = false;
	}

	public override void OnItemSwitched(PlayerController user)
	{
		m_grappleModule.ForceEndGrapple();
		m_grappleModule.ClearExtantGrapple();
		base.IsCurrentlyActive = false;
	}

	protected override void OnDestroy()
	{
		m_grappleModule.ClearExtantGrapple();
		base.OnDestroy();
	}
}
