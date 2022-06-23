using FullInspector;

[InspectorDropdownName("Bosses/DraGun/GlockDirectedHardLeft2")]
public class DraGunGlockDirectedHardLeft2 : DraGunGlockDirected2
{
	protected override string BulletName
	{
		get
		{
			return "glockLeft";
		}
	}

	protected override bool IsHard
	{
		get
		{
			return true;
		}
	}
}
