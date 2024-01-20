using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DrawLine : MonoBehaviour
{
    public LineRenderer lineRenderer;
    List<Vector2> points;

    private void Update()
    {
        for(int i = 0; i < points.Count; i++)
        {
            Vector3 worldPos = new Vector3(points[i].x + Camera.main.transform.position.x, points[i].y + Camera.main.transform.position.y, 0);
            lineRenderer.SetPosition(i, worldPos);
        }
    }
    void SetPoint(Vector2 point)
    {
        points.Add(point);
        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPosition(points.Count - 1, point);
    }

    public void UpdateLine(Vector2 position)
    {
        if(points == null)
        {
            points = new List<Vector2>();
            SetPoint(position);
            return;
        }
        if(Vector2.Distance(points.Last(), position) > .1f) 
        {
            SetPoint(position);
        }
    }

    public void FinishLine(Vector2 position)
    {
        if(position == points.Last())
        {
            position.x += 0.01f;
            position.y += 0.01f;
        }
        SetPoint(position);
    }

   
}
