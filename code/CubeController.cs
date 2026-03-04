using Sandbox;

public class CubeFloatAttribute : System.Attribute {
}

public sealed class CubeController : Component {
	[Property, CubeFloat] public float MoveSpeed { get; set; } = 100.0f;

	protected override void OnUpdate() {
		// get the current rotation as pitch yaw roll angles
		var angles = WorldRotation.Angles();

		// add the pitch and yaw from the input
		angles += Input.AnalogLook;

		// zero the pitch and roll - because we just want it to yaw
		angles.pitch = 0;
		angles.roll = 0;

		// set the new rotation
		WorldRotation = angles;

		// Create a vector3 to hold our move direction
		var moveDirection = Vector3.Zero;

		// Move in the direction of our input. AnalogMove gets built automatically from
		// "forward" "left" etc buttons. It also grabs the controller stick input
		// Multiply this by the configured move speed and the time delta to make it frame rate independent
		moveDirection += Input.AnalogMove * MoveSpeed * Time.Delta;

		// Multiply the movement direction by our rotation so we move in the direction we're facing, and
		// add it to our current position.
		WorldPosition += WorldRotation * moveDirection;
	}
}
