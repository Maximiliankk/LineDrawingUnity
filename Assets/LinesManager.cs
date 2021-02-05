using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// manages all line drawing
public class LinesManager : MonoBehaviour
{
    public List<LineData> allRends = new List<LineData>();
    public List<LineGroup> groups = new List<LineGroup>();

    public class LineData
    {
        public LineData(LineRenderer r, float width)
        {
            rend = r;
            r.material.color = col;
            r.startWidth = width;
            r.endWidth = width;
        }
        public LineRenderer rend;
        public Color col = Color.green;
    }

    public class LineGroup
    {
        public int numLines;
        public int index;
        public bool visible = false;
        public Color col;

        public void Initialize(GameObject prefab, LinesManager lm, float width)
        {
            for (int i = 0; i < numLines; ++i)
            {
                var go2 = Instantiate(prefab);
                lm.AddLine(col, go2.GetComponent<LineRenderer>(), width);
            }
        }

        public void SetWidths(float width, LinesManager lm)
        {
            for (int j = 0; j < numLines; j++)
                lm.SetWidth(index + j, width);
        }

        public void Clear(LinesManager lm)
        {
            for (int j = 0; j < numLines; j++)
                lm.ClearLine(index + j);
        }
    }

    public void AddLine(Color c, LineRenderer rend, float width)
    {
        allRends.Add(new LineData(rend, width));
        var r = allRends[allRends.Count - 1];
        r.col = c;
        r.rend.positionCount = 0;
        r.rend.material.color = c;
    }

    public void AddLineGroup(Color c, int num, GameObject prefab, float width, bool vis = false)
    {
        LineGroup lg = new LineGroup();

        lg.numLines = num;
        lg.visible = vis;
        lg.col = c;
        lg.index = (groups.Count > 0) ?
            groups[groups.Count - 1].index + groups[groups.Count - 1].numLines : 0;
        lg.Initialize(prefab, this, width);
        groups.Add(lg);
    }

    public void AddPoint(int lineIndex)
    {
        allRends[lineIndex].rend.positionCount++;
    }

    public void SetPoint(int lineIndex, int pointIndex, Vector3 pos)
    {
        allRends[lineIndex].rend.SetPosition(pointIndex, pos);
    }

    public Vector3 GetPoint(int lineIndex, int pointIndex)
    {
        return allRends[lineIndex].rend.GetPosition(pointIndex);
    }

    public LineData GetRend(int lineIndex)
    {
        return allRends[lineIndex];
    }

    public void RemovePointFromLine(int lineIndex, int pointIndex)
    {
        if (pointIndex < allRends[lineIndex].rend.positionCount)
        {
            for (int i = pointIndex; i <= allRends[lineIndex].rend.positionCount - 2; i++)
            {
                // shift all other points to fill in the removed
                allRends[lineIndex].rend.SetPosition(i, allRends[lineIndex].rend.GetPosition(i + 1));
            }
        }
        allRends[lineIndex].rend.positionCount--;
    }

    public void SetWidth(int lineIndex, float width)
    {
        allRends[lineIndex].rend.startWidth = width;
        allRends[lineIndex].rend.endWidth = width;
    }

    public void ClearLine(int lineIndex)
    {
        allRends[lineIndex].rend.positionCount = 0;
    }

}
