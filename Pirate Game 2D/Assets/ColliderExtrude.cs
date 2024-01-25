using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderExtrude : MonoBehaviour
{
    // Start is called before the first frame update
    private CompositeCollider2D thisCollider;
    public GameObject meshCollider;
    public float extrusionFactor;

    private Vector3[] newVertices;
    private int[] newTriangles;
    void Start()
    {
        thisCollider = GetComponent<CompositeCollider2D>();
        Extrude();
    }

    // Update is called once per frame
    void Update()
    {
      
    }
    void Extrude()
    {
        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        mesh.vertices = newVertices;
        mesh.triangles = newTriangles;

        List<Vector2> vertices = new List<Vector2>();
        for (int i = 0; i < thisCollider.pathCount; i++)
        {
            Vector2[] pathVerts = new Vector2[thisCollider.GetPathPointCount(i)];
            thisCollider.GetPath(i, pathVerts);
            
            vertices.AddRange(pathVerts);
            //Debug.Log(vertices[i]);
        }
        //Debug.Log(vertices.Count);


    }
}
