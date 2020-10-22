using Rnd = UnityEngine.Random;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;

// native function with piecewise_linear_distribution
// generates coordinates with bias towards EndP
delegate int GenerateCoordinates(float[] Xs, float[] Ys, int len, float xmin, float xmax, float ymin, float ymax);

public struct InputVec
{
    public float steering;
    public const float L = 0.5F;
    public float v;

    public InputVec(float _steering, float _v)
    {
        steering = _steering;
        v = _v;
    }

    public InputVec Add(float k)
    {
        InputVec result = new InputVec(steering, v);
        result.steering += k;
        //result.v += k;
        return result;
    }
};

public struct OutputVec
{
    public float x;
    public float z;
    public float theta;
    public OutputVec(float _x, float _z, float _theta)
    {
        x = _x; z = _z; theta = _theta;
    }
};

public struct Edge
{
    public Vertex xs;
    public Vertex xe;
    public float u;
    public int idx;
    public Edge(Vertex _xs, Vertex _xe, float _u)
    {
        xs = _xs; xe = _xe; u = _u; idx = 0;
    }
    public void setY(float y)
    {
        xs.value.y = y;
        xe.value.y = y;
    }
};

public class Vertex
{
    public Vertex()
    { }
    public Vertex(Vector3 _value)
    {
        value = _value;
    }
    public Vertex(Vector3 _value, Vertex parent_vertex)
    {
        value = _value;
        parent = parent_vertex;
    }
    public Vector3 value = new Vector3(0, 0, 0);
    public Vertex parent = null;
    public float dist = 0;
    public float theta = 0;
    // состовляется добавлением edge0.id + " " + edge1.id + ...
    // int code = string.toInt();
    public string path_to_root_code;
    public SteeringLaw steering_law = new SteeringLaw(-60, 60,   Time.deltaTime);
};

public class Law
{
    public Law(float _start, float _end, float _ttotal)
    {
        value = _start;
        start = _start; end = _end; ttotal = _ttotal;
    }
    public float start;
    public float end;
    public float value;
    public float ttotal;

    public static float lin_interp(float from, float to, float t0, float ttek, float ttotal)
    {
        if (ttek <= t0) return from;
        if (ttek >= (t0 + ttotal)) return to;
        float value = from + (to - from) * (ttek - t0) / ttotal;
        return value;
    }

    public float getSample(float dt)
    {
        //value = lin_interp(start, end, t0, t, ttotal);
        value += (end - start) * dt / ttotal;
        if (value > end)
            value = end;
        if (value < start)
            value = start;
        return value;
    }
    public bool isTimeOut(float dt)
    {
        float v = value;
         v += (end - start) * dt / ttotal;
        if (v >= end)
            return true;
        if (v <= start)
            return true;
        return false;
    }
};

public class SteeringLaw : Law
{
    public SteeringLaw(float _start, float _end,  float _ttotal)
        : base(_start, _end, _ttotal)
    {
    }
    public SteeringLaw(SteeringLaw law)
        : base(law.start, law.end, law.ttotal)
    {
        value = law.value;
    }
    public string toString()
    {
        return $"start={start} end={end} ttotal={ttotal}";
    }
};

class KinematicModel
{
    public delegate float MyFunc(InputVec input, ref OutputVec output);

    public float compute_theta(InputVec input, ref OutputVec output)
    {
        float theta = (input.v / InputVec.L) * Mathf.Tan(input.steering * Mathf.Deg2Rad);
        return theta;
    }

    public float compute_x(InputVec input, ref OutputVec output)
    {
        float theta = output.theta;
        float x = input.v * Mathf.Sin(theta);
        return x;
    }

    public float compute_z(InputVec input, ref OutputVec output)
    {
        float theta = output.theta;
        float z = input.v * Mathf.Cos(theta);
        return z;
    }

    public float runge_kutta_integration(MyFunc f, float dt, InputVec input, ref OutputVec output)
    {
        float result = 0;

        float k1 = f(input, ref output);
        //Debug.Log($"k1={k1}");
        float k2 = f(input.Add(k1 / 2), ref output);
        //Debug.Log($"k2={k2}");
        float k3 = f(input.Add(k2 / 2), ref output);
        //Debug.Log($"k3={k3}");
        float k4 = f(input.Add(k3), ref output);
        //Debug.Log($"k4={k4}");

        result = (dt / 6) * (k1 + 2 * k2 + 2 * k3 + k4);

        return result;
    }

