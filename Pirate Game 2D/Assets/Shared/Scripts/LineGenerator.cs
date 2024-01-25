using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using System;
using UnityEngine.UIElements;

enum LineTypes
{
    Dot,
    HorizontalLine,
    VerticalLine,
    DiagonalLine,
    Curve,
    Circle
}

public enum RuneTypes
{
    Ice,
    Fire,
    Invalid
}

public struct RuneInfo
{
    public RuneTypes type;
    public int accuracy;

    public RuneInfo(RuneTypes type, int accuracy)
    {
        this.type = type;
        this.accuracy = accuracy;
    }
}

struct LineInfo
{
    public LineTypes lineType;
    public List<Vector3> positions;
    public int accuracy;

    public LineInfo(LineTypes lineType, List<Vector3> positions, int accuracy)
    {
        this.lineType = lineType;
        this.positions = positions;
        this.accuracy = accuracy;
    }
}

public class LineGenerator : MonoBehaviour
{
    public static event Action<RuneInfo> OnRuneComplete;

    public GameObject linePrefab;

    public AudioSource drawingSound;

    List<DrawLine> lines = new List<DrawLine>();

    DrawLine activeLine;

    private float width;
    private float height;

    private void OnDestroy()
    {
        if (Input.GetMouseButton(0) && activeLine) lines.Add(activeLine);
        List<LineInfo> lineTypes = new List<LineInfo>();
        var lineLookup = new Dictionary<LineTypes, List<int>>();
        for (int i = 0; i < lines.Count; i++)
        {
            Vector3[] positions = new Vector3[lines[i].lineRenderer.positionCount];
            lines[i].lineRenderer.GetPositions(positions);
            if (lines[i].lineRenderer.positionCount == 2)
            {
                List<Vector3> linePositions = new List<Vector3>() { positions[0] };
                LineInfo newInfo = new LineInfo(LineTypes.Dot, linePositions, 100);
                lineTypes.Add(newInfo);
            }
            else if (lines[i].lineRenderer.positionCount >= 3)
            {
                float avgGradient = 0;
                float minGradient = Mathf.Infinity;
                float maxGradient = Mathf.NegativeInfinity;
                int infGrads = 0;
                for (int j = 1; j < positions.Length - 1; j++)
                {
                    float gradient = (positions[j].y - positions[j - 1].y) / (positions[j].x - positions[j - 1].x);
                    if (gradient < minGradient) minGradient = gradient;
                    if (gradient > maxGradient) maxGradient = gradient;
                    if (gradient == Mathf.Infinity || gradient == Mathf.NegativeInfinity) infGrads++;
                    else avgGradient += gradient;
                }
                if (infGrads >= (positions.Length - 2) / 3) avgGradient += 1000000;
                avgGradient /= positions.Length - 2;
                if (minGradient == Mathf.Infinity || minGradient == Mathf.NegativeInfinity) minGradient = -10 * infGrads;
                if (maxGradient == Mathf.Infinity || maxGradient == Mathf.NegativeInfinity) maxGradient = 10 * infGrads;
                if (avgGradient <= 0.3f && avgGradient >= -0.3f) AddNewLineInfo(positions, LineTypes.HorizontalLine, 
                    100 - (int)(10 * Mathf.Abs(minGradient)) - (int)(10 * Mathf.Abs(maxGradient)), i, lineTypes, lineLookup);
                else if (avgGradient >= 4.0f || avgGradient <= -4.0f) AddNewLineInfo(positions, LineTypes.VerticalLine, 
                    100 - (int)Mathf.Max(0, 15 - Mathf.Abs(minGradient)) - (int)Mathf.Max(0, 15 - Mathf.Abs(maxGradient)), i, lineTypes, lineLookup);
                else AddNewLineInfo(positions, LineTypes.DiagonalLine, 100 - (int)(10 * (maxGradient - minGradient)), i, lineTypes, lineLookup);

            }
            Destroy(lines[i].gameObject);
        }
        CheckRunes(lineTypes, lineLookup);
    }

