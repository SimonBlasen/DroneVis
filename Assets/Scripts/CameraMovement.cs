using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public float scrollSpeed = 100f;
    public float scrollSideSpeed = 0.3f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetAxis("Mouse ScrollWheel") != 0f)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            GetComponent<Camera>().orthographicSize -= scroll * scrollSpeed * Time.deltaTime;
        }
        if (Input.GetMouseButton(1))
        {
            Vector2 delta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            delta *= -GetComponent<Camera>().orthographicSize * scrollSideSpeed * Time.deltaTime;
            transform.position = new Vector3(transform.position.x + delta.x, transform.position.y, transform.position.z + delta.y);
        }
    }
}