    // Test integration of kinematic model
    public void TestKinematicModel(Vector3 startP, ref List<Edge> edges)
    {
        // Initital values
        float max_steering = 10F;     // steering 
        const float V = 10F;         // speed
        const float K = 1000;
        float t = 0;

        Vector3 xnew = startP;
        Vector3 x = startP;

        float dt = Time.deltaTime;

        InputVec input = new InputVec(max_steering, V);
        OutputVec output = new OutputVec(startP.x, startP.z, max_steering);

        for (float k = 0; k < K; k++)
        {
            x = xnew;
            input.steering = max_steering * ((K / 2 - k) / K);
            output.theta += runge_kutta_integration(compute_theta, dt, input, ref output);
            output.x += runge_kutta_integration(compute_x, dt, input, ref output);
            output.z += runge_kutta_integration(compute_z, dt, input, ref output);
            xnew.x = output.x;
            xnew.z = output.z;
            xnew.y = 0.1F;
            edges.Add(new Edge(new Vertex(x), new Vertex(xnew), input.steering));
            t += dt;
            Debug.Log($"t={t} input.steering={input.steering} theta={output.theta} x={output.x} z={output.z}");
        }


    }

 


   
    public void TestInterpolateSteering()
    {
        SteeringLaw steering_law = new SteeringLaw(-30, 30,  100 * Time.deltaTime);
        Debug.Log($"Test Interpolate Steering!");
        for (float k = 0; k < 1000; k++)
        {
            float t = k * Time.deltaTime;
            Debug.Log($"k={k} t={t} steering={steering_law.getSample(Time.deltaTime)}");
        }
    }

    public KinematicModel()
    {

        //TestInterpolateSteering();
    }
  
};




public class RRTree
{
    enum GoalState { ATUANEED, REACHED };
    public List<Vertex> vertexes;
    public List<Edge> edges;
    public Dictionary<Tuple<Vertex, Vertex>, Edge> edge_map;
    public int shortestPathIndex = 0;
    KinematicModel model = new KinematicModel();
    Vector3 startP;
    Vector3 endP;
    float dt, t = 0;
    int k = 0;
    float[] Xs, Zs; // random samples biased to endP

    public RRTree(Vector3 start, Vector3 end)
    {
        vertexes = new List<Vertex>();
        edges = new List<Edge>();
        edge_map = new Dictionary<Tuple<Vertex, Vertex>, Edge>();
        startP = start;
        endP = end;
        startP.y = TreeHeight;
        endP.y = TreeHeight;
        Xs = new float[K + 1];
        Zs = new float[K + 1];
       
    }



  
    
    //-------------------------------------------------------------------------------------------------------------------------

    public void init(Vector3 xinit)
    {
        Vertex v = new Vertex(xinit);
        vertexes.Add(v);
    }

    public Vertex add_vertex(Vector3 x)
    {
        Vertex v = new Vertex(x);
        vertexes.Add(v);
        return v;
    }

    public void add_vertex(Vertex v)
    {
        vertexes.Add(v);
    }

    public void add_edge(Vertex xnear, Vertex xnew, float Unew)
    {
        Edge e;
        e.xs = xnear;
        e.xe = xnew;
        e.u = Unew;
        e.idx = edges.Count + 1;
        edges.Add(e);
        edge_map.Add(new Tuple<Vertex, Vertex>(xnear, xnew),e);
    }

    Vertex nearest_neighbour(Vector3 x)
    {
        Vertex xnear = new Vertex();
        float minDistance = float.MaxValue;
        foreach (Vertex v in vertexes)
        {
            Vector3 d = x - v.value;
            minDistance = Mathf.Min(minDistance, d.sqrMagnitude);
            if (minDistance == d.sqrMagnitude)
            {
                xnear = v;
                //Debug.Log($"xnear {xnear.value.ToString()}");
            }
        }
        return xnear;
    }

