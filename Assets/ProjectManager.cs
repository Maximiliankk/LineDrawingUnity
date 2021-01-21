﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;

public class ProjectManager : MonoBehaviour
{
    public class LinesManager
    {
        public class LineData
        {
            public LineData(LineRenderer r, GameObject o)
            {
                rend = r;
                obj = o;
            }
            public LineRenderer rend;
            public GameObject obj;
            public Color col = Color.green;
            public List<Vector3> pts = new List<Vector3>();
            public float width = 0.5f;
        }
        List<LineData> allRends = new List<LineData>();

        public void AddLine(Color c, LineRenderer rend, GameObject go)
        {
            allRends.Add(new LineData(rend, go));
            var r = allRends[allRends.Count - 1];
            r.col = c;
            r.rend.positionCount = 0;
        }
        public void AddPoint(int lineIndex)
        {
            allRends[lineIndex].rend.positionCount++;
        }

        public void SetPoint(int lineIndex, int pointIndex, Vector3 pos)
        {
            allRends[lineIndex].rend.SetPosition(pointIndex, pos);
        }
        public void RemovePointFromLine(int lineIndex, int pointIndex)
        {

        }
        public void SetWidth(int lineIndex, float width)
        {
            //allRends[lineIndex].rend.SetWidth(width, width);
            allRends[lineIndex].rend.startWidth = width;
            allRends[lineIndex].rend.endWidth = width;
        }
        public void ClearLine(int lineIndex)
        {
            allRends[lineIndex].rend.positionCount = 0;
        }
    }
    LinesManager m_linesManager = new LinesManager();

    //GameObject emptyobj = new GameObject();

    public float lineWidth = 0.1f;
    public float pointWidth = 0.5f;

    // current number of active control points
    int pointsClicked = 0;

    // reference to our line renderer
    public GameObject lineRend;

    // public allows you to drag an object reference here in the editor
    public UnityEngine.UI.Slider slider;

    // flag for dragging points
    bool dragging = false;
    GameObject draggedObj = null;

    // list container for references to all the sphere gameObjects
    List<GameObject> draggablePoints = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        // set our line renderer reference to the line renderer component that
        // we attached to this object in the editor
        //lineRend = gameObject.GetComponent<LineRenderer>();
        var go = Instantiate(lineRend);
        m_linesManager.AddLine(Color.blue, go.GetComponent<LineRenderer>(), go);
        slider.value = lineWidth; // reasonable starting value
    }

    // Update is called once per frame
    void Update()
    {
        // 0 is Left mouse button, 1 is Right mouse button
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                Debug.Log("left-clicked on UI");
            }
            else
            {
                RaycastHit rh;
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out rh))
                {
                    // clicked on a point
                    dragging = true;
                    draggedObj = rh.collider.gameObject;
                }
                else
                {
                    pointsClicked++;
                    //lineRend.positionCount = pointsClicked;
                    m_linesManager.AddPoint(0);

                    // convert mouse coords to a world position
                    var point = Camera.main.ScreenToWorldPoint(
                    new Vector3(Input.mousePosition.x, Input.mousePosition.y, Mathf.Abs(Camera.main.transform.position.z)));

                    // create the sphere object to drag points around with
                    var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    go.transform.position = point;
                    go.transform.localScale = new Vector3(1,1,1) * pointWidth;
                    draggablePoints.Add(go);

                    // update the lineRenderer vertex position
                    //lineRend.SetPosition(pointsClicked - 1, point);
                    m_linesManager.SetPoint(0, pointsClicked - 1, point);
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
            for(int i=0;i<draggablePoints.Count;++i)
            {
                if (draggablePoints[i] == draggedObj)
                {
                    index = i;
                    break;
                }
            }

            //lineRend.SetPosition(index, point);
            m_linesManager.SetPoint(0, index, point);
        }

        m_linesManager.SetWidth(0, slider.value);
        //lineRend.SetWidth(slider.value, slider.value);
    }

    // create buttons/labels with ImGUI in here
    private void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 200, 50), "<color='green'><size=30>Clear all</size></color>"))
        {
            pointsClicked = 0;
            //lineRend.positionCount = 0;
            m_linesManager.ClearLine(0);
            foreach(var o in draggablePoints)
            {
                Destroy(o);
            }
            draggablePoints.Clear();
        }
    }
}