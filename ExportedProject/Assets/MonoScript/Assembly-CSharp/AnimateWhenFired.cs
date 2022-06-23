using Brave.BulletScript;
using UnityEngine;

public class AnimateWhenFired : BraveBehaviour
{
	public enum TriggerType
	{
		BulletBankTransform
	}

	[Header("Trigger")]
	public TriggerType trigger;

	[ShowInInspectorIf("trigger", 0, false)]
	public AIBulletBank specifyBulletBank;

	[ShowInInspectorIf("trigger", 0, false)]
	public string transformName;

	[Header("Animation")]
	public AIAnimator specifyAiAnimator;

	[CheckDirectionalAnimation(null)]
	public string fireAnim;

	public void Start()
	{
		if (!specifyAiAnimator)
		{
			specifyAiAnimator = base.aiAnimator;
		}
		if (trigger == TriggerType.BulletBankTransform)
		{
			if (!specifyBulletBank)
			{
				specifyBulletBank = base.bulletBank;
			}
			specifyBulletBank.OnBulletSpawned += BulletSpawned;
		}
	}

	protected override void OnDestroy()
	{
		if (trigger == TriggerType.BulletBankTransform && (bool)specifyBulletBank)
		{
			specifyBulletBank.OnBulletSpawned += BulletSpawned;
		}
		base.OnDestroy();
	}

	private void BulletSpawned(Bullet bullet, Projectile projectile)
	{
		if (transformName == bullet.SpawnTransform)
		{
			specifyAiAnimator.PlayUntilFinished(fireAnim);
		}
	}
}
