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

		if (!diceStopped[dice.diceId] && diceRb.linearVelocity.sqrMagnitude < 0.001f) {
			StartCoroutine(ConfirmDiceStopped(col, dice, diceRb));
		}
	}

	private void LogCurrentFaceContacts() {
		Debug.Log("=== Current Face Contacts ===");
		foreach (var contact in lastFaceCollisions.OrderBy(k => k.Key)) {
			Debug.Log(contact.Value);
		}
	}

	private IEnumerator ConfirmDiceStopped(Collider col, DiceScript dice, Rigidbody diceRb) {
		yield return new WaitForSeconds(0.5f);
		
		if (diceRb.linearVelocity.sqrMagnitude < 0.001f && !diceStopped[dice.diceId]) {
			diceStopped[dice.diceId] = true;
			
			int faceNumber = GetFaceNumberFromCollider(col, dice.diceId);
			if (faceNumber != -1) {
				diceNumbers[dice.diceId] = faceNumber;
				Debug.Log($"Die {dice.diceId} STOPPED on face {faceNumber}");
				GameManager.Instance.DieStopped(dice.diceId, faceNumber);
			}
		}
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
		Debug.Log("=== NEW ROUND STARTING ===");
		diceNumbers.Clear();
		diceStopped.Clear();
		lastFaceCollisions.Clear();
		nextLogTime = Time.time;
	}
}
