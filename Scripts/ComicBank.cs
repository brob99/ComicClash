using System.Collections.Generic;

public static class ComicBank
{
    // ----- Public data types -----

    // Panels used by ComicCreateManager (drawing + caption)
    public struct Panel
    {
        public int drawingIndex;
        public string captionText;

        public Panel(int drawingIndex, string captionText)
        {
            this.drawingIndex = drawingIndex;
            this.captionText = captionText;
        }
    }

    // Stored entry for a finished comic (4 panels max)
    public class ComicEntry
    {
        public string id;                 // optional id if you want one
        public string playerId;           // canonical author id

        // --- Compatibility shims (so existing code compiles) ---
        public string authorId            // some scripts expect this, alias playerId
        {
            get => playerId;
            set => playerId = value;
        }

        public int[] drawings = new int[4];

        // some scripts expect 'drawingIndices'
        public int[] drawingIndices
        {
            get => drawings;
            set => drawings = value;
        }

        public string[] captions = new string[4];

        // some scripts may expect 'captionTexts'
        public string[] captionTexts
        {
            get => captions;
            set => captions = value;
        }
    }

    // ----- Storage -----
    private static readonly List<ComicEntry> _comics = new List<ComicEntry>();

    // ----- API -----
    public static void Add(ComicEntry entry) => _comics.Add(entry);

    // Compatibility with older calls
    public static void AddComic(ComicEntry entry) => Add(entry);

    // Convenience for Panel[] calls
    public static void AddComic(string playerId, Panel[] panels)
    {
        Add(FromPanels(playerId, panels));
    }

    public static IReadOnlyList<ComicEntry> GetAllComics() => _comics;

    public static int Count => _comics.Count;

    // If you prefer a method call somewhere else:
    public static int GetCount() => _comics.Count;

    public static void Clear() => _comics.Clear();

    // Helper to convert Panel[] → ComicEntry
    public static ComicEntry FromPanels(string playerId, Panel[] panels)
    {
        var e = new ComicEntry
        {
            playerId = playerId,
            drawings = new int[4],
            captions = new string[4]
        };

        for (int i = 0; i < 4 && i < panels.Length; i++)
        {
            e.drawings[i] = panels[i].drawingIndex;
            e.captions[i] = panels[i].captionText ?? string.Empty;
        }

        return e;
    }
}
