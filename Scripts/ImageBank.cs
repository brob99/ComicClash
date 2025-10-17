using System.Collections.Generic;
using UnityEngine;

public class ImageBank : MonoBehaviour
{
    public static ImageBank Instance { get; private set; }

    public class DrawingEntry
    {
        public byte[] imageData;
        public string playerId;

        // Lazy cache so repeated requests return the same Texture2D instance.
        // This also lets GetImageIndex work reliably via reference equality.
        public Texture2D cachedTexture;

        public DrawingEntry(byte[] data, string id)
        {
            imageData = data;
            playerId = id;
            cachedTexture = null;
        }
    }

    private readonly List<DrawingEntry> submittedDrawings = new List<DrawingEntry>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // -------------------- Original API (unchanged) --------------------

    public void AddImage(byte[] imageData, string playerId)
    {
        submittedDrawings.Add(new DrawingEntry(imageData, playerId));
    }

    public List<DrawingEntry> GetImages()
    {
        return submittedDrawings;
    }

    public Texture2D GetImage(int index)
    {
        if (index < 0 || index >= submittedDrawings.Count) return null;

        var entry = submittedDrawings[index];
        if (entry.cachedTexture != null) return entry.cachedTexture;

        // Lazy-create and cache the texture once.
        var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        tex.LoadImage(entry.imageData);
        entry.cachedTexture = tex;
        return tex;
    }

    public string GetPlayerId(int index)
    {
        if (index < 0 || index >= submittedDrawings.Count) return null;
        return submittedDrawings[index].playerId;
    }

    public int Count => submittedDrawings.Count;

    // -------------------- Compatibility Shims --------------------

    /// <summary>
    /// Newer scripts call GetTexture(int). This simply forwards to GetImage(int).
    /// </summary>
    public Texture2D GetTexture(int index) => GetImage(index);

    /// <summary>
    /// When newer code needs to recover an index from a Texture2D it previously got from the bank.
    /// Works because we cache and return the same Texture2D instance per entry.
    /// Returns -1 if the texture does not belong to this bank.
    /// </summary>
    public int GetImageIndex(Texture2D tex)
    {
        if (tex == null) return -1;
        for (int i = 0; i < submittedDrawings.Count; i++)
        {
            if (submittedDrawings[i].cachedTexture == tex)
                return i;
        }
        return -1;
    }

    /// <summary>
    /// Useful in tests or at a hard reset between games.
    /// </summary>
    public void ClearAll()
    {
        submittedDrawings.Clear();
    }
}
