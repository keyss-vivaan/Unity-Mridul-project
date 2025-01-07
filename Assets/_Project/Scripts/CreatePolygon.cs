using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class CreatePolygon : MonoBehaviour
{
    // List of points (in 2D space) to define the polygon
    public List<Vector3> points;

    private void Start()
    {
        CreateMeshFromPoints();
    }




    void CreateMeshFromPoints()
    {
        if (points.Count < 3)
        {
            Debug.LogError("At least 3 points are needed to create a polygon.");
            return;
        }

        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[points.Count];
        int[] triangles = new int[(points.Count - 2) * 3];

        // Assign vertices
        for (int i = 0; i < points.Count; i++)
        {
            vertices[i] = points[i];
        }

        // Create triangles
        for (int i = 0; i < points.Count - 2; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
        }

        // Assign vertices and triangles to mesh
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();  // Recalculate normals for proper shading
        mesh.RecalculateBounds();   // Recalculate bounds to fit the mesh

        // Apply the mesh to the MeshFilter component
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }
}
