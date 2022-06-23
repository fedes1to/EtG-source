using FullInspector;

[InspectorDropdownName("Bosses/MetalGearRat/SidePoundRight1")]
public class MetalGearRatSidePoundRight1 : MetalGearRatSidePound1
{
	protected override float StartAngle
	{
		get
		{
			return 80f;
		}
	}

	protected override float SweepAngle
	{
		get
		{
			return -100f;
		}
	}
}
