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
    private float scoreDelay = 1.0f;
    private bool scoreCalculated = false;

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
        scoreCalculated = false;
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
                StartCoroutine(CalculateScoreAfterDelay());
            }
        }
    }

    private IEnumerator CalculateScoreAfterDelay()
    {
        if (scoreCalculated) yield break;
        
        yield return new WaitForSeconds(scoreDelay);
        
        var diceValues = diceStopped.Keys.OrderBy(k => k)
            .Select(diceId => diceZone.GetDiceNumber(diceId))
            .ToArray();

        // Check for invalid dice values (0 means die didn't settle properly)
        if (diceValues.Any(v => v == 0))
        {
            Debug.Log("Some dice didn't settle properly - re-rolling stuck dice...");
            
            var dice = FindObjectsOfType<DiceScript>();
            foreach (var die in dice)
            {
                if (diceZone.GetDiceNumber(die.diceId) == 0)
                {
                    diceStopped[die.diceId] = false;
                    die.Roll();
                    Debug.Log($"Re-rolling die {die.diceId}");
                }
            }
            
            scoreCalculated = false;
            yield break;
        }

        int score = CeeloScorer.CalculateScore(diceValues);
        string description = CeeloScorer.GetScoreDescription(score);
        
        Debug.Log($"\n=== Cee-lo Score ===");
        Debug.Log($"Dice: {string.Join("-", diceValues)}");
        Debug.Log($"Result: {description}");
        Debug.Log($"Score Value: {score}");
        Debug.Log("==================\n");
        
        scoreCalculated = true;
    }

    private void LogFinalRoundState() {
        Debug.Log("\n=== Final Results ===");
        foreach (var diceId in diceStopped.Keys.OrderBy(k => k)) {
            int number = diceZone.GetDiceNumber(diceId);
            Debug.Log($"Die {diceId}: {number}");
        }
        Debug.Log("===================\n");
    }
} 