    Vertex new_state(Vector3 xnear, float unew, float dt)
    {
        const float V = 30F;
        dt = Time.deltaTime;
        //Debug.Log($"dt={dt}");
        Vertex xnew = new Vertex();
        xnew.value.x = xnear.x + V * dt * Mathf.Cos(unew);
        xnew.value.y = TreeHeight;
        xnew.value.z = xnear.z + V * dt * Mathf.Sin(unew);
        return xnew;
    }

    float select_input(Vector3 xrand, Vector3 xnear)
    {
        Vector3 dir = xrand - xnear;
        float theta = Mathf.Atan2(dir.z, dir.x);
        //angle = Rnd.Range(0, 2 * Mathf.PI);
        return theta;
    }

    Vertex new_state2(Vertex xnear, float unew, float dt)
    {
        
        // Initital values
        const float V = 20F;         // speed
        const float max_steering = 40;
        InputVec input = new InputVec(0, V);
        OutputVec output = new OutputVec(xnear.value.x, xnear.value.z, xnear.theta);
        //Rnd.Range(-max_steering, max_steering);
        //Mathf.Atan((unew * InputVec.L ) / V ) * Mathf.Rad2Deg;
        Vertex v_out = new Vertex();
        v_out.parent = xnear;
        v_out.steering_law = new SteeringLaw(xnear.steering_law);

        // change steering law every 100 interations
        /*
        if (v_out.steering_law.isTimeOut(Time.deltaTime)) 
        {
            SteeringLaw new_steering_law = new SteeringLaw(Rnd.Range(-max_steering, max_steering), 
                                                        Rnd.Range(-max_steering, max_steering),
                                                         100 * Time.deltaTime);
            v_out.steering_law = new_steering_law;
            
            //Debug.Log("new_steering_law " + new_steering_law.toString() );
        }
        */
        //input.steering = v_out.steering_law.getSample(Time.deltaTime); //(unew - xnear.theta) * Mathf.Rad2Deg; //Rnd.Range(-max_steering, max_steering); 
        input.steering = Rnd.Range(-max_steering, max_steering);
        //Debug.Log($"k={ v_out.k } steering={input.steering} degrees");
        output.theta += model.runge_kutta_integration(model.compute_theta, dt, input, ref output);
        output.x += model.runge_kutta_integration(model.compute_x, dt, input, ref output);
        output.z += model.runge_kutta_integration(model.compute_z, dt, input, ref output);
  
        v_out.value = new Vector3(output.x, TreeHeight, output.z);
        v_out.theta = output.theta;
        return v_out;
    
    }

   

    GoalState Extend(Vector3 x)
    {
        Vertex xnear = nearest_neighbour(x);       
        float unew = select_input(x, xnear.value);
        Vertex xnew = new_state2(xnear, unew, Time.deltaTime);
        //Debug.Log($"k={k} xnear.k={xnear.k} xnear={xnear.value} theta={xnew.theta* Mathf.Rad2Deg} unew={unew * Mathf.Rad2Deg} xnew={xnew.value}");
    
        if ( colision_free_path(xnear.value, xnew.value) )
        {
            xnew.parent = xnear;
            Vector3 d = xnear.value - xnew.value;
            xnew.dist = xnear.dist + d.sqrMagnitude;
            add_vertex(xnew);
            add_edge(xnear, xnew, unew);
            Vector3 distToGoal = xnew.value - endP;
            //Debug.Log($"k={k} distToGoal={distToGoal.sqrMagnitude}");
            if (distToGoal.sqrMagnitude < GOAL_THRESHOLD)
                return GoalState.REACHED;
        }
        return GoalState.ATUANEED;
    }

    public struct Angle
    {
        public const float FORWARD = 0;
        public const float RIGHT = 90;
        public const float BACKWARD = 180;
        public const float LEFT = 270;
    }

