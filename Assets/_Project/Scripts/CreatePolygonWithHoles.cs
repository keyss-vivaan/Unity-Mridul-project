using System.Collections.Generic;
using System.Linq;
using Habrador_Computational_Geometry;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class CreatePolygonWithHoles : MonoBehaviour
{
    public List<Vector3> outerPoints; // Outer polygon points
    // public List<List<Vector3>> holes = new List<List<Vector3>>(); // List of holes (each is a list of points)
    public HolesList holes = new HolesList();

    private HashSet<Triangle2> triangulation;

    private Vector3 rotatingAxis;
    private Vector3 rotatingPoint;


    private void Start()
    {

        // CreateCustomPolygonWithHoles(false);
    }

    public void CreateCustomPolygonWithHoles(bool isWall)
    {

        rotatingAxis = (outerPoints[1] - outerPoints[0]).normalized;
        rotatingPoint = outerPoints[0];
        if (outerPoints == null)
        {
            Debug.Log("We have no points on the hull");

            return;
        }
        List<MyVector2> hullVertices_2d = new List<MyVector2>();

        if (isWall)
        {
            outerPoints = RotatePlane(outerPoints);
        }
        hullVertices_2d = outerPoints.Select(p => new MyVector2(p.x, p.z)).ToList();

        List<List<MyVector2>> allHoleVertices_2d = new List<List<MyVector2>>();

        foreach (Hole hole in holes.holes)
        {
            if (isWall)
            {
                // List<MyVector2> holeVertices_2d = hole.points.Select(p => new MyVector2(p.x, p.z)).ToList();
                List<MyVector2> holeVertices_2d = RotatePlane(hole.points).Select(p => new MyVector2(p.x, p.z)).ToList();
                allHoleVertices_2d.Add(holeVertices_2d);
            }
        }


        Normalizer2 normalizer = new Normalizer2(new List<MyVector2>(hullVertices_2d));

        List<MyVector2> hullVertices_2d_normalized = normalizer.Normalize(hullVertices_2d);

        List<List<MyVector2>> allHoleVertices_2d_normalized = new List<List<MyVector2>>();

        foreach (List<MyVector2> holeVertices_2d in allHoleVertices_2d)
        {
            List<MyVector2> holeVertices_2d_normalized = normalizer.Normalize(holeVertices_2d);

            allHoleVertices_2d_normalized.Add(holeVertices_2d_normalized);
        }


        triangulation = _EarClipping.Triangulate(hullVertices_2d, allHoleVertices_2d, optimizeTriangles: false);

        Mesh mesh = new Mesh();
        mesh = _TransformBetweenDataStructures.Triangles2ToMesh(triangulation, false, rotatingPoint.y);


        if (isWall)
            RotateMeshBack(mesh, outerPoints);

        GetComponent<MeshFilter>().mesh = mesh;

    }


    List<Vector3> RotatePlane(List<Vector3> points)
    {
        // Step 1: Define the axis of rotation (from point1 to point2)
        Vector3 axis = (points[1] - points[0]).normalized;

        // Step 2: Define the angle (90 degrees)
        float angle = 90f;

        // Step 3: Create a Quaternion for the rotation
        Quaternion rotation = Quaternion.AngleAxis(angle, rotatingAxis);

        // Step 4: Rotate the other points (point3 and point4) around the axis
        points[0] = RotatePointAroundAxis(points[0], rotatingPoint, rotation);
        points[1] = RotatePointAroundAxis(points[1], rotatingPoint, rotation);
        points[2] = RotatePointAroundAxis(points[2], rotatingPoint, rotation);
        points[3] = RotatePointAroundAxis(points[3], rotatingPoint, rotation);

        return points;
    }

    Vector3 RotatePointAroundAxis(Vector3 point, Vector3 pivot, Quaternion rotation)
    {
        // Translate point to origin (relative to pivot), apply rotation, and translate back
        return rotation * (point - pivot) + pivot;
    }

    void RotateMeshBack(Mesh mesh, List<Vector3> points)
    {
        // Step 1: Get the mesh vertices
        Vector3[] vertices = mesh.vertices;

        // Step 2: Calculate the axis of rotation (between point1 and point2)
        Vector3 axis = (points[1] - points[0]).normalized;

        // Step 3: Define the pivot point (point1 in this case)
        Vector3 pivot = rotatingPoint;

        // Step 4: Create a Quaternion for the reverse rotation (-90 degrees)
        Quaternion reverseRotation = Quaternion.AngleAxis(-90f, axis);

        // Step 5: Rotate each vertex around the pivot using the reverse rotation
        for (int i = 0; i < vertices.Length; i++)
        {
            // Rotate the vertex relative to the pivot point
            vertices[i] = RotatePointAroundPivot(vertices[i], pivot, reverseRotation);
        }

        // Step 6: Apply the modified vertices back to the mesh
        mesh.vertices = vertices;

        // Recalculate normals and bounds to ensure the mesh is updated correctly
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    // Helper method to rotate a point around a pivot using a Quaternion
    Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Quaternion rotation)
    {
        // Translate point to origin (relative to pivot), apply rotation, and translate back
        return rotation * (point - pivot) + pivot;
    }


    void OnDrawGizmos()
    {
        foreach (var point in outerPoints)
        {
            Gizmos.DrawSphere(point, 0.1f);
        }

        foreach (var hole in holes.holes)
        {
            foreach (var point in hole.points)
            {
                Gizmos.DrawSphere(point, 0.03f); // hole is now a list of Vector3
            }
        }

    }
}

[System.Serializable]
public class Hole
{
    public List<Vector3> points;
}

[System.Serializable]
public class HolesList
{
    public List<Hole> holes = new List<Hole>();
}


// GenericPropertyJSON:{"name":"points","type":-1,"arraySize":4,"arrayType":"Vector3","children":[{"name":"Array","type":-1,"arraySize":4,"arrayType":"Vector3","children":[{"name":"size","type":12,"val":4},{"name":"data","type":9,"children":[{"name":"x","type":2,"val":0.81},{"name":"y","type":2,"val":0.73},{"name":"z","type":2,"val":-1.29}]},{"name":"data","type":9,"children":[{"name":"x","type":2,"val":0.81},{"name":"y","type":2,"val":0.28},{"name":"z","type":2,"val":-1.29}]},{"name":"data","type":9,"children":[{"name":"x","type":2,"val":0.5},{"name":"y","type":2,"val":0.28},{"name":"z","type":2,"val":-1.31}]},{"name":"data","type":9,"children":[{"name":"x","type":2,"val":0.5},{"name":"y","type":2,"val":0.73},{"name":"z","type":2,"val":-1.31}]}]}]}