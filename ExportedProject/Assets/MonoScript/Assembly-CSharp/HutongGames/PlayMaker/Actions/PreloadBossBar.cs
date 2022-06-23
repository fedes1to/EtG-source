namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(".NPCs")]
	public class PreloadBossBar : FsmStateAction
	{
		public override void OnEnter()
		{
			GameUIRoot.Instance.bossController.ForceUpdateBossHealth(100f, 100f, StringTableManager.GetEnemiesString("#MANFRED_ENCNAME"));
			GameManager.Instance.DungeonMusicController.SwitchToBossMusic("Play_MUS_Boss_Theme_Gull", base.Owner.gameObject);
			Finish();
		}
	}
}
