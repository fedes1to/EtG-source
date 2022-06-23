using Dungeonator;
using UnityEngine;

public class GooperSynergyProcessor : MonoBehaviour
{
	[LongNumericEnum]
	public CustomSynergyType RequiredSynergy;

	public GoopDefinition goopDefinition;

	public float goopRadius;

	public DamageTypeModifier[] modifiers;

	private PassiveItem m_item;

	private PlayerController m_player;

	private DeadlyDeadlyGoopManager m_manager;

	private bool m_initialized;

	public void Awake()
	{
		m_item = GetComponent<PassiveItem>();
		m_manager = DeadlyDeadlyGoopManager.GetGoopManagerForGoopType(goopDefinition);
	}

	private void Initialize(PlayerController p)
	{
		if (!m_initialized)
		{
			m_initialized = true;
			p.OnIsRolling += HandleRollFrame;
			for (int i = 0; i < modifiers.Length; i++)
			{
				p.healthHaver.damageTypeModifiers.Add(modifiers[i]);
			}
			m_player = p;
		}
	}

	private void Uninitialize()
	{
		if (m_initialized)
		{
			m_initialized = false;
			m_player.OnIsRolling -= HandleRollFrame;
			for (int i = 0; i < modifiers.Length; i++)
			{
				m_player.healthHaver.damageTypeModifiers.Remove(modifiers[i]);
			}
			m_player = null;
		}
	}

	private void Update()
	{
		if (Dungeon.IsGenerating)
		{
			m_manager = null;
			return;
		}
		if (!GameManager.HasInstance || !GameManager.Instance.Dungeon)
		{
			m_manager = null;
			return;
		}
		if (!m_manager)
		{
			m_manager = DeadlyDeadlyGoopManager.GetGoopManagerForGoopType(goopDefinition);
		}
		if (m_initialized)
		{
			if (((bool)m_item && !m_item.Owner) || !m_item.Owner.HasActiveBonusSynergy(RequiredSynergy))
			{
				Uninitialize();
			}
		}
		else if ((bool)m_item && (bool)m_item.Owner && m_item.Owner.HasActiveBonusSynergy(RequiredSynergy))
		{
			Initialize(m_item.Owner);
		}
	}

	private void HandleRollFrame(PlayerController p)
	{
		if (!GameManager.Instance.IsFoyer && !GameManager.Instance.Dungeon.IsEndTimes)
		{
			m_manager.AddGoopCircle(p.specRigidbody.UnitCenter, goopRadius);
		}
	}
}
