using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceScript : MonoBehaviour {

	private Rigidbody rb;
	public Vector3 diceVelocity;
	[SerializeField]
	public int diceId;

	private Vector3 initialPosition;
	private Quaternion initialRotation;

	private bool isSettled = false;

	// Use this for initialization
	void Start () {
		rb = GetComponent<Rigidbody> ();
		initialPosition = transform.position;
		initialRotation = transform.rotation;
	} 
	
	// Update is called once per frame
	void Update () {
		diceVelocity = rb.linearVelocity;

		// Modified ground constraint logic
		if (transform.position.y < 0.01f) {  // Small threshold instead of exact 0
			Vector3 pos = transform.position;
			pos.y = 0;
			transform.position = pos;
			
			// Only set velocities if not kinematic
			if (!rb.isKinematic) {
				rb.linearVelocity = Vector3.zero;
				rb.angularVelocity = Vector3.zero;
			}
		}
	}

	void OnCollisionEnter(Collision collision) {
		DiceScript otherDice = collision.gameObject.GetComponent<DiceScript>();
		if (otherDice != null && !rb.isKinematic) {
			// Increased dampening for dice collisions
			rb.linearVelocity *= 0.5f;
			rb.angularVelocity *= 0.5f;
		}
	}

	public void RollToTarget(int targetFace) {
		Debug.Log($"Die {diceId} rolling to target face: {targetFace}");
		
		// Reset position and physics state
		Vector3 startPos = initialPosition;
		startPos.y += 1.5f; // Slightly lower starting height
		startPos.z -= 1f;   // Start slightly forward
		transform.position = startPos;
		transform.rotation = initialRotation;
		
		rb.isKinematic = false;
		
		rb.linearVelocity = Vector3.zero;
		rb.angularVelocity = Vector3.zero;

		// Create boomerang effect
		float upwardForce = -100f;
		float forwardForce = 300f;  // Negative Z is forward
		float backwardForce = -300f;  // This creates the return effect
		
		// Initial burst forward and up
		rb.AddForce(new Vector3(
			Random.Range(-50f, 50f),  // Small random sideways motion
			upwardForce,              // Strong upward force
			forwardForce              // Initial forward movement
		));
		
		// Add delayed backward force for return effect
		StartCoroutine(AddDelayedBackwardForce(backwardForce));

		// Calculate rotation needed
		Quaternion targetRotation = GetRotationForFace(targetFace);
		Quaternion rotationDifference = targetRotation * Quaternion.Inverse(transform.rotation);
		rotationDifference.ToAngleAxis(out float rotationAngle, out Vector3 rotationAxis);
		
		rotationAngle += 720f;
		rb.AddTorque(rotationAxis.normalized * rotationAngle * 2f);

		StartCoroutine(EnsureTargetFace(targetFace));
	}

	private IEnumerator AddDelayedBackwardForce(float force) {
		yield return new WaitForSeconds(0.2f); // Short delay before return force
		rb.AddForce(Vector3.forward * force);  // Add force toward back wall
	}

	private IEnumerator EnsureTargetFace(int targetFace) {
		isSettled = false;
		
		float timeoutDuration = 3.0f;
		float elapsedTime = 0f;
		
		while (elapsedTime < timeoutDuration) {
			int currentFace = GetCurrentFace();
			
			// If we're close to stopping, help it settle
			if (rb.linearVelocity.magnitude < 0.1f && rb.angularVelocity.magnitude < 0.1f) {
				// Get current position and rotation
				Vector3 currentPos = transform.position;
				Quaternion currentRot = transform.rotation;
				
				// Calculate target position (keeping x,z but setting y to 0)
				Vector3 targetPos = currentPos;
				targetPos.y = 0;
				
				// Smoothly interpolate to final position and rotation
				transform.position = Vector3.Lerp(currentPos, targetPos, Time.deltaTime * 2f);
				transform.rotation = Quaternion.Lerp(currentRot, GetRotationForFace(targetFace), Time.deltaTime * 2f);
				
				// If we're very close to target, finalize
				if (Vector3.Distance(transform.position, targetPos) < 0.01f &&
					Quaternion.Angle(transform.rotation, GetRotationForFace(targetFace)) < 1f) {
					rb.isKinematic = true;
					transform.position = targetPos;
					transform.rotation = GetRotationForFace(targetFace);
					isSettled = true;
					yield break;
				}
			}
			
			elapsedTime += Time.deltaTime;
			yield return null;
		}

		// If we timeout, smoothly transition to final state
		StartCoroutine(SmoothFinalizePosition(targetFace));
	}

	private IEnumerator SmoothFinalizePosition(int targetFace) {
		float duration = 0.5f;
		float elapsed = 0f;
		
		Vector3 startPos = transform.position;
		Quaternion startRot = transform.rotation;
		
		Vector3 targetPos = startPos;
		targetPos.y = 0;
		Quaternion targetRot = GetRotationForFace(targetFace);
		
		while (elapsed < duration) {
			float t = elapsed / duration;
			transform.position = Vector3.Lerp(startPos, targetPos, t);
			transform.rotation = Quaternion.Lerp(startRot, targetRot, t);
			
			elapsed += Time.deltaTime;
			yield return null;
		}
		
		transform.position = targetPos;
		transform.rotation = targetRot;
		rb.isKinematic = true;
		isSettled = true;
	}

	private int GetCurrentFace() {
		// Get the euler angles of our current rotation
		Vector3 angles = transform.rotation.eulerAngles;
		
		// Normalize angles to handle any rotation amount
		angles.x = Mathf.Round(angles.x % 360);
		angles.y = Mathf.Round(angles.y % 360);
		angles.z = Mathf.Round(angles.z % 360);

		// Match the rotation patterns we defined in GetRotationForFace
		if (angles == Vector3.zero) return 6;                    // No rotation = 6
		if (angles == new Vector3(0, 0, 180)) return 1;         // 180° Z = 1
		if (angles == new Vector3(0, 0, 90)) return 2;          // 90° Z = 2
		if (angles == new Vector3(90, 0, 0)) return 3;          // 90° X = 3
		if (angles == new Vector3(270, 0, 0)) return 4;         // 270° X = 4
		if (angles == new Vector3(0, 0, -90)) return 5;         // -90° Z = 5

		return -1;  // Invalid rotation
	}

	private Quaternion GetRotationForFace(int face) {
		// Define rotations relative to the starting orientation (6 up, 3 forward)
		switch (face) {
			case 1: 
				return Quaternion.Euler(0, 0, 180);    // Rotate 180° around Z to get 1 up
			case 2: 
				return Quaternion.Euler(0, 0, 90);     // Rotate 90° around Z to get 2 up
			case 3: 
				return Quaternion.Euler(90, 0, 0);     // Rotate 90° around X to get 3 up
			case 4: 
				return Quaternion.Euler(270, 0, 0);    // Rotate 270° around X to get 4 up
			case 5: 
				return Quaternion.Euler(0, 0, -90);    // Rotate -90° around Z to get 5 up
			case 6: 
				return Quaternion.identity;            // No rotation needed for 6
			default: 
				return Quaternion.identity;
		}
	}

	public void Roll() {
		// Reset position and physics state to initial values
		transform.position = initialPosition;
		transform.rotation = initialRotation;
		
		// Make sure the rigidbody is not kinematic before setting velocities
		rb.isKinematic = false;
		
		rb.linearVelocity = Vector3.zero;
		rb.angularVelocity = Vector3.zero;

		// Add initial forces for visual effect
		rb.AddForce(transform.up * 100);
		rb.AddTorque(Random.Range(-250, 250), Random.Range(-50, 50), Random.Range(-500, 500));
	}

	public bool IsSettled() {
		return isSettled || 
			   (rb.isKinematic && 
			    rb.linearVelocity.magnitude < 0.01f && 
			    rb.angularVelocity.magnitude < 0.01f);
	}

	public void ForceComplete() {
		if (rb.isKinematic) return;
		
		rb.isKinematic = true;
		rb.linearVelocity = Vector3.zero;
		rb.angularVelocity = Vector3.zero;
		
		// Ensure we're at the correct height
		Vector3 pos = transform.position;
		pos.y = 0;
		transform.position = pos;
	}

}
