using System.Collections.Generic;
using UnityEngine;

public static class MemeBank
{
    public class MemeEntry
    {
        public int drawingIndex;    // Index into ImageBank
        public string captionText;  // Chosen caption
        public string playerId;     // Who submitted it

        public MemeEntry(int drawingIndex, string captionText, string playerId)
        {
            this.drawingIndex = drawingIndex;
            this.captionText = captionText;
            this.playerId = playerId;
        }
    }

    private static List<MemeEntry> submittedMemes = new List<MemeEntry>();

    public static void AddMeme(int drawingIndex, string captionText, string playerId)
    {
        submittedMemes.Add(new MemeEntry(drawingIndex, captionText, playerId));
    }

    public static List<MemeEntry> GetAllMemes()
    {
        return submittedMemes;
    }

    public static int Count => submittedMemes.Count;

    public static void Clear()
    {
        submittedMemes.Clear();
    }
}
