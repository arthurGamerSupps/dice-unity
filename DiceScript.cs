using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceScript : MonoBehaviour {

	private Rigidbody rb;
	public Vector3 diceVelocity;
	[SerializeField]
	public int diceId;

	// Use this for initialization
	void Start () {
		rb = GetComponent<Rigidbody> ();
	}
	
	// Update is called once per frame
	void Update () {
		diceVelocity = rb.linearVelocity;
	}

	void OnCollisionEnter(Collision collision) {
	}

	public void RollToTarget(int targetFace) {
		// Reset position and physics state
		transform.position = new Vector3(0, 2, 0);
		transform.rotation = Quaternion.identity;
		rb.velocity = Vector3.zero;
		rb.angularVelocity = Vector3.zero;

		// Add initial forces for visual effect
		rb.AddForce(transform.up * 100);
		rb.AddTorque(Random.Range(-250, 250), Random.Range(-50, 50), Random.Range(-500, 500));

		// Start coroutine to ensure we land on target
		StartCoroutine(EnsureTargetFace(targetFace));
	}

	private IEnumerator EnsureTargetFace(int targetFace) {
		// Wait for initial physics to settle
		yield return new WaitForSeconds(1.0f);
		
		// Get target rotation for desired face
		Quaternion targetRotation = GetRotationForFace(targetFace);
		
		// Smoothly rotate to target
		float duration = 0.5f;
		float elapsed = 0;
		Quaternion startRotation = transform.rotation;
		
		while (elapsed < duration) {
			transform.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsed / duration);
			elapsed += Time.deltaTime;
			yield return null;
		}
		
		transform.rotation = targetRotation;
		rb.isKinematic = true;  // Lock the die in place
	}

	private Quaternion GetRotationForFace(int face) {
		// Define rotations that show each face upward
		switch (face) {
			case 1: return Quaternion.Euler(180, 0, 0);  // 1 up
			case 2: return Quaternion.Euler(90, 0, 0);   // 2 up
			case 3: return Quaternion.Euler(0, 0, 90);   // 3 up
			case 4: return Quaternion.Euler(0, 0, -90);  // 4 up
			case 5: return Quaternion.Euler(-90, 0, 0);  // 5 up
			case 6: return Quaternion.Euler(0, 0, 0);    // 6 up
			default: return Quaternion.identity;
		}
	}
}
