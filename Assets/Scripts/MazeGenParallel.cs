// Kvazikot 


using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;


public partial class MazeGenParallel : MonoBehaviour
{
    /*
    public (float, float)[] vertexes_d;
    public (int, int)[] edges_d;
    public bool[] visibility_d;
    public bool[] visited;
    public Dictionary<string, int> edge_map;
    */
    public bool canDraw;
    static (float, float)[] vertexes_d = new (float, float)[] { (-30f, 30f), (0,30), (30,30), (-30,10),
                                              (0, 10), (30,10), (-30,-8), (0,-8), (30,-8)};

    static (int, int)[] edges_d = new (int, int)[] { (0,1), (1,2), (3,4), (4,5), (6,7), (7,8),
                                                     (0,3), (1,4), (2,5), (3,6), (4,7), (5,8)};

    static bool[] visibility_d = new bool[] { true, true, true, true, true, true, true, true, true, true, true, true, true };

    static bool[] visited = new bool[] { false, false, false, false, false, false, false, false, false };
    // TODO: change this on Dictionary<int, List<int>> 
    static Dictionary<string, int> edge_map = new Dictionary<string, int>()
    {
        { "0-1", 0 }, {"1-2", 1}, {"3-4", 2}, {"4-5", 3}, {"6-7", 4}, {"7-8", 5},
         {"3-0", 6}, {"1-4", 7}, {"2-5", 8}, {"3-6", 9}, {"4-7",10}, {"5-8",11}
    };


    const int NOT_VALID_ID = -1;

    public struct CreateMazeJob : IJob
    {
        //public NativeArray<(float,float)> vertexes;
        //public NativeArray<(int,int)> edges;
        //public NativeArray<bool> visibility;
        public int cellsX;
        public int cellsY;

        public void Execute()
        {
            //vertexes.CopyFrom(vertexes_d);
            //edges.CopyFrom(edges_d);
            //visibility.CopyFrom(visibility_d);
            //BuildMaze();
            /*
                        // Test AddNeibours
                        for (int i = 0; i < 8; i++)
                        {
                            AddEdgeNeibours(i, ref neibours);
                            string values = " ";
                            foreach (var item in neibours)
                                values += $"{item}, ";
                            Debug.Log($"{edges_d[i].Item1}-{edges_d[i].Item2} neibours: " + values);
                            neibours.Clear();
                        }
            */

        }



    }

    public void CreateGrid()
    {
        float grid_step = 30F;
        (float, float) v00 = vertexes_d[0];
        Vector3 v0 = new Vector3(v00.Item1, 5, v00.Item2);
        Vector3 v = new Vector3(v00.Item1, 5, v00.Item2);
        int counter = 0;
        int x_counter = 1;

        canDraw = false;
        //fill vertices
        while (counter != (cellsX * cellsY * 2 + 1))
        {
            vertexes_d[counter] = (v.x, v.z);
            v += new Vector3(grid_step, 0, 0);
            if (x_counter == (cellsX + 1))
            {
                v.x = v0.x;
                v.z -= grid_step;
                x_counter = 1;
            }
            else
                x_counter++;
            counter++;
            //Debug.Log("counter xcounter " + counter + " " + x_counter);
        }

        // horizontal edges
        int e_counter = 0;
        counter = 0;
        x_counter = 0;
        while ((e_counter != (cellsX + 1) * 2)  )
        {
            if (x_counter == cellsX )
            {
                x_counter = 0;
                counter++;
            }
            else
            {
                x_counter++;
                edges_d[e_counter] = (counter, counter + 1);
                counter++; e_counter++;
            }
        }

        //vertical edges
        e_counter = (cellsX + 1) * 2;
        counter = 0;
        x_counter = 0;
        while ((e_counter  != (cellsX + 1) * 4))
        {
             x_counter++;
             edges_d[e_counter] = (counter, counter + cellsX + 1);
             counter++; e_counter++;
        }
        canDraw = true;
        return;
      
    }

    public void BuildMaze()
    {
        List<int> wall_list = new List<int>();
        HashSet<int> neibours = new HashSet<int>();
        var rand = new System.Random();
        int rand_idx = getRandomCell();
        ResetMe();
        CreateGrid();
        return;
        visited[rand_idx] = true;
        AddCellWalls(rand_idx, ref wall_list);
        int iters = 0;
        while (wall_list.Count != 0)
        {
            int n;
            n = rand.Next(0, wall_list.Count);
            //if ( n >= wall_list.Count) n--;
            int edge_id = wall_list[n];
            int cell_id = Math.Min(edges_d[edge_id].Item1, edges_d[edge_id].Item2);
            int cell_id2 = getNeibourCell(edge_id);

            if (visited[cell_id] && !visited[cell_id2])
            {
                visibility_d[edge_id] = false;
                AddEdgeNeibours(edge_id, ref neibours);
                foreach (var id in neibours)
                    wall_list.Add(id);
                neibours.Clear();
                visited[cell_id2] = true;
            }

            if (visited[cell_id2] && !visited[cell_id])
            {
                visibility_d[edge_id] = false;
                AddEdgeNeibours(edge_id, ref neibours);
                foreach (var id in neibours)
                    wall_list.Add(id);
                neibours.Clear();
                visited[cell_id] = true;
            }

            iters++;
            wall_list.RemoveAt(n);
            if (iters > 10000) break;
        }
        Debug.Log("iters " + iters);
    }

    int getRandomCell()
    {
        var rand = new System.Random();
        int i = rand.Next(0, cellsX - 1);
        int j = rand.Next(0, cellsY - 1);
        return j * cellsX + i;
    }

