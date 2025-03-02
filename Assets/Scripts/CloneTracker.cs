using UnityEngine;

public static class CloneTracker
{
    private static bool hasCloned = false; // Tracks if a clone exists

    public static bool HasClone()
    {
        return hasCloned;
    }

    public static void SetCloned(bool state)
    {
        hasCloned = state;
    }
}
