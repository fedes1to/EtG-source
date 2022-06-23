using System.IO;
using System.Text;
using UnityEngine;

namespace Dungeonator
{
	public class FlowBuilderDebugger
	{
		protected StringBuilder builder;

		public FlowBuilderDebugger()
		{
			builder = new StringBuilder();
		}

		public void Log(string s)
		{
			builder.AppendLine(s);
		}

		public void Log(RoomHandler parent, RoomHandler child)
		{
			builder.AppendLine(parent.area.prototypeRoom.name + " built " + child.area.prototypeRoom.name);
		}

		public void LogMonoHeapStatus()
		{
		}

		public void FinalizeLog()
		{
			string text = Application.dataPath + "\\dungeonDebug.txt";
			if (File.Exists(text))
			{
				FileInfo fileInfo = new FileInfo(text);
				fileInfo.IsReadOnly = false;
				File.Delete(text);
			}
			StreamWriter streamWriter = new StreamWriter(text);
			streamWriter.WriteLine(builder.ToString());
			streamWriter.Close();
		}
	}
}
