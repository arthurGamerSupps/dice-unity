using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class GameManager : MonoBehaviour {
    public DiceScript[] dice;
    private bool isRolling = false;
    private float rollTimeout = 5f; // Maximum time for a roll sequence
    
    void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            Debug.Log("Space pressed");
            if (!isRolling) {
                RollDice();
            }
        }
    }

    void RollDice() {
        Debug.Log("Starting dice roll");
        
        // Check if dice array is properly initialized
        if (dice == null || dice.Length == 0) {
            Debug.LogError("No dice assigned to GameManager!");
            return;
        }
        
        // Create and populate target values
        int[] targetValues = new int[dice.Length];
        Debug.Log($"Rolling {dice.Length} dice");
        
        for (int i = 0; i < dice.Length; i++) {
            targetValues[i] = Random.Range(1, 7);
            Debug.Log($"Die {i} target: {targetValues[i]}");
        }
        
        RollDiceWithResults(targetValues);
    }

    public void RollDiceWithResults(int[] targetValues) {
        if (isRolling || targetValues.Length != dice.Length) return;
        
        Debug.Log("=== Starting New Roll ===");
        Debug.Log($"Target Values: {string.Join(", ", targetValues)}");
        
        isRolling = true;
        StartCoroutine(ManageRollSequence(targetValues));
    }

    private IEnumerator ManageRollSequence(int[] targetValues) {
        // Start all dice rolling simultaneously
        for (int i = 0; i < dice.Length; i++) {
            dice[i].RollToTarget(targetValues[i]);
        }

        float elapsedTime = 0f;
        bool allSettled = false;

        // Wait for all dice to settle or timeout
        while (!allSettled && elapsedTime < rollTimeout) {
            allSettled = true;
            foreach (DiceScript die in dice) {
                if (!die.IsSettled()) {
                    allSettled = false;
                    break;
                }
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Force completion if timed out
        if (!allSettled) {
            foreach (DiceScript die in dice) {
                die.ForceComplete();
            }
        }

        isRolling = false;
    }

    // Helper method to check if any dice are currently rolling
    public bool IsRolling() {
        return isRolling;
    }
} 