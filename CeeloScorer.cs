using UnityEngine;
using System.Linq;

public class CeeloScorer : MonoBehaviour
{
    public static int CalculateScore(int[] diceValues)
    {
        if (diceValues.Length != 3) return 0;

        // Sort the values for easier pattern matching
        var sorted = diceValues.OrderBy(x => x).ToArray();

        // Check for 4-5-6 (Automatic win)
        if (sorted[0] == 4 && sorted[1] == 5 && sorted[2] == 6)
            return 1000;

        // Check for 1-2-3 (Automatic loss)
        if (sorted[0] == 1 && sorted[1] == 2 && sorted[2] == 3)
            return -1000;

        // Check for triples
        if (sorted[0] == sorted[1] && sorted[1] == sorted[2])
            return sorted[0] * 100;

        // Check for pairs
        if (sorted[0] == sorted[1])
            return sorted[2];
        if (sorted[1] == sorted[2])
            return sorted[0];

        return 0; // No scoring combination
    }

    public static string GetScoreDescription(int score)
    {
        if (score == 1000) return "4-5-6 (Automatic Win!)";
        if (score == -1000) return "1-2-3 (Automatic Loss!)";
        if (score > 99) return $"Triple {score/100}s!";
        if (score > 0) return $"Point: {score}";
        return "No Score";
    }
} 