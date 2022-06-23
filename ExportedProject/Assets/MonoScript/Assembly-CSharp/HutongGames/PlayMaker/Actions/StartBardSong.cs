using System.Collections;
using System.Collections.Generic;
using Beebyte.Obfuscator;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(".NPCs")]
	[Tooltip("Plays a robot bard song.")]
	public class StartBardSong : FsmStateAction
	{
		[Skip]
		public enum BardSong
		{
			DAMAGE_BOOST,
			SPEED_BOOST
		}

		public bool HasDuration;

		public float Duration = 120f;

		public bool LimitedToFloor = true;

		[CompoundArray("Songs", "Song Type", "Dialogue")]
		public BardSong[] songsToChooseFrom;

		public FsmString[] songDialogues;

		public FsmString targetDialogueVariable;

		public override void OnEnter()
		{
			TalkDoerLite component = base.Owner.GetComponent<TalkDoerLite>();
			PlayerController talkingPlayer = component.TalkingPlayer;
			int num = Random.Range(0, songsToChooseFrom.Length);
			ApplySongToPlayer(talkingPlayer, songsToChooseFrom[num]);
			targetDialogueVariable.Value = songDialogues[num].Value;
			Finish();
		}

		protected void ApplySongToPlayer(PlayerController targetPlayer, BardSong targetSong)
		{
			List<StatModifier> list = new List<StatModifier>();
			switch (targetSong)
			{
			case BardSong.DAMAGE_BOOST:
			{
				StatModifier statModifier2 = new StatModifier();
				statModifier2.statToBoost = PlayerStats.StatType.Damage;
				statModifier2.amount = 1.1f;
				statModifier2.modifyType = StatModifier.ModifyMethod.MULTIPLICATIVE;
				list.Add(statModifier2);
				break;
			}
			case BardSong.SPEED_BOOST:
			{
				StatModifier statModifier = new StatModifier();
				statModifier.statToBoost = PlayerStats.StatType.MovementSpeed;
				statModifier.amount = 1f;
				statModifier.modifyType = StatModifier.ModifyMethod.ADDITIVE;
				list.Add(statModifier);
				break;
			}
			}
			for (int i = 0; i < list.Count; i++)
			{
				targetPlayer.ownerlessStatModifiers.Add(list[i]);
			}
			targetPlayer.stats.RecalculateStats(targetPlayer);
			if (HasDuration || LimitedToFloor)
			{
				targetPlayer.StartCoroutine(HandleSongLifetime(targetPlayer, targetSong, list));
			}
		}

		private IEnumerator HandleSongLifetime(PlayerController targetPlayer, BardSong targetSong, List<StatModifier> activeModifiers)
		{
			float elapsed = 0f;
			while (true)
			{
				elapsed += BraveTime.DeltaTime;
				if ((HasDuration && elapsed > Duration) || (LimitedToFloor && GameManager.Instance.IsLoadingLevel))
				{
					break;
				}
				yield return null;
			}
		}
	}
}
