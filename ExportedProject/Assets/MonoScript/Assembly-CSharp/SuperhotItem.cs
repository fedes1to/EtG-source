using UnityEngine;

public class SuperhotItem : PassiveItem
{
	private bool m_active;

	protected override void Update()
	{
		base.Update();
		if (m_pickedUp && !GameManager.Instance.IsLoadingLevel && m_owner != null && (m_owner.CurrentInputState == PlayerInputState.AllInput || m_owner.CurrentInputState == PlayerInputState.OnlyMovement) && !m_owner.IsFalling && (bool)m_owner.healthHaver && !m_owner.healthHaver.IsDead)
		{
			m_active = true;
			float num = Mathf.Clamp01(m_owner.specRigidbody.Velocity.magnitude / m_owner.stats.MovementSpeed);
			if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
			{
				PlayerController otherPlayer = GameManager.Instance.GetOtherPlayer(m_owner);
				if ((bool)otherPlayer && (bool)otherPlayer.specRigidbody)
				{
					num = Mathf.Max(num, Mathf.Clamp01(otherPlayer.specRigidbody.Velocity.magnitude / otherPlayer.stats.MovementSpeed));
				}
			}
			float multiplier = Mathf.Lerp(0.01f, 1f, num);
			if (m_owner.IsDodgeRolling)
			{
				multiplier = 1f;
			}
			BraveTime.SetTimeScaleMultiplier(multiplier, base.gameObject);
		}
		else if (m_active)
		{
			m_active = false;
			BraveTime.ClearMultiplier(base.gameObject);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