    void CheckRunes(List<LineInfo> lineTypes, Dictionary<LineTypes, List<int>> lineLookup)
    {
        RuneInfo rInfo = new RuneInfo(RuneTypes.Invalid, -1);
        int count = 0;
        do
        {
            switch (count)
            {
                case 0:
                    rInfo.accuracy = CheckIce(lineTypes, lineLookup);
                    if (rInfo.accuracy != -1) rInfo.type = RuneTypes.Ice;
                    break;
                case 1:
                    rInfo.accuracy = CheckFire(lineTypes, lineLookup);
                    if (rInfo.accuracy != -1) rInfo.type = RuneTypes.Fire;
                    break;
            }
            count++;
        }
        while (count < 2 && rInfo.type == RuneTypes.Invalid);
        OnRuneComplete?.Invoke(rInfo);
    }

    void AddNewLineInfo(Vector3[] positions, LineTypes type, int accuracy, int index, List<LineInfo> lineTypes, Dictionary<LineTypes, List<int>> lineLookup)
    {
        List<Vector3> linePositions = new List<Vector3>() { positions[0], positions[positions.Length - 1] };
        LineInfo newInfo = new LineInfo(type, linePositions, accuracy);
        lineTypes.Add(newInfo);
        if (!lineLookup.ContainsKey(type)) lineLookup[type] = new List<int>() { 0 };
        lineLookup[type][0]++;
        lineLookup[type].Add(index);
    }

    int CheckIce(List<LineInfo> info, Dictionary<LineTypes, List<int>> lineLookup)
    {
        if(lineLookup.ContainsKey(LineTypes.DiagonalLine) && lineLookup.ContainsKey(LineTypes.VerticalLine))
        {
            if (lineLookup[LineTypes.DiagonalLine][0] == 2 && lineLookup[LineTypes.VerticalLine][0] == 1)
            {
                bool upwardDiagonal = false;
                for(int i = 1; i < 3; i++)
                {
                    if ((info[lineLookup[LineTypes.DiagonalLine][i]].positions[0].x < info[lineLookup[LineTypes.VerticalLine][1]].positions[0].x &&
                    info[lineLookup[LineTypes.DiagonalLine][i]].positions[1].x > info[lineLookup[LineTypes.VerticalLine][1]].positions[0].x) ||
                    (info[lineLookup[LineTypes.DiagonalLine][i]].positions[0].x > info[lineLookup[LineTypes.VerticalLine][1]].positions[0].x &&
                    info[lineLookup[LineTypes.DiagonalLine][i]].positions[1].x < info[lineLookup[LineTypes.VerticalLine][1]].positions[0].x))
                    {
                        float gradient = (info[lineLookup[LineTypes.DiagonalLine][i]].positions[0].y - info[lineLookup[LineTypes.DiagonalLine][i]].positions[1].y) /
                            (info[lineLookup[LineTypes.DiagonalLine][i]].positions[0].x - info[lineLookup[LineTypes.DiagonalLine][i]].positions[1].x);
                        float c = info[lineLookup[LineTypes.DiagonalLine][i]].positions[0].y - (info[lineLookup[LineTypes.DiagonalLine][i]].positions[0].x * gradient);
                        float yInt = (gradient * info[lineLookup[LineTypes.VerticalLine][1]].positions[1].x) + c;
                        if (yInt > Mathf.Min(info[lineLookup[LineTypes.VerticalLine][1]].positions[0].y, info[lineLookup[LineTypes.VerticalLine][1]].positions[1].y)
                            && yInt < Mathf.Max(info[lineLookup[LineTypes.VerticalLine][1]].positions[0].y, info[lineLookup[LineTypes.VerticalLine][1]].positions[1].y))
                        {
                            if (info[lineLookup[LineTypes.DiagonalLine][i]].positions[0].x < info[lineLookup[LineTypes.DiagonalLine][i]].positions[1].x)
                            {
                                if (i == 1 && info[lineLookup[LineTypes.DiagonalLine][i]].positions[0].y < info[lineLookup[LineTypes.DiagonalLine][i]].positions[1].y)
                                {
                                    upwardDiagonal = true;
                                }
                                else if (i == 1) continue;
                                else if (i == 2 && upwardDiagonal &&
                                info[lineLookup[LineTypes.DiagonalLine][i]].positions[0].y > info[lineLookup[LineTypes.DiagonalLine][i]].positions[1].y)
                                    return IceAccuracyCalculator(info, lineLookup);
                                else if (i == 2 && !upwardDiagonal &&
                                    info[lineLookup[LineTypes.DiagonalLine][i]].positions[0].y < info[lineLookup[LineTypes.DiagonalLine][i]].positions[1].y)
                                    return IceAccuracyCalculator(info, lineLookup);
                                else return -1;
                            }
                            else if (info[lineLookup[LineTypes.DiagonalLine][i]].positions[0].x > info[lineLookup[LineTypes.DiagonalLine][i]].positions[1].x)
                            {
                                if (i == 1 && info[lineLookup[LineTypes.DiagonalLine][i]].positions[0].y > info[lineLookup[LineTypes.DiagonalLine][i]].positions[1].y)
                                {
                                    upwardDiagonal = true;
                                }
                                else if (i == 1) continue;
                                else if (i == 2 && upwardDiagonal &&
                                info[lineLookup[LineTypes.DiagonalLine][i]].positions[0].y < info[lineLookup[LineTypes.DiagonalLine][i]].positions[1].y)
                                    return IceAccuracyCalculator(info, lineLookup);
                                else if (i == 2 && !upwardDiagonal &&
                                    info[lineLookup[LineTypes.DiagonalLine][i]].positions[0].y > info[lineLookup[LineTypes.DiagonalLine][i]].positions[1].y)
                                    return IceAccuracyCalculator(info, lineLookup);
                                else return -1;
                            }
                        }
                        else return -1;
                    }
                }
            }
        }
        return -1;
    }

