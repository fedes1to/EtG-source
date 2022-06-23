using UnityEngine;

[RequireComponent(typeof(dfCharacterMotorCS))]
public class dfMobileFPSInputController : MonoBehaviour
{
	public string joystickID = "LeftJoystick";

	private dfCharacterMotorCS motor;

	private void Awake()
	{
		motor = GetComponent<dfCharacterMotorCS>();
	}

	private void Update()
	{
		Vector2 joystickPosition = dfTouchJoystick.GetJoystickPosition(joystickID);
		Vector3 vector = new Vector3(joystickPosition.x, 0f, joystickPosition.y);
		if (vector != Vector3.zero)
		{
			float magnitude = vector.magnitude;
			vector /= magnitude;
			magnitude = Mathf.Min(1f, magnitude);
			magnitude *= magnitude;
			vector *= magnitude;
		}
		motor.inputMoveDirection = base.transform.rotation * vector;
	}
}
