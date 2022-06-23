using Dungeonator;
using UnityEngine;

public class BreakableShieldController : BraveBehaviour, SingleSpawnableGunPlacedObject
{
	[CheckAnimation(null)]
	public string introAnimation;

	[CheckAnimation(null)]
	public string idleAnimation;

	[CheckAnimation(null)]
	public string idleBreak1Animation;

	[CheckAnimation(null)]
	public string idleBreak2Animation;

	[CheckAnimation(null)]
	public string idleBreak3Animation;

	public float maxDuration = 60f;

	private float m_elapsed;

	private PlayerController ownerPlayer;

	private RoomHandler m_room;

	public void Deactivate()
	{
		base.majorBreakable.Break(Vector2.zero);
	}

	public void Initialize(Gun sourceGun)
	{
		if ((bool)sourceGun && (bool)sourceGun.CurrentOwner)
		{
			ownerPlayer = sourceGun.CurrentOwner as PlayerController;
			base.transform.position = sourceGun.CurrentOwner.CenterPosition.ToVector3ZUp();
			base.specRigidbody.Reinitialize();
		}
		m_room = base.transform.position.GetAbsoluteRoom();
		base.spriteAnimator.Play(introAnimation);
	}

	private void HandleIdleAnimation()
	{
		if (!base.spriteAnimator.IsPlaying(introAnimation))
		{
			float currentHealthPercentage = base.majorBreakable.GetCurrentHealthPercentage();
			string text = idleAnimation;
			if (currentHealthPercentage < 0.25f)
			{
				text = idleBreak3Animation;
			}
			else if (currentHealthPercentage < 0.5f)
			{
				text = idleBreak2Animation;
			}
			else if (currentHealthPercentage < 0.75f)
			{
				text = idleBreak1Animation;
			}
			if (!base.spriteAnimator.IsPlaying(text))
			{
				base.spriteAnimator.Play(text);
			}
		}
	}

	private void Update()
	{
		if (!base.majorBreakable.IsDestroyed)
		{
			m_elapsed += BraveTime.DeltaTime;
			HandleIdleAnimation();
			if (m_elapsed > maxDuration)
			{
				base.majorBreakable.Break(Vector2.zero);
			}
			if ((bool)ownerPlayer && ownerPlayer.CurrentRoom != m_room)
			{
				base.majorBreakable.Break(Vector2.zero);
			}
		}
	}
}
