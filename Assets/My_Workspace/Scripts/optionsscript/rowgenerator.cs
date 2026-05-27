using UnityEngine;

public class GlassRowManager : MonoBehaviour
{
    public GlassPlate[] platesInRow; // assign 4 plates in inspector

    void Start()
    {
        GenerateRow();
    }

    void GenerateRow()
    {
        // reset all
        foreach (GlassPlate p in platesInRow)
        {
            p.isSafe = false;
        }

        // pick 2 random safe plates
        int safeCount = 0;

        while (safeCount < 2)
        {
            int index = Random.Range(0, platesInRow.Length);

            if (!platesInRow[index].isSafe)
            {
                platesInRow[index].isSafe = true;
                safeCount++;
            }
        }
    }
}