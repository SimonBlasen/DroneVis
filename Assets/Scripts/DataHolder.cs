using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DataHolder : MonoBehaviour
{
    [SerializeField]
    private Text dataText;
    [SerializeField]
    private InputField renderAmountsInput;
    [SerializeField]
    private GameObject pointPrefab;
    [SerializeField]
    private float clusterThreshold = 0.1f;
    [SerializeField]
    private int neededNeighbours = 4;
    [SerializeField]
    private int renderAmountsPerSecond = 0;
    [SerializeField]
    private Color[] colors;

    private List<Vector3> points = new List<Vector3>();
    private List<Vector3> recordPos = new List<Vector3>();
    private List<int> recordRot = new List<int>();
    private List<int> pointColors = new List<int>();


    private bool[] keepPointCache;

    private bool rendering = false;

    private List<GameObject> instDataPoints = new List<GameObject>();

    private float counter = 0f;
    private int animIndex = 0;

    private void Update()
    {
        if (rendering)
        {
            counter += Time.deltaTime * renderAmountsPerSecond;
            
            while (animIndex < counter)
            {
                if (pointColors[animIndex] == 0 || keepPointCache[animIndex])
                {
                    GameObject inst = Instantiate(pointPrefab);
                    inst.transform.position = points[animIndex];
                    inst.GetComponent<MeshRenderer>().sharedMaterial = new Material(inst.GetComponent<MeshRenderer>().sharedMaterial);
                    inst.GetComponent<MeshRenderer>().sharedMaterial.color = colors[pointColors[animIndex]];
                    instDataPoints.Add(inst);
                }

                animIndex++;

                if (animIndex >= points.Count)
                {
                    rendering = false;
                    break;
                }
            }
        }
    }




    public void AddData(Vector3 point, Vector3 recordPosition, int recordYaw)
    {
        AddData(point, recordPosition, recordYaw, 1);
    }

    public void AddData(Vector3 point, Vector3 recordPosition, int recordYaw, int color)
    {
        points.Add(point);
        recordPos.Add(recordPosition);
        recordRot.Add(recordYaw);
        pointColors.Add(color);

        refreshDataText();
    }


    public void ClearData()
    {
        for (int i = 0; i < instDataPoints.Count; i++)
        {
            Destroy(instDataPoints[i]);
        }

        instDataPoints.Clear();
        instDataPoints = new List<GameObject>();

        points.Clear();
        recordPos.Clear();
        recordRot.Clear();
        pointColors.Clear();

        points = new List<Vector3>();
        recordRot = new List<int>();
        recordPos = new List<Vector3>();
        pointColors = new List<int>();
        keepPointCache = null;

        refreshDataText();
    }

    public void Animate()
    {
        for (int i = 0; i < instDataPoints.Count; i++)
        {
            Destroy(instDataPoints[i]);
        }
        instDataPoints.Clear();
        instDataPoints = new List<GameObject>();

        keepPointCache = new bool[points.Count];
        for (int i = 0; i < keepPointCache.Length; i++)
        {
            keepPointCache[i] = true;
        }
        counter = 0f;
        animIndex = 0;
        renderAmountsPerSecond = System.Convert.ToInt32(renderAmountsInput.text);
        if (renderAmountsPerSecond <= 0)
        {
            renderAmountsPerSecond = 100000;
        }
        for (int i = 0; i < points.Count; i++)
        {
            if (pointColors[i] != 0)
                keepPoint(i);
        }
        rendering = true;
    }

    public void RenderData()
    {
        for (int i = 0; i < instDataPoints.Count; i++)
        {
            Destroy(instDataPoints[i]);
        }
        instDataPoints.Clear();
        instDataPoints = new List<GameObject>();

        keepPointCache = new bool[points.Count];
        for (int i = 0; i < keepPointCache.Length; i++)
        {
            keepPointCache[i] = true;
        }
        for (int i = 0; i < points.Count; i++)
        {
            if (pointColors[i] == 0 || keepPoint(i))
            {
                GameObject inst = Instantiate(pointPrefab);
                inst.transform.position = points[i];
                inst.GetComponent<MeshRenderer>().sharedMaterial = new Material(inst.GetComponent<MeshRenderer>().sharedMaterial);
                inst.GetComponent<MeshRenderer>().sharedMaterial.color = colors[pointColors[i]];
                instDataPoints.Add(inst);
            }
        }
    }


    public Color[] Colors
    {
        get
        {
            return colors;
        }
    }

    public List<int> PointColors
    {
        get
        {
            return pointColors;
        }
        set
        {
            pointColors = value;
        }
    }

    public List<int> RecrodRot
    {
        get
        {
            return recordRot;
        }
        set
        {
            recordRot = value;
        }
    }

    public List<Vector3> RecordedPos
    {
        get
        {
            return recordPos;
        }
        set
        {
            recordPos = value;
        }
    }

    public List<Vector3> Points
    {
        get
        {
            return points;
        }
        set
        {
            points = value;
        }
    }

    private bool keepPoint(int index)
    {
        int neighbours = 0;
        for (int i = 0; i < points.Count; i++)
        {
            if (pointColors[i] != 0 && i != index)
            {
                if (Mathf.Sqrt((points[i].x - points[index].x) * (points[i].x - points[index].x) + (points[i].z - points[index].z) * (points[i].z - points[index].z)) < clusterThreshold)
                {
                    neighbours++;
                    if (neighbours >= neededNeighbours)
                    {
                        break;
                    }
                }
            }
        }

        keepPointCache[index] = neighbours >= neededNeighbours;

        return neighbours >= neededNeighbours;
    }

    private void refreshDataText()
    {
        dataText.text = "Data points: " + points.Count.ToString();
    }

}
