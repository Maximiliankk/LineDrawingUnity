using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using System;

public class ProjectManager : MonoBehaviour
{
    readonly float lineWidth = 0.1f;
    readonly float pointWidth = 0.5f;

    // manages all line strips
    public class LinesManager
    {
        // data for one line strip
        public class LineData
        {
            public LineData(LineRenderer r, GameObject o)
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
            public List<Vector3> pts = new List<Vector3>();
            public float width = 0.1f;
        }

        // main data of LinesManager
        List<LineData> allRends = new List<LineData>();

        // public interface for LinesManager
        public void AddLine(Color c, LineRenderer rend, GameObject go)
        {
            allRends.Add(new LineData(rend, go));
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
    }
    LinesManager m_linesManager = new LinesManager();

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
        var go = Instantiate(lineRendPrefab);
        m_linesManager.AddLine(Color.blue, go.GetComponent<LineRenderer>(), go);
        var go2 = Instantiate(lineRendPrefab);
        m_linesManager.AddLine(Color.yellow, go2.GetComponent<LineRenderer>(), go2);
        slider.value = lineWidth; // reasonable starting value
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

        if (pointsClicked > 1)
            UpdateCurve();
        //if(pointsClicked > 1)
        //{
        //    var midp = m_linesManager.GetPoint(0, pointsClicked - 2) + m_linesManager.GetPoint(0, pointsClicked - 1);
        //    m_linesManager.AddPoint(1);
        //    m_linesManager.SetPoint(1, pointsClicked - 2, midp * 0.5f);
        //}
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

        m_linesManager.SetWidth(0, slider.value);
    }

    private void UpdateCurve()
    {
        if (pointsClicked < 2)
            return;

        // remove everything and re-generate the curve
        m_linesManager.ClearLine(1);
        //m_linesManager.AddPoint(1);

        for (int i = 0; i < draggablePoints.Count - 1; ++i)
        {
            m_linesManager.AddPoint(1);
            var midp = m_linesManager.GetPoint(0, i) + m_linesManager.GetPoint(0, i+1);
            m_linesManager.SetPoint(1, i, midp * 0.5f);

            //m_linesManager.SetPoint(1, i, (m_linesManager.GetPoint(0, i) + m_linesManager.GetPoint(0, i + 1)) * 0.5f);
        }
    }

    // create buttons/labels with ImGUI in here
    private void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 200, 50), "<color='green'><size=30>Clear all</size></color>"))
        {
            pointsClicked = 0;
            m_linesManager.ClearLine(0);
            m_linesManager.ClearLine(1);
            foreach (var o in draggablePoints)
            {
                Destroy(o);
            }
            draggablePoints.Clear();
        }
    }
}
