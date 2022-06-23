using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[AddComponentMenu("Daikon Forge/Examples/Actionbar/View Model")]
public class ActionBarViewModel : MonoBehaviour
{
	public delegate void SpellEventHandler(SpellDefinition spell);

	private class SpellCastInfo
	{
		public SpellDefinition spell;

		public float whenCast;
	}

	[SerializeField]
	private float _health;

	[SerializeField]
	private int _maxHealth = 100;

	[SerializeField]
	private float _healthRegenRate = 0.5f;

	[SerializeField]
	private float _energy;

	[SerializeField]
	private int _maxEnergy = 100;

	[SerializeField]
	private float _energyRegenRate = 1f;

	private List<SpellCastInfo> activeSpells = new List<SpellCastInfo>();

	public int MaxHealth
	{
		get
		{
			return _maxHealth;
		}
	}

	public int MaxEnergy
	{
		get
		{
			return _maxEnergy;
		}
	}

	public int Health
	{
		get
		{
			return (int)_health;
		}
		private set
		{
			_health = Mathf.Max(0, Mathf.Min(_maxHealth, value));
		}
	}

	public int Energy
	{
		get
		{
			return (int)_energy;
		}
		private set
		{
			_energy = Mathf.Max(0, Mathf.Min(_maxEnergy, value));
		}
	}

	public event SpellEventHandler SpellActivated;

	public event SpellEventHandler SpellDeactivated;

	private void OnEnable()
	{
	}

	private void Start()
	{
		_health = 35f;
		_energy = 50f;
	}

	private void Update()
	{
		_health = Mathf.Min(_maxHealth, _health + BraveTime.DeltaTime * _healthRegenRate);
		_energy = Mathf.Min(_maxEnergy, _energy + BraveTime.DeltaTime * _energyRegenRate);
		for (int num = activeSpells.Count - 1; num >= 0; num--)
		{
			SpellCastInfo spellCastInfo = activeSpells[num];
			float num2 = Time.realtimeSinceStartup - spellCastInfo.whenCast;
			if (spellCastInfo.spell.Recharge <= num2)
			{
				activeSpells.RemoveAt(num);
				if (this.SpellDeactivated != null)
				{
					this.SpellDeactivated(spellCastInfo.spell);
				}
			}
		}
	}

	public void CastSpell(string spellName)
	{
		SpellDefinition spell = SpellDefinition.FindByName(spellName);
		if (spell == null)
		{
			throw new InvalidCastException();
		}
		if (!activeSpells.Any((SpellCastInfo activeSpell) => activeSpell.spell == spell) && Energy >= spell.Cost)
		{
			Energy -= spell.Cost;
			activeSpells.Add(new SpellCastInfo
			{
				spell = spell,
				whenCast = Time.realtimeSinceStartup
			});
			if (this.SpellActivated != null)
			{
				this.SpellActivated(spell);
			}
		}
	}
}
