using FullInspector;

[InspectorDropdownName("Bosses/Megalich/PoundLeft1")]
public class MegalichPoundLeft1 : MegalichPound1
{
	protected override float FireDirection
	{
		get
		{
			return 1f;
		}
	}
}
