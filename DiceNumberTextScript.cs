using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class DiceNumberTextScript : MonoBehaviour {

	Text text;
	private int previousSum = -1;
	
	// Use this for initialization
	void Start () {
		text = GetComponent<Text> ();
	}
	
	// Update is called once per frame
	void Update () {
		// Get all dice numbers and sum them
		int currentSum = 0;
		var diceZone = GameManager.Instance.diceZone;
		var allDice = FindObjectsOfType<DiceScript>();
		
		foreach (var die in allDice) {
			currentSum += diceZone.GetDiceNumber(die.diceId);
		}

		if (currentSum != previousSum) {
			previousSum = currentSum;
			text.text = $"{currentSum}";
		}
	}
}
