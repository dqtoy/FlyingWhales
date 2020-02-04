using csDelaunay;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI.Extensions;

public class VoronoiTest : MonoBehaviour {

    // The number of polygons/sites we want
    public int polygonNumber = 200;

    // This is where we will store the resulting data
    private Dictionary<Vector2f, Site> sites;
    private List<Edge> edges;
    Voronoi voronoi;

    public GameObject colliderParent;

    void Start() {
        // Create your sites (lets call that the center of your polygons)
        List<Vector2f> points = CreateRandomPoint();

        // Create the bounds of the voronoi diagram
        // Use Rectf instead of Rect; it's a struct just like Rect and does pretty much the same,
        // but like that it allows you to run the delaunay library outside of unity (which mean also in another tread)
        Rectf bounds = new Rectf(0, 0, 512, 512);

        // There is a two ways you can create the voronoi diagram: with or without the lloyd relaxation
        // Here I used it with 2 iterations of the lloyd relaxation
        voronoi = new Voronoi(points, bounds, 10);

        // But you could also create it without lloyd relaxtion and call that function later if you want
        //Voronoi voronoi = new Voronoi(points,bounds);
        //voronoi.LloydRelaxation(5);

        // Now retreive the edges from it, and the new sites position if you used lloyd relaxtion
        sites = voronoi.SitesIndexedByLocation;
        edges = voronoi.Edges;

        DisplayVoronoiDiagram();
    }

    private List<Vector2f> CreateRandomPoint() {
        // Use Vector2f, instead of Vector2
        // Vector2f is pretty much the same than Vector2, but like you could run Voronoi in another thread
        List<Vector2f> points = new List<Vector2f>();
        for (int i = 0; i < polygonNumber; i++) {
            points.Add(new Vector2f(Random.Range(0, 512), Random.Range(0, 512)));
        }

        return points;
    }

    // Here is a very simple way to display the result using a simple bresenham line algorithm
    // Just attach this script to a quad
    private void DisplayVoronoiDiagram() {
        Texture2D tx = new Texture2D(512, 512);
        foreach (KeyValuePair<Vector2f, Site> kv in sites) {
            tx.SetPixel((int)kv.Key.x, (int)kv.Key.y, Color.red);

            GameObject newGO = new GameObject();
            newGO.transform.parent = colliderParent.transform;
            PolygonCollider2D polygon = newGO.AddComponent<PolygonCollider2D>();
            //polygon.offset = new Vector2(kv.Key.x, kv.Key.y);
            polygon.points = kv.Value.Region(voronoi.PlotBounds).Select(x => new Vector2(x.x, x.y)).ToArray();

            var vertices2D = polygon.points;
            var vertices3D = System.Array.ConvertAll<Vector2, Vector3>(vertices2D, v => v);

            // Use the triangulator to get indices for creating triangles
            var triangulator = new Triangulator(vertices2D);
            var indices = triangulator.Triangulate();

            // Generate a color for each vertex
            var colors = Enumerable.Range(0, vertices3D.Length)
                .Select(i => Random.ColorHSV())
                .ToArray();

            // Create the mesh
            var mesh = new Mesh {
                vertices = vertices3D,
                triangles = indices,
                colors = colors
            };

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            // Set up game object with mesh;
            var meshRenderer = newGO.AddComponent<MeshRenderer>();
            meshRenderer.material = new Material(Shader.Find("Sprites/Default"));

            var filter = newGO.AddComponent<MeshFilter>();
            filter.mesh = mesh;

        }
        foreach (Edge edge in edges) {
            // if the edge doesn't have clippedEnds, if was not within the bounds, dont draw it
            if (edge.ClippedEnds == null) continue;

            DrawLine(edge.ClippedEnds[LR.LEFT], edge.ClippedEnds[LR.RIGHT], tx, Color.black);
        }
        tx.Apply();

        this.GetComponent<Renderer>().material.mainTexture = tx;
    }

    // Bresenham line algorithm
    private void DrawLine(Vector2f p0, Vector2f p1, Texture2D tx, Color c, int offset = 0) {
        int x0 = (int)p0.x;
        int y0 = (int)p0.y;
        int x1 = (int)p1.x;
        int y1 = (int)p1.y;

        int dx = Mathf.Abs(x1-x0);
        int dy = Mathf.Abs(y1-y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx-dy;

        while (true) {
            tx.SetPixel(x0+offset, y0+offset, c);

            if (x0 == x1 && y0 == y1) break;
            int e2 = 2*err;
            if (e2 > -dy) {
                err -= dy;
                x0 += sx;
            }
            if (e2 < dx) {
                err += dx;
                y0 += sy;
            }
        }
    }
}
