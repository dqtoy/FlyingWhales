using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using Pathfinding.RVO;

/** Adds a GridGraph as RVO obstacles.
* Add this to a scene in which has a GridGraph based graph, when scanning (or loading from cache) the graph
* it will be added as RVO obstacles to the RVOSimulator (which must exist in the scene).
* 
* Adapted by from code from the A* Pathfinding Project Pro forums here:
* http://forum.arongranberg.com/t/rvonavmesh-equivalent-for-gridgraph-updated-code-inside/225/3
* 
* The key modification is that obstacles are generated on the edge of walkable space, not on the unwalkable nodes.
* If done the original way, unwalkable gaps one node wide will generate obstacles on the same position but pointing
* in different directions, which [is one of the things that] makes RVO agents get stuck inside them.
* \astarpro 
*/
[AddComponentMenu("Local Avoidance/RVOGridGraph")]
public class RVOGridGraph : GraphModifier {
    /** Height of the walls added for each obstacle edge.
    * If a graph contains overlapping you should set this low enough so
    * that edges on different levels do not interfere, but high enough so that
    * agents cannot move over them by mistake.
    */
    public float wallHeight = 5;

    /** Obstacles currently added to the simulator */
    List<ObstacleVertex> obstacles = new List<ObstacleVertex>();

    /** Last simulator used */
    Simulator lastSim = null;

    public bool drawGizmosWhenNotSelected = false;
    public bool drawDirectionOnGizmos = true;

    public override void OnPostCacheLoad() {
        if (!enabled) {
            return;
        }

        FullRecalculation();
    }

    public override void OnGraphsPostUpdate() {
        if (!enabled) {
            return;
        }

        FullRecalculation();
    }

    public override void OnLatePostScan() {
        if (!enabled) {
            return;
        }

        FullRecalculation();
    }

    void FullRecalculation() {
        if (!Application.isPlaying) {
            return;
        }

        RemoveObstacles();

        NavGraph[] graphs = AstarPath.active.graphs;

        RVOSimulator rvosim = FindObjectOfType(typeof(RVOSimulator)) as RVOSimulator;
        if (rvosim == null) {
            throw new System.NullReferenceException("No RVOSimulator could be found in the scene. Please add one to any GameObject");
        }

        Pathfinding.RVO.Simulator sim = rvosim.GetSimulator();

        for (int i = 0; i < graphs.Length; i++) {
            AddGraphObstacles(sim, graphs[i]);
        }

        sim.UpdateObstacles();
    }

    /** Removes obstacles which were added with AddGraphObstacles */
    void RemoveObstacles() {
        if (lastSim == null) {
            return;
        }

        Pathfinding.RVO.Simulator sim = lastSim;
        lastSim = null;

        for (int i = 0; i < obstacles.Count; i++) {
            sim.RemoveObstacle(obstacles[i]);
        }

        obstacles.Clear();
    }

    /** Adds RVO obstacles for a Grid Graph */
    void AddGraphObstacles(Pathfinding.RVO.Simulator sim, NavGraph graph) {
        if (obstacles.Count > 0 && lastSim != null && lastSim != sim) {
            Debug.LogError("Simulator has changed but some old obstacles are still added for the previous simulator. Deleting previous obstacles.");
            RemoveObstacles();
        }

        //Remember which simulator these obstacles were added to
        lastSim = sim;

        GridGraph gg = graph as GridGraph;

        if (gg == null) {
            return;
        }

        GridNode[] nodes = gg.nodes;

        for (int w = 0; w < gg.width; w++) {
            for (int d = 0; d < gg.depth; d++) {
                int iA = d * gg.width + w;
                if (nodes[iA].Walkable == false) {
                    /*
                     * Quarters order ("A" is the tested node) :
                     *  3 | 4
                     *  --A--B
                     *  2 | 1
                     *    C  D
                     */

                    // Quarter 1
                    if ((w < (gg.width - 1)) && (d > 0)) {
                        ComputeQuarter(sim, gg, w, d, 1, 0, 0, -1, 1, -1);
                    }

                    // Quarter 2
                    if ((w > 0) && (d > 0)) {
                        ComputeQuarter(sim, gg, w, d, 0, -1, -1, 0, -1, -1);
                    }

                    // Quarter 3
                    if ((w > 0) && (d < (gg.depth - 1))) {
                        ComputeQuarter(sim, gg, w, d, -1, 0, 0, 1, -1, 1);
                    }

                    // Quarter 4
                    if ((w < (gg.width - 1)) && (d < (gg.depth - 1))) {
                        ComputeQuarter(sim, gg, w, d, 0, 1, 1, 0, 1, 1);
                    }
                }
            }
        }
        Debug.Log(string.Format("RVOGridGraph.AddGraphObstacles() : {0}  obstacles", obstacles.Count));
    }

    /** Compute a quarter of 4 nodes (A, B, C, D) where A is unwalkable and add CD or CB ObstacleVertex if needed */
    void ComputeQuarter(Pathfinding.RVO.Simulator sim, GridGraph gg, int w, int d, int wB, int dB, int wC, int dC, int wD, int dD) {
        GridNode[] nodes = gg.nodes;

        int iB = (d + dB) * gg.width + w + wB;
        int iC = (d + dC) * gg.width + w + wC;
        int iD = (d + dD) * gg.width + w + wD;

        bool nodeB = nodes[iB].Walkable;
        bool nodeC = nodes[iC].Walkable;
        bool nodeD = nodes[iD].Walkable;

        if ((nodeB == false) && (nodeC == true) && (nodeD == true)) {
            obstacles.Add(sim.AddObstacle((Vector3)nodes[iC].position, (Vector3)nodes[iD].position, wallHeight));
        }

        if ((nodeB == true) && (nodeC == true) && (nodeD == true)) {
            obstacles.Add(sim.AddObstacle((Vector3)nodes[iC].position, (Vector3)nodes[iB].position, wallHeight));
        }
    }

    /** Draws Gizmos */
    public void OnDrawGizmos() {
        if (drawGizmosWhenNotSelected) {
            DrawGizmos();
        }
    }

    /** Draws Gizmos */
    public void OnDrawGizmosSelected() {
        DrawGizmos();
    }

    /** Draws Gizmos */
    public void DrawGizmos() {
        Gizmos.color = new Color(0.615f, 1, 0.06f, 1.0f);

        foreach (ObstacleVertex ov in obstacles) {
            if (ov.next == null) {
                throw new System.InvalidOperationException("RVOGridGraph.OnDrawGizmos() : obstacles[...].next == null");
            }

            Vector3 a = ov.position;
            Vector3 b = ov.next.position;

            Gizmos.DrawLine(a, b);

            if (drawDirectionOnGizmos) {
                // Draw the little arrow to show the direction of the obstacle
                Vector3 avg = (a + b) * 0.5f;
                Vector3 tang = (b - a).normalized;
                if (tang != Vector3.zero) {
                    Vector3 normal = Vector3.Cross(Vector3.up, tang);

                    Gizmos.DrawLine(avg, avg + normal);
                    Gizmos.DrawLine(avg + normal, avg + normal * 0.5f + tang * 0.5f);
                    Gizmos.DrawLine(avg + normal, avg + normal * 0.5f - tang * 0.5f);
                }
            }
        }
    }
}