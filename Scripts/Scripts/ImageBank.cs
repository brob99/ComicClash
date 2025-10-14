using System.Collections.Generic;
using UnityEngine;

public class ImageBank : MonoBehaviour
{
    public static ImageBank Instance { get; private set; }

    public class DrawingEntry
    {
        public byte[] imageData;
        public string playerId;

        public DrawingEntry(byte[] data, string id)
        {
            imageData = data;
            playerId = id;
        }
    }

    private List<DrawingEntry> submittedDrawings = new List<DrawingEntry>();

    void Awake()
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

        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(submittedDrawings[index].imageData);
        return texture;
    }

    public string GetPlayerId(int index)
    {
        if (index < 0 || index >= submittedDrawings.Count) return null;
        return submittedDrawings[index].playerId;
    }

    public int Count => submittedDrawings.Count;
}