    int IceAccuracyCalculator(List<LineInfo> info, Dictionary<LineTypes, List<int>> lineLookup)
    {
        int lineAccuracy = (info[0].accuracy + info[1].accuracy + info[2].accuracy) / 3;
        //Debug.Log("Accuracy: " + lineAccuracy);
        float vertLineMiddle = info[lineLookup[LineTypes.VerticalLine][1]].positions[0].x + (info[lineLookup[LineTypes.VerticalLine][1]].positions[1].x - info[lineLookup[LineTypes.VerticalLine][1]].positions[0].x)/2;
        float gradientOne = (info[lineLookup[LineTypes.DiagonalLine][1]].positions[0].y - info[lineLookup[LineTypes.DiagonalLine][1]].positions[1].y) /
                            (info[lineLookup[LineTypes.DiagonalLine][1]].positions[0].x - info[lineLookup[LineTypes.DiagonalLine][1]].positions[1].x);
        float cOne = info[lineLookup[LineTypes.DiagonalLine][1]].positions[0].y - (info[lineLookup[LineTypes.DiagonalLine][1]].positions[0].x * gradientOne);
        float yIntOne = (gradientOne * vertLineMiddle) + cOne;
        float gradientTwo = (info[lineLookup[LineTypes.DiagonalLine][2]].positions[0].y - info[lineLookup[LineTypes.DiagonalLine][2]].positions[1].y) /
                            (info[lineLookup[LineTypes.DiagonalLine][2]].positions[0].x - info[lineLookup[LineTypes.DiagonalLine][2]].positions[1].x);
        float cTwo = info[lineLookup[LineTypes.DiagonalLine][2]].positions[0].y - (info[lineLookup[LineTypes.DiagonalLine][2]].positions[0].x * gradientTwo);
        float yIntTwo = (gradientTwo * vertLineMiddle) + cTwo;
        lineAccuracy -= (int)Mathf.Abs(25 * (yIntOne - yIntTwo));
        //Debug.Log("Accuracy: " + lineAccuracy);
        return Mathf.Max(lineAccuracy, 10);
    }

    int CheckFire(List<LineInfo> info, Dictionary<LineTypes, List<int>> lineLookup)
    {
        if (lineLookup.ContainsKey(LineTypes.HorizontalLine) && lineLookup.ContainsKey(LineTypes.VerticalLine))
        {
            if (lineLookup[LineTypes.HorizontalLine][0] == 1 && lineLookup[LineTypes.VerticalLine][0] == 3)
            {
                float minX = Mathf.Min(info[lineLookup[LineTypes.HorizontalLine][1]].positions[0].x, info[lineLookup[LineTypes.HorizontalLine][1]].positions[1].x);
                float maxX = Mathf.Max(info[lineLookup[LineTypes.HorizontalLine][1]].positions[0].x, info[lineLookup[LineTypes.HorizontalLine][1]].positions[1].x);
                for(int i = 1; i < 4; i++)
                {
                    float minY = Mathf.Min(info[lineLookup[LineTypes.VerticalLine][i]].positions[0].y, info[lineLookup[LineTypes.VerticalLine][i]].positions[1].y);
                    if(minY < info[lineLookup[LineTypes.HorizontalLine][1]].positions[0].y ||
                        minX > info[lineLookup[LineTypes.VerticalLine][i]].positions[0].x ||
                        maxX < info[lineLookup[LineTypes.VerticalLine][i]].positions[0].x)
                    {
                        return -1;
                    }
                }
                return FireAccuracyCalculator(info, lineLookup);
            }
        }
        return -1;
    }

