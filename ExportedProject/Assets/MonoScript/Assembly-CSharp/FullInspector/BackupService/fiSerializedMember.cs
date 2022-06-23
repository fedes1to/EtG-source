using System;

namespace FullInspector.BackupService
{
	[Serializable]
	public class fiSerializedMember
	{
		public string Name;

		public string Value;

		public fiEnableRestore ShouldRestore = new fiEnableRestore
		{
			Enabled = true
		};
	}
}
