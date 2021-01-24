using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using System;

public class ProjectManager : MonoBehaviour
{
    static float lineWidth = 0.5f;
    readonly float pointWidth = 0.5f;
    readonly int example1numlines = 20;
    readonly int example2numlines = 1;

    // manages all line strips
    public class LinesManager
    {
        // data for one line strip
        public class LineData
        {
            public LineData(LineRenderer r, GameObject o, float width)
            {
                rend = r;
                obj = o;
                r.material.color = col;
                r.startWidth = width;
                r.endWidth = width;
            }
            public LineRenderer rend;
            public GameObject obj;
            public Color col = Color.green;
        }

        // main data of LinesManager
        List<LineData> allRends = new List<LineData>();

        // public interface for LinesManager
        public void AddLine(Color c, LineRenderer rend, GameObject go, float width)
        {
            allRends.Add(new LineData(rend, go, width));
            var r = allRends[allRends.Count - 1];
            r.col = c;
            r.rend.positionCount = 0;
            r.rend.material.color = c;
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
        public void RemovePointFromLine(int lineIndex, int pointIndex)
        {
            if(pointIndex < allRends[lineIndex].rend.positionCount)
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

        public class LineGroup
        {
            public int numLines;
            public int index;
            public bool visible = false;
            public Color col;

            public void Initialize(GameObject prefab)
            {
                for (int i = 0; i < numLines; ++i)
                {
                    var go2 = Instantiate(prefab);
                    m_linesManager.AddLine(col, go2.GetComponent<LineRenderer>(), go2, lineWidth);
                }
            }

            public void SetWidths(float width)
            {
                for (int j = 0; j < numLines; j++)
                    m_linesManager.SetWidth(index + j, width);
            }

            public void Clear()
            {
                for (int j = 0; j < numLines; j++)
                    m_linesManager.ClearLine(index + j);
            }
        }
        public List<LineGroup> groups = new List<LineGroup>();

        public void AddLineGroup(Color c, int num, GameObject prefab, bool vis = false)
        {
            LineGroup lg = new LineGroup();
            
            lg.numLines = num;
            lg.visible = vis;
            lg.col = c;
            lg.index = (groups.Count > 0) ?
                groups[groups.Count-1].index + groups[groups.Count - 1].numLines : 0;
            lg.Initialize(prefab);
            groups.Add(lg);
        }
    }
    static LinesManager m_linesManager = new LinesManager();


    // current number of active control points
    int pointsClicked = 0;

    // use this as a template to make more lineRenderers
    [SerializeField] GameObject lineRendPrefab;

    // public allows you to drag an object reference here in the editor
    [SerializeField] UnityEngine.UI.Slider slider;

    // flag for dragging points
    bool dragging = false;
    GameObject draggedObj = null;

    // list container for references to all the sphere gameObjects
    List<GameObject> draggablePoints = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        m_linesManager.AddLineGroup(Color.blue, 1, lineRendPrefab);
        m_linesManager.AddLineGroup(Color.yellow, example1numlines, lineRendPrefab, true);
        m_linesManager.AddLineGroup(Color.red, example2numlines, lineRendPrefab);

        slider.value = lineWidth;
    }

    void ClickPoint(Vector3 point)
    {
        pointsClicked++;
        m_linesManager.AddPoint(0); // allocate memory for new point

        // create the sphere object to drag points around with
        var sphereObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphereObj.transform.position = point;
        sphereObj.transform.localScale = new Vector3(1, 1, 1) * pointWidth;
        draggablePoints.Add(sphereObj);

        // update the lineRenderer vertex position
        m_linesManager.SetPoint(0, pointsClicked - 1, point);
        UpdateCurve();
    }

    // Update is called once per frame
    void Update()
    {
        // 0 is Left mouse button, 1 is Right mouse button
        if(Input.GetMouseButtonDown(1))
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit rh))
            {
                // find the index
                int index = 0;
                for (int i = 0; i < draggablePoints.Count; ++i)
                {
                    if (draggablePoints[i] == rh.collider.gameObject)
                    {
                        index = i;
                        break;
                    }
                }
                draggablePoints.RemoveAt(index);
                m_linesManager.RemovePointFromLine(0, index);
                Destroy(rh.collider.gameObject);
                pointsClicked--;
                UpdateCurve();
            }
        }
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                Debug.Log("left-clicked on UI");
            }
            else
            {
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit rh))
                {
                    // clicked on a point
                    dragging = true;
                    draggedObj = rh.collider.gameObject;
                }
                else
                {
                    // convert mouse coords to a world position
                    var point = Camera.main.ScreenToWorldPoint(
                    new Vector3(Input.mousePosition.x, Input.mousePosition.y, Mathf.Abs(Camera.main.transform.position.z)));

                    ClickPoint(point);
                }
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            dragging = false;
            draggedObj = null;
        }

        if(dragging)
        {
            var point = Camera.main.ScreenToWorldPoint(
            new Vector3(Input.mousePosition.x, Input.mousePosition.y, Mathf.Abs(Camera.main.transform.position.z)));
            draggedObj.transform.position = point;

            int index = 0;
            for (int i = 0; i < draggablePoints.Count; ++i)
            {
                if (draggablePoints[i] == draggedObj)
                {
                    index = i;
                    break;
                }
            }

            m_linesManager.SetPoint(0, index, point);

            UpdateCurve();
        }

        foreach(var g in m_linesManager.groups)
        {
            g.SetWidths(slider.value * 0.1f);
        }
    }

    private void UpdateCurve()
    {
        if (pointsClicked < 2)
            return;

        // each example
        for(int i=1; i< m_linesManager.groups.Count;++i)
        {
            var curGroup = m_linesManager.groups[i];

            // remove everything and re-generate the curve
            curGroup.Clear();

            // each line list in this example
            for (int j = 0; j < curGroup.numLines; j++)
            {
                if(curGroup.visible)
                {
                    // for 1-less than the control points
                    for (int k = 0; k < draggablePoints.Count - 1; ++k)
                    {
                        m_linesManager.AddPoint(curGroup.index + j);
                        Vector3 pos = new Vector3();
                        Vector3 p1 = m_linesManager.GetPoint(0, k);
                        Vector3 p2 = m_linesManager.GetPoint(0, k + 1);

                        switch (i)
                        {
                            case 1:
                                var toNext = p2 - p1;
                                pos = p1 + toNext * (j * (1.0f / (float)curGroup.numLines));
                                break;
                            case 2:
                                var midp = (p1 + p2) * 0.5f;
                                pos = midp;
                                break;
                        }
                        m_linesManager.SetPoint(curGroup.index + j, k, pos);
                    }
                }
            }
        }
    }

    // create buttons/labels with ImGUI in here
    private void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 200, 50), "<color='green'><size=30>Clear all</size></color>"))
        {
            pointsClicked = 0;

            foreach (var g in m_linesManager.groups)
            {
                g.Clear();
            }
            foreach (var o in draggablePoints)
            {
                Destroy(o);
            }
            draggablePoints.Clear();
        }

        for (int i = 1; i < m_linesManager.groups.Count; ++i)
        {
            var g = m_linesManager.groups[i];
            string color = ColorUtility.ToHtmlStringRGBA(m_linesManager.groups[i].col);
            string onoff = g.visible ? "on" : "off";
            string label = "<color=#" + color + "><size=30>Example " + i.ToString() + " " + onoff + "</size></color>";

            if (GUI.Button(new Rect(10, 170 + 60 * i, 200, 50), label))
            {
                g.visible = !g.visible;
                UpdateCurve();
            }
        }
    }
}
