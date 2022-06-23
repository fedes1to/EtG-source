using UnityEngine;

public class NormalDistributionDecoratorAttribute : PropertyAttribute
{
	public string MeanProperty;

	public string StdDevProperty;

	public NormalDistributionDecoratorAttribute(string meanPropertyName, string devPropertyName)
	{
		MeanProperty = meanPropertyName;
		StdDevProperty = devPropertyName;
	}
}