    int FireAccuracyCalculator(List<LineInfo> info, Dictionary<LineTypes, List<int>> lineLookup)
    {
        int lineAccuracy = (info[0].accuracy + info[1].accuracy + info[2].accuracy + info[3].accuracy) / 4;
        //Debug.Log("Accuracy: " + lineAccuracy);
        float minX = Mathf.Min(info[lineLookup[LineTypes.VerticalLine][1]].positions[0].x,
            Mathf.Min(info[lineLookup[LineTypes.VerticalLine][2]].positions[0].x, info[lineLookup[LineTypes.VerticalLine][3]].positions[0].x));
        float maxX = Mathf.Max(info[lineLookup[LineTypes.VerticalLine][1]].positions[0].x,
            Mathf.Max(info[lineLookup[LineTypes.VerticalLine][2]].positions[0].x, info[lineLookup[LineTypes.VerticalLine][3]].positions[0].x));
        float midX = info[lineLookup[LineTypes.VerticalLine][1]].positions[0].x + info[lineLookup[LineTypes.VerticalLine][2]].positions[0].x +
            info[lineLookup[LineTypes.VerticalLine][3]].positions[0].x - minX - maxX;
        lineAccuracy -= (int)Mathf.Abs((maxX - midX) - (midX - minX)) * 100;
        //Debug.Log("Accuracy: " + lineAccuracy);
        float sizeOne = Mathf.Abs(info[lineLookup[LineTypes.VerticalLine][1]].positions[0].y - info[lineLookup[LineTypes.VerticalLine][1]].positions[1].y);
        float sizeTwo = Mathf.Abs(info[lineLookup[LineTypes.VerticalLine][2]].positions[0].y - info[lineLookup[LineTypes.VerticalLine][2]].positions[1].y);
        float sizeThree = Mathf.Abs(info[lineLookup[LineTypes.VerticalLine][3]].positions[0].y - info[lineLookup[LineTypes.VerticalLine][3]].positions[1].y);
        lineAccuracy -= (int)((Mathf.Abs(sizeOne - sizeTwo) * 25) + (Mathf.Abs(sizeOne - sizeThree) * 25));
        //Debug.Log("Accuracy: " + lineAccuracy);
        return Mathf.Max(lineAccuracy, 10);
    }

    public void Awake()
    {
        width = Mathf.Abs(transform.position.x - (transform as RectTransform).TransformPoint((transform as RectTransform).rect.center).x) * 2;
        height = Mathf.Abs(transform.position.y - (transform as RectTransform).TransformPoint((transform as RectTransform).rect.center).y) * 2;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (InsideBox())
            {
                drawingSound.pitch = UnityEngine.Random.Range(0.8f, 1.2f);
                drawingSound.Play();
                GameObject newLine = Instantiate(linePrefab);
                activeLine = newLine.GetComponent<DrawLine>();
            }
        }
        if (Input.GetMouseButtonUp(0))
            {
            if (activeLine)
            {
                drawingSound.Stop();
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.transform.position.z))
                    - new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, 0);
                activeLine.FinishLine(mousePos);
                lines.Add(activeLine);
                activeLine = null;
            }
        }
        if(Input.GetMouseButton(0))
        {
            if (!InsideBox() && activeLine)
            {
                drawingSound.Stop();
                lines.Add(activeLine);
                activeLine = null;
            }
            else if(InsideBox() && !activeLine)
            {
                drawingSound.pitch = UnityEngine.Random.Range(0.8f, 1.2f);
                drawingSound.Play();
                GameObject newLine = Instantiate(linePrefab);
                activeLine = newLine.GetComponent<DrawLine>();
            }
        }
        
        if(activeLine != null)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.transform.position.z))
                - new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, 0);
            activeLine.UpdateLine(mousePos);
        }
    }

    private bool InsideBox()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.transform.position.z));
        return mousePos.x > transform.position.x && mousePos.x < transform.position.x + width 
            && mousePos.y < transform.position.y && mousePos.y > transform.position.y - height;
    }
}
