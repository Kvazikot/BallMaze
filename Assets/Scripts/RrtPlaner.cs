using Rnd = UnityEngine.Random;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;



public struct InputVec
{
    public float stearing;
    public const float L = 1;
    public float v;

    public InputVec(float _stearing, float _v)
    {
        stearing = _stearing;
        v = _v;
    }

    public InputVec Add(float k)
    {
        InputVec result = new InputVec(stearing, v);
        result.stearing += k;
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
    public Vector3 xs;
    public Vector3 xe;
    public float u;
    public Edge(Vector3 _xs, Vector3 _xe, float _u)
    {
        xs = _xs; xe = _xe; u = _u;
    }
};

public class Vertex
{
    public Vertex()
    {
        parent = null;
        value = new Vector3(0,0,0);
        dist = 0;
    }
    public Vertex(Vector3 _value)
    {
        value = _value;
        parent = null;
        dist = 0;
    }
    public Vertex(Vector3 _value, Vertex parent_vertex)
    {
        value = _value;
        parent = parent_vertex;
        dist = 0;
    }
    public Vector3 value;
    public Vertex parent;
    public float dist;
};

class KinematicModel
{
    public delegate float MyFunc(InputVec input, ref OutputVec output);

    float compute_theta(InputVec input, ref OutputVec output)
    {
        float theta = (input.v / InputVec.L) * Mathf.Tan(input.stearing * Mathf.Deg2Rad);
        return theta;
    }

    float compute_x(InputVec input, ref OutputVec output)
    {
        float theta = output.theta; 
        float x = input.v * Mathf.Sin(theta);
        return x;
    }

    float compute_z(InputVec input, ref OutputVec output)
    {
        float theta = output.theta;
        float z = input.v * Mathf.Cos(theta);
        return z;
    }

    float runge_kutta_integration(MyFunc f, float dt, InputVec input, ref OutputVec output)
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
        float max_stearing = 10F;     // stearing 
        const float V = 10F;         // speed
        const float K = 1000;
        float t = 0;

        Vector3 xnew = startP;
        Vector3 x = startP;

        float dt = Time.deltaTime;

        InputVec input = new InputVec(max_stearing, V);
        OutputVec output = new OutputVec(startP.x, startP.z, max_stearing);

        for (float k = 0; k < K; k++)
        {
            x = xnew;
            input.stearing = max_stearing * ((K/2 - k) / K) ;
            output.theta += runge_kutta_integration(compute_theta, dt, input, ref output);
            output.x += runge_kutta_integration(compute_x, dt, input, ref output);
            output.z += runge_kutta_integration(compute_z, dt, input, ref output);
            xnew.x = output.x;
            xnew.z = output.z;
            xnew.y = 1;
            edges.Add(new Edge(x, xnew, input.stearing));
            t += dt;
            Debug.Log($"t={t} input.stearing={input.stearing} theta={output.theta} x={output.x} z={output.z}");
        }

    }
};




public class RRTree
{
    enum GoalState { ATUANEED, REACHED };