    bool colision_free_path(Vector3 xnear, Vector3 xnew)
    {
        // Bit shift the index of the layer (8 - obstacles) to get a bit mask
        int layerMask = 1 << 8;
        const float MIN_DIST_RAY = 1.3F;

        List<float> scan_angles = new List<float> { Angle.FORWARD, Angle.RIGHT, Angle.BACKWARD, Angle.LEFT };
        //Debug.Log($"xnear={xnear.x},{xnear.y},{xnear.z}");
        foreach (float angle in scan_angles)
        {
            Vector3 dir;
            RaycastHit hitP;
            Vector3 p = xnew;
            dir.x = Mathf.Sin(Mathf.Deg2Rad * angle);
            dir.z = Mathf.Cos(Mathf.Deg2Rad * angle);
            dir.y = TreeHeight;
            p.y = TreeHeight;
            Ray ray = new Ray(p, dir);
            if (Physics.Raycast(ray, out hitP, MIN_DIST_RAY, layerMask))
            {
                // if ((r1 == true) || (r2 == true) || (r3 == true))
                //Debug.Log($"k={k} colision " + hitP.point.ToString() + $" xnear={xnear}"); //
                //Debug.Log($"colision");
                return false;
            }
        }
        return true;

    }

    bool colision_free_path2(Vector3 xnear, Vector3 xnew)
    {
        // Bit shift the index of the layer (8 - obstacles) to get a bit mask
        int layerMask = 1 << 8;
        RaycastHit hitP;

        if (Physics.Linecast(xnear, xnew, out hitP, layerMask))
        {
            // if ((r1 == true) || (r2 == true) || (r3 == true))
            //Debug.Log($"k={k} colision " + hitP.point.ToString() + $" xnear={xnear}"); //
            //Debug.Log($"colision");
            return false;
        }
        return true;

    }

    Vector3 random_state(bool bIsBiased)
    {
        float x,z;
        if (!bIsBiased)
        {
            if (startP.x < endP.x)
                x = Rnd.Range(startP.x, endP.x);
            else
                x = Rnd.Range(endP.x, startP.x);
            if (startP.z < endP.z)
                z = Rnd.Range(startP.z, endP.z);
            else
                z = Rnd.Range(endP.z, startP.z);
            return new Vector3(x, TreeHeight, z);
        }
        else
            return new Vector3(Xs[k], TreeHeight, Zs[k]);
    }

    public float Build(IntPtr nativeLibraryPtr)
    {
        //generate random points biased towards endP
        int ret = Native.Invoke<int, GenerateCoordinates>(nativeLibraryPtr, Xs, Zs, K, startP.x, endP.x, startP.z, endP.z);
        Debug.Log("ret = " + ret);

        float shortestDist = float.MaxValue;
        init(startP);
        for (k = 0; k < K; k++)
        {
            Vector3 xrand = random_state(true);
            //Debug.Log("xrand=" + xrand.ToString());
            if( Extend(xrand) ==  GoalState.REACHED )
            {
                Vertex lastVertex = vertexes[vertexes.Count - 1];
                shortestDist = Mathf.Min(shortestDist, lastVertex.dist);
                if (shortestDist == lastVertex.dist)
                {
                    shortestPathIndex = vertexes.Count - 1;
                    //Debug.Log($"Reached at {k} node. lastVertex.dist = {lastVertex.dist} ");
                }
                //CreateSpherePrimitive(lastVertex.value, sphere_scale, $"node {k}", Color.cyan);
            }
        }
        Debug.Log($"generated {vertexes.Count} vertexes");
        Debug.Log($"generated {edges.Count} edges");
        return shortestDist;
    }

    public Vertex getShortestPath()
    {
        if(shortestPathIndex < vertexes.Count)
            return vertexes[shortestPathIndex];
        return new Vertex();
    }

    public const int K = 10000;
    public const float TreeHeight = 0.3F;
    const float MAX_DIST_RAY = 40F;
    const float GOAL_THRESHOLD = 2F;
};


public class RrtPlaner : MonoBehaviour
{
    static float[] Xs, Zs;
    public Vector3 StartP;
    public Vector3 EndP;
    public List<Component> wps;
    public RRTree rrt = null;
    static Vector3 sphere_scale = new Vector3(0.3F, 0.3F, 0.3F);
    public const int n_waypoints = 20;
    static IntPtr nativeLibraryPtr;

