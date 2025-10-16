using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DrawingSelector : MonoBehaviour
{
    public List<RawImage> drawingSlots;   // assign in Inspector
    public List<Button>   drawingButtons; // assign in Inspector

    public event Action<int> OnDrawingSelected;  // ← NEW

    private int        selectedLocalSlot = -1;
    private List<int>  bankIndices       = new();

    void Start()
    {
        LoadDrawings();
    }

    void LoadDrawings()
    {
        List<ImageBank.DrawingEntry> all = ImageBank.Instance.GetImages();

        // allow own drawings for solo test
        List<int> valid = new();
        for (int i = 0; i < all.Count; i++) valid.Add(i);

        valid = Shuffle(valid);
        int show = Mathf.Min(3, valid.Count);

        bankIndices.Clear();

        for (int i = 0; i < drawingSlots.Count; i++)
        {
            if (i < show)
            {
                int bankIdx = valid[i];
                bankIndices.Add(bankIdx);

                drawingSlots[i].texture     = ImageBank.Instance.GetImage(bankIdx);
                drawingButtons[i].interactable = true;

                int local = i; // capture
                drawingButtons[i].onClick.AddListener(() => Select(local));
            }
            else
            {
                drawingSlots[i].texture     = null;
                drawingButtons[i].interactable = false;
            }
        }
        UpdateVisuals();
    }

    void Select(int localSlot)
    {
        selectedLocalSlot                = localSlot;
        MemeCreateState.SelectedDrawing  = bankIndices[localSlot];
        UpdateVisuals();
        OnDrawingSelected?.Invoke(MemeCreateState.SelectedDrawing);   // ← fire event
    }

    void UpdateVisuals()
    {
        for (int i = 0; i < drawingButtons.Count; i++)
        {
            Outline o = drawingButtons[i].GetComponent<Outline>() ??
                        drawingButtons[i].gameObject.AddComponent<Outline>();

            o.effectColor    = Color.blue;
            o.effectDistance = new Vector2(5,5);
            o.enabled        = (i == selectedLocalSlot);
        }
    }

    List<int> Shuffle(List<int> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int tmp = list[i];
            int r   = UnityEngine.Random.Range(i, list.Count);
            list[i] = list[r];
            list[r] = tmp;
        }
        return list;
    }
}
