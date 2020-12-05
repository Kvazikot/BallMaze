// Kvazikot 

using Rnd = UnityEngine.Random;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;


[ExecuteInEditMode]
public class RrtPlaner : MonoBehaviour
{
    static float[] Xs, Zs;
    Vector3 StartP;
    Vector3 EndP;
    public Transform StartPObject;
    public Transform EndPObject;
    public List<Component> wps;
    public RRTree rrt = null;
    static Vector3 sphere_scale = new Vector3(0.2F, 0.2F, 0.2F);
    public int n_waypoints = 20;
    static IntPtr nativeLibraryPtr;
    public float Velocity = 20F;
    public int K_iterations = 50000;
    float shortestPathDist = 0;
    public float MAX_DIST_RAY = 40F;
    public float MIN_DIST_RAY = 5F;
    public float GOAL_THRESHOLD = 2F;

    // Start is called before the first frame update
    public void Start()
    {
        StartP = StartPObject.transform.position;
        EndP = EndPObject.transform.position;
        if (nativeLibraryPtr != IntPtr.Zero) return;
        nativeLibraryPtr = Native.LoadLibrary(Directory.GetCurrentDirectory() + "\\TestDLL\\x64\\Debug\\TestDLL.dll");
        if (nativeLibraryPtr == IntPtr.Zero)
        {
            Debug.LogError("Failed to load native library " + Directory.GetCurrentDirectory());
        }

     

      
    }

    GameObject CreateSpherePrimitive(Vector3 position, Vector3 scale, string name, Color color)
    {
        //create a copy of start waypoint to supress coliders
        GameObject S = GameObject.Find("S");//GameObject.CreatePrimitive(PrimitiveType.Sphere);
        GameObject sphere = UnityEngine.Object.Instantiate(S, position, Quaternion.identity);
        GameObject waypoints = GameObject.Find("Waypoints");
        sphere.transform.parent = waypoints.transform;
        //GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = position;
        SphereCollider collider = sphere.GetComponent<SphereCollider>();
        collider.isTrigger = true;
        sphere.transform.localScale = scale;
        //sphere.transform.parent = GetComponent<Transform>();
        sphere.transform.name = name;
        //var renderer = sphere.GetComponent<MeshRenderer>();
        //renderer.material.SetColor("_Color", color);
    
        return sphere;
    }

    void AddWaypoint(Vector3 position, int n, Color color)
    {
        GameObject sphere = CreateSpherePrimitive(position, sphere_scale, $"waypoint_{n}", color);
        wps.Add(sphere.transform);
    }

    public Component FindClosestWaypoint(Vector3 in_point, Component targetWP, ref bool bReached)
    {
        int layerMask = 1 << 8;
        const float dest_threshold_radius = 1.5f;

        float minDistance = float.MaxValue;
        if (wps.Count < 2) return null;
        Component outWayPoint=wps[0];
        //loop thru waypoints
        float d = 0;
        foreach (Component wp in wps)
        {

            if (!Physics.Linecast(in_point, wp.transform.position, layerMask))
            {

                //$"old stuff {wp.position}";
                d = Vector3.Distance(in_point, wp.transform.position);

                //Debug.Log($"distance to wp {wp.name} is {d}");
                if (d < minDistance)
                {
                    minDistance = d;
                    outWayPoint = wp;
                }
            }
        }
        if (outWayPoint.name == targetWP.name)
            bReached = true;
        if (d < dest_threshold_radius)
        {
            bReached = true;
            Debug.Log($"dist to Destination waypoint = {d}");
        }
        return outWayPoint;
    }

    public Vector3 FindClosestPointOnPath(Vector3 in_point)
    {
        if (rrt == null) return in_point;
        Vertex v = rrt.getShortestPath();
        Vector3 outPoint= new Vector3();
        float minDistance = float.MaxValue;
        //loop thru grapth vertices
        while (v != null)
        {
            float d = Vector3.Distance(in_point, v.value);
            if (d < minDistance)
            {
                minDistance = d;
                outPoint = v.value;
            }
            v = v.parent;
        }
        return outPoint;
    }

    Vertex traverseTree(Vertex root, float shortestPathDist, int n_waypoints,  float d,  int idx, int level, Color color )
    {
        float D = d;
        int IDX = idx;
		int LEVEL = level;
		Color COLOR = color;
        if (root!=null)
        {
		
            foreach (var child in root.children)
            {
                //new Color(Rnd.Range(0.0f,1.0f), Rnd.Range(0.0f, 1.0f), Rnd.Range(0.0f, 1.0f));

                //Debug.Log($"child {child.value} {child.dist} d={d} shortestPathDist={shortestPathDist} n_waypoints={n_waypoints}");
                if (child == null) continue;    
                float interval_dist = shortestPathDist / n_waypoints;
                D += Vector3.Distance(root.value, child.value);
                if (D > interval_dist)
                {
                    AddWaypoint(child.value, IDX, COLOR);
                    D = 0; IDX++;
                }
                if (child.dist > shortestPathDist)
                    break;
                LEVEL++;
                traverseTree(child, shortestPathDist, n_waypoints, D, IDX, LEVEL, COLOR);
            }
        }
        return null;
    }

