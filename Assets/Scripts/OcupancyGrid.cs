using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OcupancyGrid : MonoBehaviour
{
    [SerializeField]
    private int gridDimX = 1024;
    [SerializeField]
    private int gridDimY = 4;
    [SerializeField]
    private int gridDimZ = 1024;
    [SerializeField]
    private Vector3 offset = Vector3.zero;
    [SerializeField]
    private Vector3 scale = new Vector3(1f, 1f, 1f);
    [SerializeField]
    private bool visible = false;
    [SerializeField]
    private bool visualize2DGrid = false;
    [SerializeField]
    private bool fetchData = false;
    [SerializeField]
    private int confirmationThresh = 5;
    [SerializeField]
    private GameObject dataPrefab;
    [SerializeField]
    private DataHolder dataHolder;
    [SerializeField]
    private InputField inputGridX;
    [SerializeField]
    private InputField inputGridY;
    [SerializeField]
    private InputField inputGridXSize;
    [SerializeField]
    private InputField inputGridYSize;

    private List<GameObject> inst = new List<GameObject>();

    private byte[,,] grid = null;

    // Start is called before the first frame update
    void Start()
    {
        initGrid();

        inputGridX.text = gridDimX.ToString();
        inputGridY.text = gridDimZ.ToString();
        inputGridXSize.text = scale.x.ToString();
        inputGridYSize.text = scale.z.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        if (visible != GridVisible)
        {
            GridVisible = visible;
        }

        if (fetchData)
        {
            fetchData = false;
            FetchData();
        }
    }

    public void SetParametersGUI()
    {
        gridDimX = System.Convert.ToInt32(inputGridX.text);
        gridDimZ = System.Convert.ToInt32(inputGridY.text);
        scale.x = System.Convert.ToSingle(inputGridXSize.text);
        scale.z = System.Convert.ToSingle(inputGridYSize.text);

        initGrid();
    }

    public void FetchData()
    {
        ClearData();

        for (int i = 0; i < dataHolder.Points.Count; i++)
        {
            if (dataHolder.PointColors[i] != 0)
            {
                AddPoint(dataHolder.Points[i]);
            }
        }
    }

    public void ClearData()
    {
        initGrid();
    }

    public void AddPoint(Vector3 pos)
    {
        pos = new Vector3((pos.x - offset.x) / scale.x + gridDimX * 0.5f, (pos.y - offset.y) / scale.y + gridDimY * 0.5f, (pos.z - offset.z) / scale.z + gridDimZ * 0.5f);

        Vector3Int gridPos = new Vector3Int((int)pos.x, (int)pos.y, (int)pos.z);
        if (gridPos.x >= 0 && gridPos.x < gridDimX
            && gridPos.y >= 0 && gridPos.y < gridDimY
            && gridPos.z >= 0 && gridPos.z < gridDimZ)
        {
            grid[gridPos.x, gridPos.y, gridPos.z]++;
            if (grid[gridPos.x, gridPos.y, gridPos.z] == confirmationThresh)
            {
                GameObject instO = Instantiate(dataPrefab, transform);
                instO.transform.position = new Vector3((gridPos.x + 0.5f) * scale.x, (gridPos.y + 0.5f) * scale.y, (gridPos.z + 0.5f) * scale.z)
                                            - (new Vector3(gridDimX * scale.x, gridDimY * scale.y, gridDimZ * scale.z) * 0.5f)
                                            + offset;
                instO.transform.localScale = scale;
                inst.Add(instO);
            }
        }
    }


    private bool gridVisible = false;
    public bool GridVisible
    {
        get
        {
            return gridVisible;
        }
        set
        {
            gridVisible = value;

            LineRenderer lr = GetComponent<LineRenderer>();

            if (gridVisible)
            {
                //Vector3 minPos = new Vector3(offset.x * scale.x, offset.y * scale.y, offset.z * scale.z) - (new Vector3(gridDimX * scale.x, gridDimY * scale.y, gridDimZ * scale.z)) * 0.5f;
                //Vector3 maxPos = new Vector3(offset.x * scale.x, offset.y * scale.y, offset.z * scale.z) + (new Vector3(gridDimX * scale.x, gridDimY * scale.y, gridDimZ * scale.z)) * 0.5f;
                Vector3 minPos = offset - (new Vector3(gridDimX * scale.x, gridDimY * scale.y, gridDimZ * scale.z)) * 0.5f;
                Vector3 maxPos = offset + (new Vector3(gridDimX * scale.x, gridDimY * scale.y, gridDimZ * scale.z)) * 0.5f;

                List<Vector3> linePs = new List<Vector3>();

                for (int y = 0; y <= (visualize2DGrid ? 0 : gridDimY); y++)
                {
                    for (int z = 0; z <= gridDimZ; z++)
                    {
                        if (z % 2 == 0)
                        {
                            linePs.Add(minPos + new Vector3(0f, y * scale.y, z * scale.z));
                            linePs.Add(minPos + new Vector3(maxPos.x - minPos.x, y * scale.y, z * scale.z));
                        }
                        else
                        {
                            linePs.Add(minPos + new Vector3(maxPos.x - minPos.x, y * scale.y, z * scale.z));
                            linePs.Add(minPos + new Vector3(0f, y * scale.y, z * scale.z));
                        }
                    }
                    for (int x = 0; x <= gridDimX; x++)
                    {
                        if (x % 2 == 0)
                        {
                            linePs.Add(minPos + new Vector3(x * scale.x, y * scale.y, 0f));
                            linePs.Add(minPos + new Vector3(x * scale.x, y * scale.y, maxPos.z - minPos.z));
                        }
                        else
                        {
                            linePs.Add(minPos + new Vector3(x * scale.x, y * scale.y, maxPos.z - minPos.z));
                            linePs.Add(minPos + new Vector3(x * scale.x, y * scale.y, 0f));
                        }
                    }
                }

                lr.positionCount = linePs.Count;
                lr.widthMultiplier = 0.1f * scale.x;

                lr.SetPositions(linePs.ToArray());

                Debug.Log("Visualized grid");
            }
            else
            {
                lr.SetPositions(new Vector3[] { });
            }
            
        }
    }
    
    private void initGrid()
    {
        for (int i = 0; i < inst.Count; i++)
        {
            Destroy(inst[i]);
        }
        inst.Clear();
        inst = new List<GameObject>();

        grid = new byte[gridDimX, gridDimY, gridDimZ];
        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                for (int z = 0; z < grid.GetLength(2); z++)
                {
                    grid[x, y, z] = 0;
                }
            }
        }

        if (GridVisible)
        {
            GridVisible = true;
        }

        Debug.Log("Grid inited");
    }
}
