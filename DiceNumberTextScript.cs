using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class DiceNumberTextScript : MonoBehaviour {

	Text text;
	private int previousScore = -1;
	
	// Use this for initialization
	void Start () {
		text = GetComponent<Text> ();
	}
	
	// Update is called once per frame
	void Update () {
		var diceZone = GameManager.Instance.diceZone;
		var allDice = FindObjectsOfType<DiceScript>();
		
		// Get all dice values in order
		var diceValues = allDice
			.Select(die => diceZone.GetDiceNumber(die.diceId))
			.ToArray();
		
		// Only calculate score if all dice have valid values
		if (!diceValues.Contains(0)) {
			int currentScore = CeeloScorer.CalculateScore(diceValues);
			
			if (currentScore != previousScore) {
				previousScore = currentScore;
				text.text = currentScore.ToString();
			}
		} else {
			text.text = "...";
		}
	}
}