    public void CreateTree()
    {
        rrt = new RRTree(StartP, EndP);
        rrt.Velocity = Velocity;
        rrt.K = K_iterations;
        rrt.MAX_DIST_RAY = MAX_DIST_RAY;
        rrt.MIN_DIST_RAY = MIN_DIST_RAY;
        rrt.GOAL_THRESHOLD = GOAL_THRESHOLD;
        shortestPathDist = rrt.Build(nativeLibraryPtr);       
    }

    public void DeleteWaypoints()
    {
        int i = 0;
        while (wps.Count!=2)
        {
            Component wp = wps[i];
            if (wp.name != "S" && wp.name != "E")
            {
                SafeDestroy.SafeDestroyGameObject(wp);
                i--;
            }
            wps.Remove(wp);
            i++;
        }
    }

    public void SetWaypoints()
    {

        //create nodes of a RRT as spheres

        // render the shortest path
        Vertex v = rrt.getShortestPath();//rrt.vertexes[RRTree.K/2];
                                         //Debug.Log($"rrt.shortestPathIndex = {rrt.shortestPathIndex}");
        int max_iters = K_iterations;

        // insert sphere every 1/20 part of the path
        Debug.Log($"n paths = {rrt.paths.Count}");
        wps.Clear();
        /*
        foreach (var path_kv in rrt.paths)
        {
            float interval_dist = path_kv.Key / n_waypoints;
            float d = 0;
            int idx = 0;
            v = rrt.vertexes[path_kv.Value];
            while (v != null)
            {
                if (v.parent != null)
                    d += Vector3.Distance(v.parent.value, v.value);
                if (d > interval_dist)
                {
                    Vector3 pos = v.value;
                    pos.y = 10;
                    AddWaypoint(v.value, idx, Color.red);
                    d = 0; idx++;

                }
                v = v.parent;
            }
            n_shortests_paths--;
            if (n_shortests_paths==0) break;
        }
        */
        //=============================================================
        /*
        int idx = 0;
        float interval_dist = shortestPathDist / n_waypoints;
        float d = 0;
        for (int i = 0; i < rrt.vertexes.Count; i++)
        {
            v = rrt.vertexes[i];
            if (v.parent != null)
                d += Vector3.Distance(v.parent.value, v.value);
            if (d > interval_dist)
            {
                AddWaypoint(v.value, idx, Color.red);
                d = 0; idx++;
            }
                
            AddWaypoint(v.value, idx, Color.red);
            if (v.dist > shortestPathDist)
                break;
        }
        */
        //--------------------------------------------------------------------
        int level = 0; Color color = Color.yellow; float d = 0; int idx = 0; 
        traverseTree(rrt.vertexes[0], shortestPathDist, n_waypoints, d, idx, level, color);

            // reindex waypoints
        int idx2 = wps.Count;
        foreach (Component wp in wps)
            wp.name = $"waypoint_{idx2--}";
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        n_frame++;

        if (n_frame  == 2)
        {
            CreateTree();
            SetWaypoints();
        }


        if ((n_frame % 1000) == 0)
        {
            DeleteWaypoints();
            SetWaypoints();
        }


        //---------- TREE RENDERING
        //if (wps.Count >= 2)
        {
            
            if (rrt == null) return;
            // render RRT edges as lines
            // foreach (Vector3 v in rrt.vertexes)
            //    Debug.DrawLine(v, v+new Vector3(0.001F, 0.001F, 0.001F), Color.green);
            Color c = new Color(Rnd.Range(0, 1), Rnd.Range(0, 1), Rnd.Range(0, 1));
            int k = 0;
            foreach (Edge e in rrt.edges)
            {
                Edge e2 = new Edge(e.xs, e.xe, 0);
                if (k % 100 == 0)
                    c = new Color(Rnd.Range(0, 1F), Rnd.Range(0, 1F), Rnd.Range(0, 1F));
                Debug.DrawLine(e.xe.value, e.xs.value, c);
                k++;
            }

            // render the shortest path
            Vertex v = rrt.getShortestPath();//rrt.vertexes[RRTree.K/2];
            //Debug.Log($"rrt.shortestPathIndex = {rrt.shortestPathIndex}");
            int max_iters = K_iterations;

            while (v != null )
            {
                if( v.parent != null )
                    Debug.DrawLine(v.parent.value, v.value, Color.red);
                v = v.parent;
                //max_iters--;
               // if (max_iters == 0)
               //     return;
            }
        }

        //tree rendering

    }

    int n_frame = 0;

    void OnApplicationQuit()
    {
        if (nativeLibraryPtr == IntPtr.Zero) return;

        Debug.Log(Native.FreeLibrary(nativeLibraryPtr)
                      ? "Native library TestDLL.dll successfully unloaded."
                      : "Native library TestDLL.dll could not be unloaded.");
    }
}
