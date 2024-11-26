using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class GameManager : MonoBehaviour {
    public static GameManager Instance;
    public DiceCheckZoneScript diceZone;
    private Dictionary<int, bool> diceStopped = new Dictionary<int, bool>();
    private bool roundComplete = false;

    void Awake() {
        if (Instance == null)
            Instance = this;
        else {
            Destroy(gameObject);
            return;
        }

        // Find zone if not assigned
        if (diceZone == null) {
            diceZone = FindFirstObjectByType<DiceCheckZoneScript>();
        }

        // Initialize all dice - REMOVE THIS SECTION
        // var dice = FindObjectsOfType<DiceScript>();
        // for (int i = 0; i < dice.Length; i++) {
        //     dice[i].diceId = i + 1;
        //     diceStopped[i + 1] = false;
        // }

        // NEW CODE: Use existing diceId values
        var dice = FindObjectsOfType<DiceScript>();
        foreach (var die in dice) {
            diceStopped[die.diceId] = false;
        }
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            RollDice();
        }
    }

    void RollDice() {
        roundComplete = false;
        diceZone.Reset();
        foreach (var key in diceStopped.Keys.ToList()) {
            diceStopped[key] = false;
        }

        var dice = FindObjectsOfType<DiceScript>();
        foreach (var die in dice) {
            die.Roll();
        }
    }

    public void DieStopped(int diceId, int number) {
        if (!diceStopped[diceId]) {
            diceStopped[diceId] = true;
            Debug.Log($"Die {diceId} stopped with number: {number}");
            
            // Check if all dice have stopped
            if (diceStopped.Values.All(stopped => stopped)) {
                LogFinalRoundState();
                roundComplete = true;
            }
        }
    }

    private void LogFinalRoundState() {
        Debug.Log("=== ROUND COMPLETE ===");
        foreach (var diceId in diceStopped.Keys.OrderBy(k => k)) {
            int number = diceZone.GetDiceNumber(diceId);
            Debug.Log($"Final Die {diceId} result: {number}");
        }
        Debug.Log("===================");
    }
} 