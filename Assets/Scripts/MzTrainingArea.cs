using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MzTrainingArea : MonoBehaviour
{
	MazeGenParallel maze_gen;
    RrtPlaner rrt_planer;
    public Transform maze;
    public Transform CubeAgent;
    public float max_coord;
    // Start is called before the first frame update
    public  void Start()
    {
        max_coord = 2.5F;
        maze_gen = maze.GetComponent<MazeGenParallel>();
        rrt_planer = GetComponent<RrtPlaner>();
     

    }

    public  void Reset(Transform agent)
    {
        //pick the random cell
        int i = Random.Range(0, maze_gen.cellsX);
        int j = Random.Range(0, maze_gen.cellsY);
        //Vector3 c = maze_gen.getCellCenter(j * maze_gen.cellsX + i);
        //agent.position = c;
        int n = maze_gen.transform.childCount;
       // for (int ki = 0; ki < n; ki++)
       //     SafeDestroy.SafeDestroyGameObject(maze_gen.transform.GetChild(0));

    }

    // Update is called once per frame
    void Update()
    {
        //Reset();
    }
}
