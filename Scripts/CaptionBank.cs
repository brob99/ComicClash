using System.Collections.Generic;
using UnityEngine;

public static class CaptionBank
{
    private static List<string> submittedCaptions = new List<string>();  // Stores captions as strings

    public static void AddCaption(string caption)
    {
        if (!submittedCaptions.Contains(caption))
        {
            submittedCaptions.Add(caption);
            Debug.Log($"Caption added: {caption}");
        }
        else
        {
            Debug.LogWarning($"Duplicate caption ignored: {caption}");
        }
    }


    public static List<string> GetCaptions()
    {
        return submittedCaptions;  // Return the list of captions
    }
}
