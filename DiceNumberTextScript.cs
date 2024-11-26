using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class DiceNumberTextScript : MonoBehaviour {

	public int diceId;
	Text text;
	private int previousDiceNumber = -1;
	
	// Use this for initialization
	void Start () {
		text = GetComponent<Text> ();
	}
	
	// Update is called once per frame
	void Update () {
		int currentNumber = GameManager.Instance.diceZone.GetDiceNumber(diceId);
		if (currentNumber != previousDiceNumber) {
			previousDiceNumber = currentNumber;
			text.text = $"Dice {diceId}: {currentNumber}";
		}
	}
}