    void ResetMe()
    {
        edge_map.Clear();
        for (int i = 0; i < vertexes_d.Length; i++)
            vertexes_d[i] = (0, 0);
        for (int i = 0; i < edges_d.Length; i++)
            edges_d[i] = (0,0);
        for (int i = 0; i < visibility_d.Length; i++)
            visibility_d[i] = true;
        for (int i = 0; i < visited.Length; i++)
            visited[i] = false;
    }

    int getNeibourCell(int edge_id)
    {
        //horizontal or vertical?
        int diff = Math.Abs(edges_d[edge_id].Item1 - edges_d[edge_id].Item2);
        int cell_id = Math.Min(edges_d[edge_id].Item1, edges_d[edge_id].Item2);
        if (diff == 1)
            return cell_id + 1;
        else
            return cell_id + cellsX + 1;
    }

    int AddCellWalls(int vertex, ref List<int> wall_list)
    {
        int i1 = vertex;
        int i2 = vertex + 1;
        int i3 = i1 + cellsX + 1;
        int i4 = i1 + cellsX + 2;
        int id1 = edge_id(i1, i2);
        int id2 = edge_id(i2, i4);
        int id3 = edge_id(i3, i4);
        int id4 = edge_id(i1, i3);
        int n = wall_list.Count;
        wall_list.Add(id1);
        wall_list.Add(id2);
        wall_list.Add(id3);
        wall_list.Add(id4);
        return wall_list.Count - n;
    }

    int edge_id(int Item1, int Item2)
    {
        int id;
        string key = $"{Item1}-{Item2}";
        if (edge_map.TryGetValue(key, out id))
            return id;
        key = $"{Item2}-{Item1}";
        if (edge_map.TryGetValue(key, out id))
            return id;
        return NOT_VALID_ID;
    }

    bool edges_equal((int, int) edge1, (int, int) edge2)
    {
        return ((edge1.Item1 == edge2.Item1) && (edge2.Item1 == edge2.Item2))
            || ((edge1.Item1 == edge2.Item2) && (edge1.Item2 == edge2.Item2));
    }


    int AddVertexNeibours(int vertex_idx, (int, int) edge, ref HashSet<int> wall_list)
    {
        int grid_period = cellsX + 1;
        int id = edge_id(vertex_idx, vertex_idx + 1);
        wall_list.Add(id);
        id = edge_id(vertex_idx, vertex_idx - 1);
        wall_list.Add(id);
        id = edge_id(vertex_idx + grid_period, vertex_idx);
        wall_list.Add(id);
        id = edge_id(vertex_idx - grid_period, vertex_idx);
        wall_list.Add(id);
        id = edge_id(edge.Item1, edge.Item2);
        wall_list.Remove(id);
        wall_list.Remove(NOT_VALID_ID);
        return 0;
    }

    int AddEdgeNeibours(int edge_idx, ref HashSet<int> wall_list)
    {
        (int, int) edge = edges_d[edge_idx];
        AddVertexNeibours(edge.Item1, edge, ref wall_list);
        AddVertexNeibours(edge.Item2, edge, ref wall_list);
        //wall_list.Remove((edge.Item2, edge.Item1));
        return 0;
    }

    public void DrawGrapth((float,float)[] vertexes, (int,int)[] edges, ref bool[] visibility)
    {
       
        int n = 0;
        foreach (var e in edges)
        {
              float x1 = vertexes[e.Item1].Item1;
              float y1 = vertexes[e.Item1].Item2;
              float x2 = vertexes[e.Item2].Item1;
              float y2 = vertexes[e.Item2].Item2;

              if (visibility[n])
                Gizmos.DrawLine(new Vector3(x1, 5, y1), new Vector3(x2, 5, y2));
            n++;
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
      

    }

    public void RegenerateMaze()
    {
        /*
        vertexes_d = new (float, float)[cellsX * cellsY + 1];
        edges_d = new (int, int)[cellsX * cellsY * 3];
        visibility_d = new bool[cellsX * cellsY * 3];
        visited = new bool[cellsX * cellsY + 1];
        // creating vertex and edges arrays based on grid topology
        edge_map = new Dictionary<string, int>();
        */

        //NativeArray<Cell1> result = new NativeArray<Cell1>((cellsX+2) * (cellsY+2) + 2, Allocator.TempJob);
        maze_job = new CreateMazeJob
        {
            //vertexes = new NativeArray<(float, float)>(vertexes_d.Length, Allocator.Persistent),
            //edges = new NativeArray<(int, int)>(edges_d.Length, Allocator.Persistent),
            //visibility = new NativeArray<bool>(visibility_d.Length, Allocator.Persistent),
            cellsX = cellsX,
            cellsY = cellsY
        };
        JobHandle firstHandle = maze_job.Schedule();
        firstHandle.Complete();
        //DrawCells1(result);
        BuildMaze();
        //maze_job.visibility.Dispose();

        bMazeGenerated = true;


    }

    private void OnDrawGizmos()
    {
        if (!canDraw) return;
        foreach (var v in vertexes_d)
            Gizmos.DrawSphere(new Vector3(v.Item1, 5, v.Item2), 1);

        DrawGrapth(vertexes_d, edges_d, ref visibility_d);
    }


    // Update is called once per frame
    void Update()
    {

        
    }

    public int cellsX;
    public int cellsY;
    CreateMazeJob maze_job;
    float wall_width;
    float wall_height;
    float size;
    public bool bMazeGenerated = false;
    //This declared because of note in documentation on function CreatePrimitive 
    private MeshFilter mesh_filter;



}


