using FullInspector;

[InspectorDropdownName("Bosses/DraGun/GlockDirectedHardRight2")]
public class DraGunGlockDirectedHardRight2 : DraGunGlockDirected2
{
	protected override string BulletName
	{
		get
		{
			return "glockRight";
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
