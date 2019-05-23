using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class LogfileReader : MonoBehaviour
{
    [SerializeField]
    private InputField path;
    [SerializeField]
    private DataHolder dataHolder;


    private void Start()
    {
        path.text = @"C:\Users\Simon\Downloads\mapping.log";
    }

    public void ReadFile()
    {
        if (File.Exists(path.text))
        {
            string[] lines = File.ReadAllLines(path.text);

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Length > 3)
                {
                    int x = System.Convert.ToInt32(lines[i].Substring(6).Split(',')[0]);
                    int y = System.Convert.ToInt32(lines[i].Split(',')[1].Substring(1));
                    int z = System.Convert.ToInt32(lines[i].Split(',')[2].Substring(1));
                    int yaw = System.Convert.ToInt32(lines[i].Split(',')[3].Substring(1).Split(')')[0]);


                    int m1 = System.Convert.ToInt32(lines[i].Split(',')[3].Split('(')[1]);
                    int m2 = System.Convert.ToInt32(lines[i].Split(',')[4].Substring(1));
                    int m3 = System.Convert.ToInt32(lines[i].Split(',')[5].Substring(1));
                    int m4 = System.Convert.ToInt32(lines[i].Split(',')[6].Substring(1));
                    int m5 = System.Convert.ToInt32(lines[i].Split(',')[7].Substring(1).Replace(')', ' '));

                    Vector3 midPos = (new Vector3(x, z, y)) * 0.001f;

                    Vector3 m1Point = Vector3.zero;
                    float cos = Mathf.Cos((yaw + 90f) * (Mathf.PI / 180f));
                    float sin = Mathf.Sin((yaw + 90f) * (Mathf.PI / 180f));
                    m1Point = (new Vector3(sin, 0f, -cos)) * m1 * 0.001f;
                    m1Point += midPos;

                    Vector3 m2Point = Vector3.zero;
                    cos = Mathf.Cos((yaw - 90f) * (Mathf.PI / 180f));
                    sin = Mathf.Sin((yaw - 90f) * (Mathf.PI / 180f));
                    m2Point = (new Vector3(sin, 0f, -cos)) * m2 * 0.001f;
                    m2Point += midPos;

                    Vector3 m3Point = Vector3.zero;
                    m3Point = (new Vector3(0f, 1f, 0f)) * m3 * 0.001f;
                    m3Point += midPos;

                    Vector3 m4Point = Vector3.zero;
                    cos = Mathf.Cos((yaw + 180f) * (Mathf.PI / 180f));
                    sin = Mathf.Sin((yaw + 180f) * (Mathf.PI / 180f));
                    m4Point = (new Vector3(sin, 0f, -cos)) * m4 * 0.001f;
                    m4Point += midPos;

                    Vector3 m5Point = Vector3.zero;
                    cos = Mathf.Cos((yaw) * (Mathf.PI / 180f));
                    sin = Mathf.Sin((yaw) * (Mathf.PI / 180f));
                    m5Point = (new Vector3(sin, 0f, -cos)) * m5 * 0.001f;
                    m5Point += midPos;



                    dataHolder.AddData(m1Point, midPos, yaw, 1);
                    dataHolder.AddData(m2Point, midPos, yaw, 1);
                    //dataHolder.AddData(m3Point, midPos, yaw, Color.blue);
                    dataHolder.AddData(m4Point, midPos, yaw, 1);
                    dataHolder.AddData(m5Point, midPos, yaw, 1);


                    dataHolder.AddData(midPos, midPos, yaw, 0);
                }
            }

        }
    }
}
