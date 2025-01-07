using System.Collections;
using System.Collections.Generic;
using Habrador_Computational_Geometry;
using UnityEngine;

public class GenerateMesh : MonoBehaviour
{

    public List<Vector3> points = new List<Vector3>();
    public Material material;

    private List<MyVector2> vertices = new List<MyVector2>();
    private List<Vector3> totalVertices = new List<Vector3>();
    private List<int> triangles = new List<int>();

    private float yPos = 0f;
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < points.Count; i++)
        {
            vertices.Add(new MyVector2(points[i].x, points[i].z));
            yPos = (yPos * i + points[i].y) / (i + 1); ;
        }

        EarClippingMethod(vertices);
    }

    private void EarClippingMethod(List<MyVector2> vertices)
    {
        EarClippingPolygon earClippingPolygon = new EarClippingPolygon(new Polygon2(vertices));
        // Triangulate the polygon using the Ear Clipping algorithm
        List<MyVector2> polygonVertices = earClippingPolygon.polygon.vertices;
        HashSet<Triangle2> triangulatedPoly = _EarClipping.Triangulate(polygonVertices);

        // Convert the 2D triangles to 3D vertices and create the mesh
        Mesh mesh = new Mesh();
        foreach (Triangle2 tri in triangulatedPoly)
        {
            totalVertices.Add(new Vector3(tri.p1.x, yPos, tri.p1.y));
            totalVertices.Add(new Vector3(tri.p2.x, yPos, tri.p2.y));
            totalVertices.Add(new Vector3(tri.p3.x, yPos, tri.p3.y));

            int currentVertexCount = totalVertices.Count;
            triangles.Add(currentVertexCount - 3); // First vertex of the last triangle
            triangles.Add(currentVertexCount - 2); // Second vertex of the last triangle
            triangles.Add(currentVertexCount - 1); // Third vertex of the last triangle
        }

        mesh.vertices = totalVertices.ToArray();
        mesh.triangles = triangles.ToArray();


        // Calculate the UVs for the mesh
        // Vector2[] uvs = new Vector2[totalVertices.Count];
        // for (int i = 0; i < uvs.Length; i += 3)
        // {
        //     // Assuming the mesh is a flat surface on the X and Y plane, Z is ignored
        //     uvs[i] = new Vector2(totalVertices[i].x + 0.15f, (1 - totalVertices[i].y + 0.15f) * 620 / 694);
        //     uvs[i + 1] = new Vector2(totalVertices[i + 1].x + 0.15f, (1 - totalVertices[i + 1].y + 0.15f) * 620 / 694);
        //     uvs[i + 2] = new Vector2(totalVertices[i + 2].x + 0.15f, (1 - totalVertices[i + 2].y + 0.15f) * 620 / 694);
        // }

        // Assign the UVs to the mesh
        // mesh.uv = uvs;
        mesh.RecalculateNormals();
        // Assign the mesh to a MeshFilter or MeshRenderer component
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        MeshCollider meshCollider = GetComponent<MeshCollider>();
        if (meshFilter != null)
        {
            meshFilter.mesh = mesh;
            meshCollider.sharedMesh = mesh;
        }
        else
        {
            Debug.LogError("No MeshFilter component found on the GameObject.");
        }
    }
}
