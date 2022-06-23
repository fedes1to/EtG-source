using UnityEngine;

public class ModifyBeamSynergyProcessor : MonoBehaviour
{
	public CustomSynergyType SynergyToCheck;

	public bool AddsFreezeEffect;

	[ShowInInspectorIf("AddsFreezeEffect", false)]
	public GameActorFreezeEffect FreezeEffect;

	private Projectile m_projectile;

	private BeamController m_beam;

	public void Awake()
	{
		m_projectile = GetComponent<Projectile>();
		m_beam = GetComponent<BeamController>();
	}

	public void Start()
	{
		PlayerController playerController = m_projectile.Owner as PlayerController;
		if ((bool)playerController && playerController.HasActiveBonusSynergy(SynergyToCheck) && AddsFreezeEffect)
		{
			m_projectile.AppliesFreeze = true;
			m_projectile.FreezeApplyChance = 1f;
			m_projectile.freezeEffect = FreezeEffect;
			m_projectile.damageTypes |= CoreDamageTypes.Ice;
			m_beam.statusEffectChance = 1f;
			m_beam.statusEffectAccumulateMultiplier = 1f;
		}
	}
}
