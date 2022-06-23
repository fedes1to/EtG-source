using System;
using System.IO;
using FullInspector.Internal;
using UnityEngine;

namespace FullInspector
{
	public class fiSettings
	{
		public static bool PrettyPrintSerializedJson;

		public static CommentType DefaultCommentType;

		public static bool ForceDisplayInlineObjectEditor;

		public static bool EnableAnimation;

		public static bool ForceSaveAllAssetsOnSceneSave;

		public static bool ForceSaveAllAssetsOnRecompilation;

		public static bool ForceRestoreAllAssetsOnRecompilation;

		public static bool AutomaticReferenceInstantation;

		public static bool InspectorAutomaticReferenceInstantation;

		public static bool InspectorRequireShowInInspector;

		public static bool SerializeAutoProperties;

		public static bool EmitWarnings;

		public static bool EmitGraphMetadataCulls;

		public static float MinimumFoldoutHeight;

		public static bool EnableOpenScriptButton;

		public static bool ForceDisableMultithreadedSerialization;

		public static float LabelWidthPercentage;

		public static float LabelWidthOffset;

		public static float LabelWidthMax;

		public static float LabelWidthMin;

		public static int DefaultPageMinimumCollectionLength;

		public static string RootDirectory;

		public static string RootGeneratedDirectory;

		static fiSettings()
		{
			DefaultCommentType = CommentType.Info;
			EnableAnimation = true;
			InspectorAutomaticReferenceInstantation = true;
			SerializeAutoProperties = true;
			MinimumFoldoutHeight = 80f;
			EnableOpenScriptButton = true;
			LabelWidthPercentage = 0.45f;
			LabelWidthOffset = 30f;
			LabelWidthMax = 600f;
			DefaultPageMinimumCollectionLength = 20;
			RootDirectory = "Assets/FullInspector2/";
			foreach (fiSettingsProcessor assemblyInstance in fiRuntimeReflectionUtility.GetAssemblyInstances<fiSettingsProcessor>())
			{
				assemblyInstance.Process();
			}
			if (fiUtility.IsEditor)
			{
				EnsureRootDirectory();
			}
			if (RootGeneratedDirectory == null)
			{
				RootGeneratedDirectory = RootDirectory.TrimEnd('/') + "_Generated/";
			}
			if (fiUtility.IsEditor && !fiDirectory.Exists(RootGeneratedDirectory))
			{
				Debug.Log("Creating directory at " + RootGeneratedDirectory);
				fiDirectory.CreateDirectory(RootGeneratedDirectory);
			}
		}

		private static void EnsureRootDirectory()
		{
			if (RootDirectory == null || !fiDirectory.Exists(RootDirectory))
			{
				Debug.Log("Failed to find FullInspector root directory at \"" + RootDirectory + "\"; running scan to find it.");
				string text = FindDirectoryPathByName("Assets", "FullInspector2");
				if (text == null)
				{
					Debug.LogError("Unable to locate \"FullInspector2\" directory. Please make sure that Full Inspector is located within \"FullInspector2\"");
					return;
				}
				text = (RootDirectory = text.Replace('\\', '/').TrimEnd('/') + '/');
				Debug.Log("Found FullInspector at \"" + text + "\". Please add the following code to your project in a non-Editor folder:\n\n" + FormatCustomizerForNewPath(text));
			}
		}

		private static string FormatCustomizerForNewPath(string path)
		{
			return "using FullInspector;" + Environment.NewLine + Environment.NewLine + "public class UpdateFullInspectorRootDirectory : fiSettingsProcessor {" + Environment.NewLine + "    public void Process() {" + Environment.NewLine + "        fiSettings.RootDirectory = \"" + path + "\";" + Environment.NewLine + "    }" + Environment.NewLine + "}" + Environment.NewLine;
		}

		private static string FindDirectoryPathByName(string currentDirectory, string targetDirectory)
		{
			targetDirectory = Path.GetFileName(targetDirectory);
			foreach (string directory in fiDirectory.GetDirectories(currentDirectory))
			{
				if (Path.GetFileName(directory) == targetDirectory)
				{
					return directory;
				}
				string text = FindDirectoryPathByName(directory, targetDirectory);
				if (text != null)
				{
					return text;
				}
			}
			return null;
		}
	}
}
