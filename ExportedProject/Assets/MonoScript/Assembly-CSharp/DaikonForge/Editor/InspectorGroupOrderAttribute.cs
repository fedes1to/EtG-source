using System;
using System.Collections.Generic;

namespace DaikonForge.Editor
{
	[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
	public class InspectorGroupOrderAttribute : Attribute
	{
		public List<string> Groups = new List<string>();

		public InspectorGroupOrderAttribute(params string[] groups)
		{
			Groups.AddRange(groups);
		}
	}
}
