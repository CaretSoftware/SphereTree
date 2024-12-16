using UnityEngine;

namespace Ship_Controls {
	public class ShipControls : MonoBehaviour {
		[SerializeField] private float _rotationVelocity = 200.0f;
		[SerializeField] private float _smoothTimeRotationDamp = 0.25f;

		private Vector3 _rotationVector;
    
		private float _xVel;
		private float _yVel;
		private float _zVel;
    
		void Update() {
			DampRotationVector(InputVector());
			transform.Rotate(_rotationVelocity * Time.deltaTime * _rotationVector, Space.Self);
		}

		private Vector3 InputVector() {
			var inputVector = new Vector3();
	    
			// Pitch
			if (Input.GetKey(KeyCode.W)) inputVector.x += Time.deltaTime * _rotationVelocity;
			if (Input.GetKey(KeyCode.S)) inputVector.x -= Time.deltaTime * _rotationVelocity;
			// Yaw
			if (Input.GetKey(KeyCode.E)) inputVector.y += Time.deltaTime * _rotationVelocity;
			if (Input.GetKey(KeyCode.Q)) inputVector.y -= Time.deltaTime * _rotationVelocity;
			// Roll
			if (Input.GetKey(KeyCode.A)) inputVector.z += Time.deltaTime * _rotationVelocity;
			if (Input.GetKey(KeyCode.D)) inputVector.z -= Time.deltaTime * _rotationVelocity;
	    
			return inputVector;
		}

		private void DampRotationVector(Vector3 inputVector) {
			_rotationVector = new Vector3(
					Mathf.SmoothDampAngle(_rotationVector.x, inputVector.x, ref _xVel, _smoothTimeRotationDamp),
					Mathf.SmoothDampAngle(_rotationVector.y, inputVector.y, ref _yVel, _smoothTimeRotationDamp),
					Mathf.SmoothDampAngle(_rotationVector.z, inputVector.z, ref _zVel, _smoothTimeRotationDamp)
			);
		}
	}
}
