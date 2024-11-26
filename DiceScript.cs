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
		Debug.Log($"Die {diceId} collided with {collision.gameObject.name} " +
				 $"at velocity {rb.linearVelocity.magnitude:F2} " +
				 $"(Position: {transform.position})");
	}

	public void Roll() {
		Debug.Log($"Die {diceId} starting roll...");
		float dirX = Random.Range(0, 250);
		float dirY = Random.Range(0, 50);
		float dirZ = Random.Range(0, 500);
		
		// Simplified position setting for single die
		transform.position = new Vector3(0, 2, 0);  // Center position
		transform.rotation = Quaternion.identity;
		rb.linearVelocity = Vector3.zero;
		rb.angularVelocity = Vector3.zero;
		rb.AddForce(transform.up * 500);
		rb.AddTorque(dirX, dirY, dirZ);

		Debug.Log($"Die {diceId} rolled with forces: " +
				 $"Torque({dirX:F0}, {dirY:F0}, {dirZ:F0}), " +
				 $"UpForce(500)");
	}
}