    public RRTree(Vector3 start, Vector3 end)
    {
        vertexes = new List<Vertex>();
        edges = new List<Edge>();
        startP = start;
        endP = end;
        //Build(startP);
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

    public void add_edge(Vector3 xnear, Vector3 xnew, float Unew)
    {
        Edge e;
        e.xs = xnear;
        e.xe = xnew;
        e.u = Unew;
        edges.Add(e);
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

    Vector3 new_state(Vector3 xnear, float unew, float dt)
    {
        const float V = 30F;
        dt = Time.deltaTime;
        //Debug.Log($"dt={dt}");
        Vector3 xnew = new Vector3();
        xnew.x = xnear.x + V * dt * Mathf.Cos(unew);
        xnew.y = 1;
        xnew.z = xnear.z + V * dt * Mathf.Sin(unew);
        return xnew;
    }

    float select_input(Vector3 xrand, Vector3 xnear)
    {
        Vector3 dir = xnear - xrand;
        float angle = Mathf.Atan2(dir.z, dir.x);
        angle = Rnd.Range(0, 2 * Mathf.PI);
        return angle;
    }

    bool colision_free_path(Vector3 xnear, Vector3 xnew)
    {
        // Bit shift the index of the layer (8 - obstacles) to get a bit mask
        int layerMask = 1 << 8;
        bool r1 = false;
        bool r2 = false;
        r1 = Physics.Raycast(xnear, xnew - xnear, MAX_DIST_RAY, layerMask);
        xnear.y = 0;
        xnew.y = 0;
        r2 = Physics.Raycast(xnear, xnew - xnear, MAX_DIST_RAY, layerMask);
        if ((r1 == true) || (r2 == true))
            return false;
        else
            return true;

    }



    GoalState Extend(Vector3 x)
    {
        Vertex xnear = nearest_neighbour(x);
        float unew = select_input(x, xnear.value);
        Vector3 xnew = new_state(xnear.value, unew, dt);
        //Debug.Log($"k={k} xnear={xnear} unew={unew} xnew={xnew}");
    
        if ( colision_free_path(xnear.value, xnew) )
        {
            Vertex v = add_vertex(xnew);
            v.parent = xnear;
            Vector3 d = xnear.value - xnew;
            v.dist = xnear.dist + d.sqrMagnitude;
            add_edge(xnear.value, xnew, unew);
            Vector3 distToGoal = xnew - endP;
            //Debug.Log($"k={k} distToGoal={distToGoal.sqrMagnitude}");
            if (distToGoal.sqrMagnitude < GOAL_THRESHOLD)
                return GoalState.REACHED;
        }
        return GoalState.ATUANEED;
    }

    Vector3 random_state(bool bIsBiased)
    {
        float x,z;
        if (startP.x < endP.x)
             x = Rnd.Range(startP.x, endP.x);
        else
            x = Rnd.Range(endP.x, startP.x);
        if (startP.z < endP.z)
            z = Rnd.Range(startP.z, endP.z);
        else
            z = Rnd.Range(endP.z, startP.z);
        return new Vector3(x, 1, z);
    }

    public void Build(Vector3 xinit)
    {
        float shortestDist = float.MaxValue;
        init(xinit);
        for (k = 0; k < K; k++)
        {
            Vector3 xrand = random_state(false);
            //Debug.Log("xrand=" + xrand.ToString());
            if( Extend(xrand) ==  GoalState.REACHED )
            {
                Vertex lastVertex = vertexes[vertexes.Count - 1];
                shortestDist = Mathf.Min(shortestDist, lastVertex.dist);
                if (shortestDist == lastVertex.dist)
                {
                    shortestPathIndex = vertexes.Count - 1;
                    Debug.Log($"Reached at {k} node. lastVertex.dist = {lastVertex.dist} ");
                }
                //CreateSpherePrimitive(lastVertex.value, sphere_scale, $"node {k}", Color.cyan);
            }
        }
        Debug.Log($"generated {vertexes.Count} vertexes");
        Debug.Log($"generated {edges.Count} edges");
    }

    public Vertex getShortestPath()
    {
        if(shortestPathIndex < vertexes.Count)
            return vertexes[shortestPathIndex];
        return new Vertex();
    }
    public List<Vertex> vertexes;
    public List<Edge> edges;
    public int shortestPathIndex = 0;

    Vector3 startP;
    Vector3 endP;
    float dt, t=0;
    int k = 0;
    public const int K = 100;
    const float MAX_DIST_RAY = 40F;
    const float GOAL_THRESHOLD = 2F;
};


public class RrtPlaner : MonoBehaviour
{
    public Vector3 StartP;
    public Vector3 EndP;
    public List<Component> wps;
    public RRTree rrt = null;


    void CreateSpherePrimitive(Vector3 position, Vector3 scale, string name, Color color)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = position;
        sphere.transform.localScale = scale;
        sphere.transform.parent = GetComponent<Transform>();
        sphere.transform.name = name;
        var renderer = sphere.GetComponent<MeshRenderer>();
        renderer.material.SetColor("_Color", color);
    }

    // Start is called before the first frame update
    void Start()
    {
        Component[] obs_list;

        GameObject o = GameObject.Find("Obstacles");
        obs_list = o.GetComponentsInChildren(typeof(Transform));
        //foreach(GameObject p in wps.
        //GameObject[] points = wps.GetComponents<GameObject>();
        //Debug.Log("number of obstacles is " + obs_list.Length);
        foreach (Transform p in obs_list)
        {
       
         
           // Debug.Log("obs " + p.name);
        }
      
        GameObject StartP = GameObject.Find("S");
        GameObject EndP = GameObject.Find("E");
        rrt = new RRTree(StartP.transform.position, EndP.transform.position);
        KinematicModel model = new KinematicModel();
        model.TestKinematicModel(StartP.transform.position, ref rrt.edges);
        //create nodes of a RRT as spheres
        int i = 0;
        //foreach (Vertex v in rrt.vertexes)
        //    if(v.parent == null)
        //      CreateSpherePrimitive(v.value, sphere_scale, $"node {i++}", Color.cyan);

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (wps.Count == 2)
        {
            StartP = wps[0].transform.position;
            EndP = wps[1].transform.position;
            Debug.DrawLine(StartP, EndP);
            if (rrt == null) return;
            // render RRT edges as lines
            // foreach (Vector3 v in rrt.vertexes)
            //    Debug.DrawLine(v, v+new Vector3(0.001F, 0.001F, 0.001F), Color.green);
            foreach (Edge e in rrt.edges)
                Debug.DrawLine(e.xe, e.xs, Color.green);

            // render the shortest path
            Vertex v = rrt.getShortestPath();//rrt.vertexes[RRTree.K/2];
            //Debug.Log($"rrt.shortestPathIndex = {rrt.shortestPathIndex}");
            int max_iters = RRTree.K;
            while (v.parent != null)
            {
                Debug.DrawLine(v.parent.value, v.value, Color.red);
                v = v.parent;
                max_iters--;
                if (max_iters == 0)
                    return;
            }
        }

        //tree rendering
    }
    static Vector3 sphere_scale = new Vector3(2.1F, 2.1F, 2.1F);
}
