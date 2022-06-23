using System;
using UnityEngine;

public class DodgeRollSynergyProcessor : MonoBehaviour
{
	public bool LeavesGoopTrail;

	public CustomSynergyType GoopTrailRequiredSynergy;

	public GoopDefinition GoopTrailGoop;

	public float GoopTrailRadius;

	private PassiveItem m_item;

	private PlayerController m_player;

	private void Awake()
	{
		m_item = GetComponent<PassiveItem>();
		PassiveItem item = m_item;
		item.OnPickedUp = (Action<PlayerController>)Delegate.Combine(item.OnPickedUp, new Action<PlayerController>(HandlePickedUp));
	}

	private void HandlePickedUp(PlayerController obj)
	{
		m_player = obj;
		m_player.OnIsRolling += HandleRollFrame;
	}

	private void HandleRollFrame(PlayerController sourcePlayer)
	{
		if (LeavesGoopTrail && (bool)m_player && m_player.HasActiveBonusSynergy(GoopTrailRequiredSynergy))
		{
			if (!m_item || m_item.Owner != m_player)
			{
				m_player.OnIsRolling -= HandleRollFrame;
			}
			else
			{
				DeadlyDeadlyGoopManager.GetGoopManagerForGoopType(GoopTrailGoop).AddGoopCircle(m_player.specRigidbody.UnitCenter, GoopTrailRadius);
			}
		}
	}
}
