using System;
using UnityEngine;

namespace FullInspector
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Field)]
	public sealed class fiInspectorOnlyAttribute : PropertyAttribute
	{
	}
}
