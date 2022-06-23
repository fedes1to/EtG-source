using System;
using System.Collections.Generic;
using UnityEngine;

public class AlphabetSoupSynergyProcessor : MonoBehaviour
{
	public AlphabetSoupEntry[] Entries;

	private Gun m_gun;

	private string m_currentEntry = "BULLET";

	private int m_currentEntryCount;

	private bool m_hasReplacedProjectileList;

	private bool m_hasPlayedAudioThisShot;

	private string m_currentAudioEvent = "Play_WPN_rgun_bullet_01";

	public void Awake()
	{
		m_gun = GetComponent<Gun>();
		Gun gun = m_gun;
		gun.PostProcessVolley = (Action<ProjectileVolleyData>)Delegate.Combine(gun.PostProcessVolley, new Action<ProjectileVolleyData>(HandlePostProcessVolley));
		Gun gun2 = m_gun;
		gun2.PostProcessProjectile = (Action<Projectile>)Delegate.Combine(gun2.PostProcessProjectile, new Action<Projectile>(HandlePostProcessProjectile));
		Gun gun3 = m_gun;
		gun3.OnReloadPressed = (Action<PlayerController, Gun, bool>)Delegate.Combine(gun3.OnReloadPressed, new Action<PlayerController, Gun, bool>(HandleReloadPressed));
		Gun gun4 = m_gun;
		gun4.OnPostFired = (Action<PlayerController, Gun>)Delegate.Combine(gun4.OnPostFired, new Action<PlayerController, Gun>(HandlePostFired));
		Gun gun5 = m_gun;
		gun5.OnFinishAttack = (Action<PlayerController, Gun>)Delegate.Combine(gun5.OnFinishAttack, new Action<PlayerController, Gun>(HandleFinishAttack));
		Gun gun6 = m_gun;
		gun6.OnBurstContinued = (Action<PlayerController, Gun>)Delegate.Combine(gun6.OnBurstContinued, new Action<PlayerController, Gun>(HandleBurstContinued));
	}

	private void HandleBurstContinued(PlayerController arg1, Gun arg2)
	{
		if (!m_gun || m_gun.gunClass != GunClass.EXPLOSIVE)
		{
			HandleFinishAttack(arg1, arg2);
		}
	}

	private void HandlePostFired(PlayerController arg1, Gun arg2)
	{
		if ((!m_gun || m_gun.gunClass != GunClass.EXPLOSIVE) && !m_hasPlayedAudioThisShot)
		{
			m_hasPlayedAudioThisShot = true;
			AkSoundEngine.PostEvent(m_currentAudioEvent, arg2.gameObject);
		}
	}

	private void HandlePostProcessVolley(ProjectileVolleyData obj)
	{
		m_currentEntryCount = 0;
	}

	private void HandleReloadPressed(PlayerController arg1, Gun arg2, bool arg3)
	{
		m_currentEntryCount = 0;
	}

	private string GetLetterForWordPosition(string word)
	{
		if (m_currentEntryCount < 0 || m_currentEntryCount >= word.Length)
		{
			return "word_projectile_B_001";
		}
		switch (word[m_currentEntryCount])
		{
		case 'A':
			return "word_projectile_A_001";
		case 'B':
			return "word_projectile_B_001";
		case 'C':
			return "word_projectile_C_001";
		case 'D':
			return "word_projectile_D_001";
		case 'E':
			return "word_projectile_B_004";
		case 'F':
			return "word_projectile_F_001";
		case 'G':
			return "word_projectile_G_001";
		case 'H':
			return "word_projectile_H_001";
		case 'I':
			return "word_projectile_I_001";
		case 'J':
			return "word_projectile_J_001";
		case 'K':
			return "word_projectile_K_001";
		case 'L':
			return "word_projectile_B_003";
		case 'M':
			return "word_projectile_M_001";
		case 'N':
			return "word_projectile_N_001";
		case 'O':
			return "word_projectile_O_001";
		case 'P':
			return "word_projectile_P_001";
		case 'Q':
			return "word_projectile_Q_001";
		case 'R':
			return "word_projectile_R_001";
		case 'S':
			return "word_projectile_S_001";
		case 'T':
			return "word_projectile_B_005";
		case 'U':
			return "word_projectile_B_002";
		case 'V':
			return "word_projectile_V_001";
		case 'W':
			return "word_projectile_W_001";
		case 'X':
			return "word_projectile_X_001";
		case 'Y':
			return "word_projectile_Y_001";
		case 'Z':
			return "word_projectile_Z_001";
		case 'a':
			return "word_projectile_alpha_001";
		case 'o':
			return "word_projectile_omega_001";
		case '+':
			return "word_projectile_+_001";
		case '1':
			return "word_projectile_1_001";
		default:
			return "word_projectile_B_001";
		}
	}

	private void HandlePostProcessProjectile(Projectile targetProjectile)
	{
		if ((bool)targetProjectile && (bool)targetProjectile.sprite && (!m_gun || m_gun.gunClass != GunClass.EXPLOSIVE))
		{
			targetProjectile.sprite.SetSprite(GetLetterForWordPosition(m_currentEntry));
			m_currentEntryCount++;
		}
	}

	private void HandleFinishAttack(PlayerController sourcePlayer, Gun sourceGun)
	{
		if ((bool)m_gun && m_gun.gunClass == GunClass.EXPLOSIVE)
		{
			return;
		}
		m_hasPlayedAudioThisShot = false;
		int num = UnityEngine.Random.Range(0, Entries.Length);
		AlphabetSoupEntry alphabetSoupEntry = null;
		for (int i = num; i < num + Entries.Length; i++)
		{
			AlphabetSoupEntry alphabetSoupEntry2 = Entries[i % Entries.Length];
			if (sourcePlayer.HasActiveBonusSynergy(alphabetSoupEntry2.RequiredSynergy))
			{
				alphabetSoupEntry = alphabetSoupEntry2;
				break;
			}
		}
		if (alphabetSoupEntry != null)
		{
			ProcessVolley(m_gun.modifiedVolley, alphabetSoupEntry);
			return;
		}
		m_currentEntryCount = 0;
		m_currentEntry = "BULLET";
		m_currentAudioEvent = "Play_WPN_rgun_bullet_01";
	}

	private void ProcessVolley(ProjectileVolleyData currentVolley, AlphabetSoupEntry entry)
	{
		if (!m_gun || m_gun.gunClass != GunClass.EXPLOSIVE)
		{
			ProjectileModule projectileModule = currentVolley.projectiles[0];
			projectileModule.ClearOrderedProjectileData();
			if (!m_hasReplacedProjectileList)
			{
				m_hasReplacedProjectileList = true;
				projectileModule.projectiles = new List<Projectile>();
			}
			projectileModule.projectiles.Clear();
			int num = UnityEngine.Random.Range(0, entry.Words.Length);
			m_currentEntry = entry.Words[num];
			m_currentAudioEvent = entry.AudioEvents[num];
			projectileModule.burstShotCount = m_currentEntry.Length;
			for (int i = 0; i < m_currentEntry.Length; i++)
			{
				projectileModule.projectiles.Add(entry.BaseProjectile);
			}
			m_currentEntryCount = 0;
		}
	}
}
