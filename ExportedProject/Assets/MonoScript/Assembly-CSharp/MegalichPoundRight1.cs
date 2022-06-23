using FullInspector;

[InspectorDropdownName("Bosses/Megalich/PoundRight1")]
public class MegalichPoundRight1 : MegalichPound1
{
	protected override float FireDirection
	{
		get
		{
			return -1f;
		}
	}
}
