using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionManager : MonoBehaviour
{
    public GameObject selectionCirclePrototype;

    public LineRenderer lineRenderer;
    public Transform lineBeginningCircle;
    public Transform lineEndingCircle;

    Vector3 offset = new Vector3(0.0f, 0.0f, -1.1f);

    List<Block> selectedBlocks = new List<Block>();

    void Start()
    {
        lineRenderer.startWidth = lineRenderer.endWidth = (lineRenderer.startWidth * 2.0f);
        lineRenderer.gameObject.SetActive(false);

        lineBeginningCircle.gameObject.SetActive(false);
        lineEndingCircle.gameObject.SetActive(false);
    }

    public int NumSelectedBlocks { get { return selectedBlocks.Count; } }

    public void CopySelectedBlocks(ref List<Block> refSelectedBlocks)
    {
        refSelectedBlocks.Clear();

        for (int i = 0; i < selectedBlocks.Count; i++)
        {
            refSelectedBlocks.Add(selectedBlocks[i]);
        }
    }

    public bool Contains(Block block)
    {
        return selectedBlocks.Contains(block);
    }
    
    public void AddUnique(Block block)
    {
        if (!selectedBlocks.Contains(block))
        {
            selectedBlocks.Add(block);
        }
    }

    public void RemoveFollowingBlocks(Block startWith)
    {
        int index = GetListIndex(startWith) + 1;

        if (index >= 0 && index < selectedBlocks.Count)
        {
            for (int i = selectedBlocks.Count - 1; i >= index; i--)
            {
                selectedBlocks[i] = null;
                selectedBlocks.RemoveAt(i);
            }
        }
    }

    public int GetListIndex(Block block)
    {
        return selectedBlocks.FindIndex(eachBlock => eachBlock == block);
    }

    public void UpdateSelectionLine()
    {
        if (selectedBlocks.Count < 2)
        {
            if (selectedBlocks.Count < 1)
            {
                lineRenderer.gameObject.SetActive(false);
                lineBeginningCircle.gameObject.SetActive(false);
                lineEndingCircle.gameObject.SetActive(false);
            }
            else
            {
                lineRenderer.gameObject.SetActive(false);
                lineEndingCircle.gameObject.SetActive(false);

                lineBeginningCircle.gameObject.SetActive(true);
                lineBeginningCircle.position = selectedBlocks[0].Position + offset;
            }
            return;
        }

        lineRenderer.gameObject.SetActive(true);
        lineBeginningCircle.gameObject.SetActive(true);
        lineEndingCircle.gameObject.SetActive(true);

        lineBeginningCircle.position = selectedBlocks[0].Position + offset;
        lineEndingCircle.position = selectedBlocks[selectedBlocks.Count - 1].Position + offset;

        lineRenderer.positionCount = selectedBlocks.Count;

        for (int i = 0; i < selectedBlocks.Count; i++)
        {
            lineRenderer.SetPosition(i, selectedBlocks[i].transform.position);
        }
    }

    public Block GetLastSelectedBlock()
    {
        return selectedBlocks == null || selectedBlocks.Count == 0 ? null : selectedBlocks[selectedBlocks.Count - 1];
    }

    public void Clear()
    {
        selectedBlocks.Clear();
    }
}
