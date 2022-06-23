using FullInspector;

[InspectorDropdownName("Bosses/DraGun/GlockDirectedHardLeft1")]
public class DraGunGlockDirectedHardLeft1 : DraGunGlockDirected1
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
