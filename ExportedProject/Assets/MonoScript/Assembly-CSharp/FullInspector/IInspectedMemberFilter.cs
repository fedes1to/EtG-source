namespace FullInspector
{
	public interface IInspectedMemberFilter
	{
		bool IsInterested(InspectedProperty property);

		bool IsInterested(InspectedMethod method);
	}
}
