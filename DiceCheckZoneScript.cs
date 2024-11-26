using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DiceCheckZoneScript : MonoBehaviour {
	private Dictionary<int, int> diceNumbers = new Dictionary<int, int>();
	private Dictionary<int, bool> diceStopped = new Dictionary<int, bool>();
	private Dictionary<int, string> lastFaceCollisions = new Dictionary<int, string>();
	private float logInterval = 0.5f;
	private float nextLogTime = 0f;
	private const float VELOCITY_THRESHOLD = 0.05f;
	private const float ANGULAR_VELOCITY_THRESHOLD = 0.05f;

	void OnTriggerStay(Collider col) {
		if (col == null) return;

		DiceScript dice = col.GetComponentInParent<DiceScript>();
		if (dice == null) return;

		Rigidbody diceRb = dice.GetComponent<Rigidbody>();
		if (diceRb == null) return;

		// Track the last face collision for each die
		int faceNumber = GetFaceNumberFromCollider(col, dice.diceId);
		if (faceNumber != -1) {
			lastFaceCollisions[dice.diceId] = $"Die {dice.diceId} touching face {faceNumber}";
		}

		// Periodic logging of current face contacts
		if (Time.time >= nextLogTime) {
			LogCurrentFaceContacts();
			nextLogTime = Time.time + logInterval;
		}

		// Check if die has stopped
		if (!diceStopped.ContainsKey(dice.diceId)) {
			diceStopped[dice.diceId] = false;
		}

		if (!diceStopped[dice.diceId] && 
			diceRb.linearVelocity.sqrMagnitude < VELOCITY_THRESHOLD && 
			diceRb.angularVelocity.sqrMagnitude < ANGULAR_VELOCITY_THRESHOLD) {
			StartCoroutine(ConfirmDiceStopped(col, dice, diceRb));
		}
	}

	private void LogCurrentFaceContacts() {
		// Remove this periodic logging since it's not helpful
		// We'll only log when dice actually stop
	}

	private IEnumerator ConfirmDiceStopped(Collider col, DiceScript dice, Rigidbody diceRb) {
		yield return new WaitForSeconds(0.25f);
		
		if (diceRb.linearVelocity.sqrMagnitude < VELOCITY_THRESHOLD && 
			diceRb.angularVelocity.sqrMagnitude < ANGULAR_VELOCITY_THRESHOLD && 
			!diceStopped[dice.diceId]) {
			
			int faceNumber = GetFaceNumberFromCollider(col, dice.diceId);
			if (faceNumber == -1) {
				// Die hasn't landed properly - help it complete its rotation
				StartCoroutine(AdjustDiceRotation(dice, diceRb));
				yield break;
			}

			diceStopped[dice.diceId] = true;
			diceNumbers[dice.diceId] = faceNumber;
			Debug.Log($"[Dice Result] Die {dice.diceId}: {faceNumber}");
			GameManager.Instance.DieStopped(dice.diceId, faceNumber);
		}
	}

	private IEnumerator AdjustDiceRotation(DiceScript dice, Rigidbody diceRb) {
		// Find the closest face-down orientation
		Quaternion currentRotation = dice.transform.rotation;
		Vector3 up = currentRotation * Vector3.up;
		float angle = Vector3.Angle(up, Vector3.up);
		
		// Apply a small torque to help it rotate
		Vector3 torqueDirection = Vector3.Cross(up, Vector3.up);
		if (torqueDirection.magnitude > 0.01f) {
			diceRb.AddTorque(torqueDirection.normalized * 2f, ForceMode.Impulse);
			Debug.Log($"Adjusting die {dice.diceId} rotation - angle: {angle:F1}°");
		}
		
		// Wait a moment before allowing it to be checked again
		diceStopped[dice.diceId] = false;
		yield return new WaitForSeconds(0.5f);
	}

	private int GetFaceNumberFromCollider(Collider col, int diceId) {
		string colliderName = col.gameObject.name.ToLower();
		string dicePrefix = $"dice{diceId}side";
		
		if (colliderName.Contains(dicePrefix)) {
			string[] parts = colliderName.Split(new string[] { dicePrefix }, System.StringSplitOptions.RemoveEmptyEntries);
			if (parts.Length > 0 && int.TryParse(parts[parts.Length - 1], out int faceNumber)) {
				return faceNumber;
			}
		}
		return -1;
	}

	public int GetDiceNumber(int diceId) {
		return diceNumbers.ContainsKey(diceId) ? diceNumbers[diceId] : 0;
	}

	public void Reset() {
		Debug.Log("=== Rolling Dice ===");
		diceNumbers.Clear();
		diceStopped.Clear();
		lastFaceCollisions.Clear();
		nextLogTime = Time.time;
	}
}
