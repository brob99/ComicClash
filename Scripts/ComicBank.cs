using System.Collections.Generic;

public static class ComicBank
{
    public struct Panel
    {
        public int    drawingIndex;
        public string captionText;

        public Panel(int drawingIndex, string captionText)
        {
            this.drawingIndex = drawingIndex;
            this.captionText  = captionText;
        }
    }

    public class ComicEntry
    {
        public Panel[] panels;   // always length 4
        public string  playerId;

        public ComicEntry(Panel[] panels, string playerId)
        {
            this.panels   = panels;
            this.playerId = playerId;
        }
    }

    private static readonly List<ComicEntry> comics = new();

    public static void AddComic(Panel[] panels, string playerId) =>
        comics.Add(new ComicEntry(panels, playerId));

    public static IReadOnlyList<ComicEntry> GetAllComics() => comics;

    // ✅ Added: lets other scripts do ComicBank.Count
    public static int Count => comics.Count;

    public static void Clear() => comics.Clear();
}
