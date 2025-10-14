using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ComicCardUI : MonoBehaviour
{
    [Header("Panel Images (2x2 order)")]
    public RawImage[] panelImages = new RawImage[4];

    [Header("Panel Captions (2x2 order)")]
    public TMP_Text[] panelCaptions = new TMP_Text[4];

    public Button voteButton;

    /// <summary>
    /// Bind the entry data into the 2x2 card. Supports several ComicEntry shapes.
    /// </summary>
    public void Bind(object entry, Action onVote)
    {
        if (entry == null)
        {
            Debug.LogWarning("[ComicCardUI] Bind called with null entry.");
            return;
        }

        int[] drawings = ExtractDrawingIndices(entry);
        string[] captions = ExtractCaptions(entry);

        for (int i = 0; i < 4; i++)
        {
            bool hasPanel = (i < drawings.Length) || (i < captions.Length);

            // Image (square)
            if (panelImages != null && i < panelImages.Length)
            {
                if (hasPanel && i < drawings.Length && ImageBank.Instance != null)
                {
                    int idx = drawings[i];
                    if (idx >= 0)
                        panelImages[i].texture = ImageBank.Instance.GetImage(idx);
                    else
                        panelImages[i].texture = null;
                }
                panelImages[i].gameObject.SetActive(hasPanel);
            }

            // Caption
            if (panelCaptions != null && i < panelCaptions.Length)
            {
                string text = (i < captions.Length) ? (captions[i] ?? "") : "";
                panelCaptions[i].text = text;
                panelCaptions[i].gameObject.SetActive(hasPanel);
            }
        }

        if (voteButton != null)
        {
            voteButton.onClick.RemoveAllListeners();
            voteButton.onClick.AddListener(() => onVote?.Invoke());
        }
    }

    // ------------------------
    // Reflection helpers
    // ------------------------

    // Try to fetch int[] drawing indices from several common shapes.
    private int[] ExtractDrawingIndices(object entry)
    {
        // 1) int[] drawingIndices
        if (TryGetIntArray(entry, "drawingIndices", out var arr)) return arr;

        // 2) int[] panelDrawingIndices
        if (TryGetIntArray(entry, "panelDrawingIndices", out arr)) return arr;

        // 3) panels[] -> each has drawingIndex
        if (TryGetFromPanelArray(entry, "panels", "drawingIndex", out arr)) return arr;

        // 4) drawings[] or panelDrawings[] (some people use these names)
        if (TryGetIntArray(entry, "drawings", out arr)) return arr;
        if (TryGetIntArray(entry, "panelDrawings", out arr)) return arr;

        return Array.Empty<int>();
    }

    // Try to fetch string[] captions from several common shapes.
    private string[] ExtractCaptions(object entry)
    {
        // 1) string[] captions
        if (TryGetStringArray(entry, "captions", out var arr)) return arr;

        // 2) string[] panelCaptions
        if (TryGetStringArray(entry, "panelCaptions", out arr)) return arr;

        // 3) panels[] -> each has caption
        if (TryGetFromPanelArray(entry, "panels", "caption", out arr)) return arr;

        return Array.Empty<string>();
    }

    // Generic: read int[] field or property by name
    private bool TryGetIntArray(object obj, string memberName, out int[] result)
    {
        result = Array.Empty<int>();
        if (obj == null) return false;

        var t = obj.GetType();

        // Field?
        var f = t.GetField(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (f != null && f.FieldType == typeof(int[]))
        {
            result = (int[])f.GetValue(obj);
            return result != null;
        }

        // Property?
        var p = t.GetProperty(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (p != null && p.PropertyType == typeof(int[]))
        {
            result = (int[])p.GetValue(obj, null);
            return result != null;
        }

        return false;
    }

    // Generic: read string[] field or property by name
    private bool TryGetStringArray(object obj, string memberName, out string[] result)
    {
        result = Array.Empty<string>();
        if (obj == null) return false;

        var t = obj.GetType();

        // Field?
        var f = t.GetField(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (f != null && f.FieldType == typeof(string[]))
        {
            result = (string[])f.GetValue(obj);
            return result != null;
        }

        // Property?
        var p = t.GetProperty(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (p != null && p.PropertyType == typeof(string[]))
        {
            result = (string[])p.GetValue(obj, null);
            return result != null;
        }

        return false;
    }

    // Handle shapes like: panels[] where each panel has either an int "drawingIndex" or string "caption"
    private bool TryGetFromPanelArray<T>(object obj, string panelArrayName, string childMember, out T[] result)
    {
        result = Array.Empty<T>();
        if (obj == null) return false;

        var t = obj.GetType();
        var f = t.GetField(panelArrayName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        var p = t.GetProperty(panelArrayName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        object raw = null;
        Type elemType = null;

        if (f != null)
        {
            raw = f.GetValue(obj);
            elemType = f.FieldType.IsArray ? f.FieldType.GetElementType() : f.FieldType;
        }
        else if (p != null)
        {
            raw = p.GetValue(obj, null);
            elemType = p.PropertyType.IsArray ? p.PropertyType.GetElementType() : p.PropertyType;
        }

        if (raw == null) return false;

        var list = new List<T>();

        if (raw is IEnumerable enumerable)
        {
            foreach (var item in enumerable)
            {
                if (item == null) { list.Add(default); continue; }

                var it = item.GetType();

                // child field or property
                var cf = it.GetField(childMember, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                var cp = it.GetProperty(childMember, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                object value = null;
                if (cf != null) value = cf.GetValue(item);
                else if (cp != null) value = cp.GetValue(item, null);

                if (value is T casted)
                    list.Add(casted);
                else
                    list.Add(default);
            }

            result = list.ToArray();
            return true;
        }

        return false;
    }
}
