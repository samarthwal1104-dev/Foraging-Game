using UnityEngine;
using System.Collections.Generic;

public class Apple : MonoBehaviour
{
    public int pointValue = 10;
    public string fruitType = "Red";

    // A static list that holds all active apples in the scene
    public static List<Apple> ActiveApples = new List<Apple>();

    // Call this from GameManager to prevent list errors between trials
    public static void ClearActiveList()
    {
        ActiveApples.Clear();
    }

    private void OnEnable()
    {
        // Add this apple to the list when it is spawned/enabled
        if (!ActiveApples.Contains(this))
        {
            ActiveApples.Add(this);
        }
    }

    private void OnDisable()
    {
        // Remove this apple from the list when it is collected/disabled
        ActiveApples.Remove(this);
    }
}

