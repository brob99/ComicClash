using System;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ComicCardUI : MonoBehaviour
{
    [Header("Panel images (2x2 order: 0,1,2,3)")]
    [SerializeField] private RawImage[] drawingCells = new RawImage[4];

    [Header("Captions under each panel (0..3)")]
    [SerializeField] private TMP_Text[] captionLabels = new TMP_Text[4];

    [Header("Vote button on the card")]
    [SerializeField] private Button voteButton;

    // Call from ComicVoteManager after you instantiate the prefab
    public void Bind(ComicBank.ComicEntry entry, Action onVote)
    {
        if (entry == null)
        {
            Debug.LogWarning("[ComicCardUI] Bind called with null entry");
            return;
        }

        // Defensive: make sure arrays are sized
        if (drawingCells == null || drawingCells.Length < 4 || captionLabels == null || captionLabels.Length < 4)
        {
            Debug.LogError("[ComicCardUI] drawingCells/captionLabels not wired (need 4 each).");
            return;
        }

        // Each entry has 4 panels. We expect ComicBank.Panel { int drawingIndex; string captionText; }
        for (int i = 0; i < 4; i++)
        {
            var panel = entry.panels != null && entry.panels.Length > i ? entry.panels[i] : default;
            var img   = drawingCells[i];
            var text  = captionLabels[i];

            // Caption
            if (text != null)
                text.text = panel.captionText ?? string.Empty;

            // Image
            if (img != null)
            {
                img.texture = ResolveTexture(panel.drawingIndex);
                img.color   = img.texture ? Color.white : new Color(1, 1, 1, 0.2f);
                img.uvRect  = new Rect(0, 0, 1, 1);
                img.SetMaterialDirty();
                img.maskable = true;
                img.raycastTarget = false;
#if UNITY_2021_3_OR_NEWER
                img.texture?.Apply(false, false);
#endif
            }
        }

        // Hook vote
        if (voteButton != null)
        {
            voteButton.onClick.RemoveAllListeners();
            if (onVote != null) voteButton.onClick.AddListener(() => onVote());
        }
    }

    // Tries to get a Texture2D from ImageBank via reflection so we don't depend on exact API names.
    private static Texture2D ResolveTexture(int index)
    {
        if (index < 0) return null;
        var bankType = FindType("ImageBank");
        if (bankType == null) return null;

        var instanceProp = bankType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
        var bank = instanceProp != null ? instanceProp.GetValue(null) : null;
        if (bank == null) return null;

        // Try common signatures in order: GetTexture(int), GetImage(int), GetSprite(int)
        Texture2D tex = Invoke<Texture2D>(bank, "GetTexture", index);
        if (tex != null) return tex;

        tex = Invoke<Texture2D>(bank, "GetImage", index);
        if (tex != null) return tex;

        // If the bank returns a Sprite, use its texture
        var sprite = Invoke<Sprite>(bank, "GetSprite", index);
        if (sprite != null) return sprite.texture;

        // Some projects use GetByIndex
        tex = Invoke<Texture2D>(bank, "GetByIndex", index);
        if (tex != null) return tex;

        return null;
    }

    private static T Invoke<T>(object instance, string method, int arg) where T : class
    {
        var m = instance.GetType().GetMethod(method, BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(int) }, null);
        if (m == null) return null;
        try { return m.Invoke(instance, new object[] { arg }) as T; }
        catch { return null; }
    }

    private static Type FindType(string name)
    {
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            var t = asm.GetType(name);
            if (t != null) return t;
        }
        return null;
    }
}
