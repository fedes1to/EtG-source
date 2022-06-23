using FullInspector;

[InspectorDropdownName("Bosses/DraGun/GlockDirectedHardRight1")]
public class DraGunGlockDirectedHardRight1 : DraGunGlockDirected1
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
