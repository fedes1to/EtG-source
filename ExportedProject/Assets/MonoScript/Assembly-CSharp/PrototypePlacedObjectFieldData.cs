using System;

[Serializable]
public class PrototypePlacedObjectFieldData
{
	public enum FieldType
	{
		FLOAT,
		BOOL
	}

	public FieldType fieldType;

	public string fieldName;

	public float floatValue;

	public bool boolValue;
}