    // Start is called before the first frame update
    void Start()
    {
        if (nativeLibraryPtr != IntPtr.Zero) return;
        nativeLibraryPtr = Native.LoadLibrary(Directory.GetCurrentDirectory() + "\\TestCPPApp\\x64\\Debug\\TestDLL.dll");
        if (nativeLibraryPtr == IntPtr.Zero)
        {
            Debug.LogError("Failed to load native library " + Directory.GetCurrentDirectory());
        }

        GameObject StartPObject = GameObject.Find("S");
        GameObject EndPObject = GameObject.Find("E");

        StartP = StartPObject.transform.position;
        EndP  = EndPObject.transform.position;

      
    }

    GameObject CreateSpherePrimitive(Vector3 position, Vector3 scale, string name, Color color)
    {
        //create a copy of start waypoint to supress coliders
        GameObject S = GameObject.Find("S");//GameObject.CreatePrimitive(PrimitiveType.Sphere);
        GameObject sphere = UnityEngine.Object.Instantiate(S, position, Quaternion.identity);
        //GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = position;
        sphere.transform.localScale = scale;
        //sphere.transform.parent = GetComponent<Transform>();
        sphere.transform.name = name;
        var renderer = sphere.GetComponent<MeshRenderer>();
        renderer.material.SetColor("_Color", color);
    
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

  
    void TestCoordinates(int numberPoints, Vector3 startP, Vector3 endP)
    {
        Debug.Log($"numberPoints = {numberPoints} "); 
        float[] Xs = new float[numberPoints+1];
        float[] Zs = new float[numberPoints+1];
        int ret = Native.Invoke<int, GenerateCoordinates>(nativeLibraryPtr, Xs, Zs, numberPoints, startP.x, endP.x, startP.z, endP.z);
        Debug.Log("ret = " + ret);
        for (int i = 0; i < numberPoints; i++)
        {
            //Debug.Log($"X = {Xs[i]} Z = {Zs[i]}");
            CreateSpherePrimitive(new Vector3(Xs[i], 1, Zs[i]), sphere_scale, $"node {i}", Color.cyan);
        }
    }



    // Update is called once per frame
    void FixedUpdate()
    {
        float shortestPathDist = 0;
        //check that maze is generated
        GameObject ground = GameObject.Find("Plane");
        MazeGen maze = ground.GetComponent<MazeGen>();
        n_frame++;

        if (maze.bMazeGenerated && n_frame == 120)
        {
            rrt = new RRTree(StartP, EndP);
            shortestPathDist = rrt.Build(nativeLibraryPtr);
            KinematicModel model = new KinematicModel();
            //model.TestKinematicModel(StartP.transform.position, ref rrt.edges);
            //create nodes of a RRT as spheres
            int i = 0;
            //foreach (Vertex v in rrt.vertexes)
            //    if(v.parent == null)
            //      CreateSpherePrimitive(v.value, sphere_scale, $"node {i++}", Color.cyan);
            //TestCoordinates(1000, StartP.transform.position, EndP.transform.position);
            // render the shortest path
            Vertex v = rrt.getShortestPath();//rrt.vertexes[RRTree.K/2];
            //Debug.Log($"rrt.shortestPathIndex = {rrt.shortestPathIndex}");
            int max_iters = RRTree.K;
            // insert sphere every 1/20 part of the path
        
            float interval_dist = shortestPathDist / n_waypoints;
            float d = 0;
            int idx = 0;

            while (v != null)
            {
                if (v.parent != null)
                    d += Vector3.Distance(v.parent.value, v.value);
                if (d > interval_dist)
                {
                    Vector3 pos = v.value;
                    pos.y = 10;
                    AddWaypoint(v.value, idx, Color.green);
                    d = 0;
                }
                v = v.parent;
                idx++;



            }

            // reindex waypoints
            int idx2 = wps.Count;
            foreach (Component wp in wps)
                wp.name = $"waypoint_{idx2--}";
                           
        }

        //---------- TREE RENDERING
        if (wps.Count == 2)
        {
            
            StartP = wps[0].transform.position;
            EndP = wps[1].transform.position;
            Debug.DrawLine(StartP, EndP);
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
            int max_iters = RRTree.K;

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
