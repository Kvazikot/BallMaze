// Kvazikot 


using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MazeGen : MonoBehaviour
{
    public enum WALL_TYPE
    {
        LEFT, RIGHT, TOP, BOTTOM
    };

    public class Wall
    {
        public WALL_TYPE type;
        public Color color;
        public Cell parent_cell;
        public Cell parent_cell2;
        List<Cell> neibours;
        public bool bVisible;
        public Wall()
        {
            parent_cell = null;
            parent_cell2 = null;
            color = Color.green;
        }
        public Wall(WALL_TYPE t, Cell parent)
        {
            type = t;
            parent_cell = parent;
            parent_cell2 = null;
            bVisible = true;
            color = Color.green;
        }
        public void SetCells(Cell c1, Cell c2)
        {
            parent_cell = c1;
            parent_cell2 = c2;
        }
        public bool isBorder() { return parent_cell2 == null; }

    }

    public class Cell : IEquatable<Cell>
    {
        public Cell()
        {
            leftWall = new Wall(WALL_TYPE.LEFT, this);
            rightWall = new Wall(WALL_TYPE.RIGHT, this);
            topWall = new Wall(WALL_TYPE.TOP, this);
            bottomWall = new Wall(WALL_TYPE.BOTTOM, this);
            bVisited = false;
            color = Color.white;
        }

        public Cell(int ii, int jj)
        {
            leftWall = new Wall(WALL_TYPE.LEFT, this);
            rightWall = new Wall(WALL_TYPE.RIGHT, this);
            topWall = new Wall(WALL_TYPE.TOP, this);
            bottomWall = new Wall(WALL_TYPE.BOTTOM, this);
            bVisited = false;
            color = Color.white;
        }
        public void setIJ(int ii, int jj) { i = ii; j = jj; }
        public bool isNull()
        {
            return this == null;
        }
        public bool Equals(Cell other)
        {
            if (other == null) return false;
            return (this.i == other.i) && (this.j == other.j);
        }
        public void addWalls(ref List<Wall> list)
        {
            list.Add(leftWall);
            list.Add(rightWall);
            list.Add(topWall);
            list.Add(bottomWall);
        }

        public void addTopWall(Cell c, ref List<Wall> list)
        {
            if (c != null) list.Add(c.topWall);
        }
        public void addBottomWall(Cell c, ref List<Wall> list)
        {
            if (c != null) list.Add(c.bottomWall);
        }
        public void addLeftWall(Cell c, ref List<Wall> list)
        {
            if (c != null) list.Add(c.leftWall);
        }
        public void addRightWall(Cell c, ref List<Wall> list)
        {
            if (c != null) list.Add(c.rightWall);
        }

        public void addNeiborWalls(WALL_TYPE t, ref List<Wall> list)
        {

            if (t == WALL_TYPE.LEFT)
            {
                addTopWall(leftWall.parent_cell, ref list);
                addBottomWall(leftWall.parent_cell, ref list);
                addTopWall(leftWall.parent_cell2, ref list);
                addBottomWall(leftWall.parent_cell2, ref list);
                addRightWall(topWall.parent_cell, ref list);
                addRightWall(bottomWall.parent_cell2, ref list);
            }
            if (t == WALL_TYPE.RIGHT)
            {
                addTopWall(rightWall.parent_cell, ref list);
                addBottomWall(rightWall.parent_cell, ref list);
                addTopWall(rightWall.parent_cell2, ref list);
                addBottomWall(rightWall.parent_cell2, ref list);
                addRightWall(topWall.parent_cell, ref list);
                addRightWall(bottomWall.parent_cell2, ref list);
            }
            if (t == WALL_TYPE.TOP)
            {
                addTopWall(leftWall.parent_cell, ref list);
                addTopWall(rightWall.parent_cell2, ref list);
                addLeftWall(topWall.parent_cell, ref list);
                addRightWall(topWall.parent_cell, ref list);
                addLeftWall(topWall.parent_cell2, ref list);
                addRightWall(topWall.parent_cell2, ref list);
            }
            if (t == WALL_TYPE.BOTTOM)
            {
                addBottomWall(leftWall.parent_cell, ref list);
                addBottomWall(rightWall.parent_cell2, ref list);
                addLeftWall(bottomWall.parent_cell, ref list);
                addRightWall(bottomWall.parent_cell, ref list);
                addLeftWall(bottomWall.parent_cell2, ref list);
                addRightWall(bottomWall.parent_cell2, ref list);
            }

            return;

        }

        public Wall leftWall;
        public Wall rightWall;
        public Wall topWall;
        public Wall bottomWall;
        public Color color;
        public int i{ get; set; }
        public int j { get; set; }
        public bool bVisited;
    }

   

    static Tuple<int, int> get_index(int i, int j)
    {
        return new Tuple<int, int>(i, j);
    }

    void CreateCells(int cellsX, int cellsY)
    {
        int cellsExtraX = cellsX + 2;
        int cellsExtraY = cellsY + 2;

        cells = new Dictionary<Tuple<int, int>, Cell>();

        for (int i = 0; i < cellsExtraX; i+=2 )
            for (int j = 0; j < cellsExtraY; j+=2 )
            {
                cells[get_index(i, j)] = new Cell(i, j);
            }

        //проставить смежность
        for (int i = 1; i < cellsExtraX - 1; i += 2)
            for (int j = 0; j < cellsExtraY; j += 2)
            {
                Cell c = new Cell(i, j); 
                c.leftWall = cells[get_index(i - 1, j)].rightWall;
                c.rightWall = cells[get_index(i + 1, j)].leftWall;
                c.topWall = new Wall(WALL_TYPE.TOP, c);
                c.bottomWall = new Wall(WALL_TYPE.BOTTOM, c);
                cells[get_index(i, j)] = c;
            }

        for (int i = 0; i < cellsExtraX; i += 2)
            for (int j = 1; j < cellsExtraY - 1; j += 2)
            {
                Cell c = new Cell(i, j);
                c.leftWall = new Wall(WALL_TYPE.LEFT, c);
                c.rightWall = new Wall(WALL_TYPE.RIGHT, c);
                c.topWall = cells[get_index(i, j + 1)].bottomWall;
                c.bottomWall = cells[get_index(i, j - 1)].topWall;
                cells[get_index(i, j)] = c;
            }


        for (int i = 1; i < cellsExtraX - 1; i += 2)
            for (int j = 1; j < cellsExtraY - 1; j += 2)
            {
                Cell c = new Cell(i, j);
                c.leftWall = cells[get_index(i - 1, j)].rightWall;
                c.rightWall = cells[get_index(i + 1, j)].leftWall;
                c.topWall = cells[get_index(i, j + 1)].bottomWall;
                c.bottomWall = cells[get_index(i, j - 1)].topWall;
                cells[get_index(i, j)] = c;
            }


        //return;
        // для каждой стены смежные ячейки
        for (int i = 0; i < cellsExtraX - 1; i++)
            for (int j = 0; j < cellsExtraY - 1; j++)
            {
                Cell c = cells[get_index(i, j)];
                if (i > 0)
                    c.leftWall.SetCells(cells[get_index(i - 1, j)],c);
                if (i < (cellsX - 2))
                    c.rightWall.SetCells(c, cells[get_index(i + 1, j)]);
                if (j > 0)
                    c.bottomWall.SetCells(cells[get_index(i, j - 1)],c);
                if (j < (cellsY - 2))
                    c.topWall.SetCells(c, cells[get_index(i, j + 1)]);
                cells[get_index(i, j)] = c;
            }




    }

    void colorParents(Wall wall, Color color1, Color color2)
    {
        wall.parent_cell.leftWall.color = color1;
        wall.parent_cell.rightWall.color = color1;
        wall.parent_cell.topWall.color = color1;
        wall.parent_cell.bottomWall.color = color1;
        wall.parent_cell2.leftWall.color = color2;
        wall.parent_cell2.rightWall.color = color2;
        wall.parent_cell2.topWall.color = color2;
        wall.parent_cell2.bottomWall.color = color2;

    }

    void Save(string filename)
    {
        string[] lines = new string[cells.Count+1];
        lines[0] = cellsX + " " + cellsY;
        int l = 1;
        for (int j = 0; j < cellsY; j++)
            for (int i=0; i < cellsX; i++)
            {
                var k = get_index(i, j);
                if (cells.ContainsKey(k))
                {
                    Cell c = cells[k];
                    lines[l] = c.topWall.bVisible + " " + c.rightWall.bVisible + " ";
                    lines[l] = lines[l] + c.bottomWall.bVisible + " " + c.leftWall.bVisible;
                    l++;
                }
            }

        // WriteAllLines creates a file, writes a collection of strings to the file,
        // and then closes the file.  You do NOT need to call Flush() or Close().
        System.IO.File.WriteAllLines(filename, lines);
    }

    void Load(string filename)
    {
        string[] lines = System.IO.File.ReadAllLines(filename);
        if (lines.Length > 1)
        {
            string[] header = lines[0].Split(' ');
            if (header.Length == 2)
            {
                if (Int32.TryParse(header[0], out int _cellsX))
                {
                    if (Int32.TryParse(header[1], out int _cellsY))
                        CreateCells(_cellsX, _cellsY);
                }
            }
            for(var l=1; l < lines.Length; l++)
            {
                string[] header2 = lines[l].Split(' ');
                if (header2.Length == 4)
                {
                    bool.TryParse(header2[0], out bool topWall);
                    bool.TryParse(header2[1], out bool rightWall);
                    bool.TryParse(header2[2], out bool bottomWall);
                    bool.TryParse(header2[3], out bool leftWall);
                    int i = (l - 1) % cellsX;
                    int j = (l - 1) / cellsX;
                    var k = get_index(i, j);
                    if (cells.ContainsKey(k))
                    {
                        cells[k].topWall.bVisible = topWall;
                        cells[k].rightWall.bVisible = rightWall;
                        cells[k].bottomWall.bVisible = bottomWall;
                        cells[k].leftWall.bVisible = leftWall;
                    }
                }
            }
        }

    }

    void CreateMaze()
    {
        var rand = new System.Random();
        wall_list = new List<Wall>();
    /*
        foreach (KeyValuePair<Tuple<int,int>, Cell> kvp in cells)
        {
            Cell c = kvp.Value;
            c.leftWall.color = Color.green;
            c.rightWall.color = Color.blue;
            c.topWall.color = Color.white;
            c.bottomWall.color = Color.cyan;

        }
        */

        //colorParents(cells[get_index(3, 3)].rightWall, Color.red, Color.blue);
        //colorParents(cells[get_index(3, 4)].leftWall, Color.red, Color.blue);
        //colorParents(cells[get_index(3, 3)].topWall, Color.red, Color.blue);
        //colorParents(cells[get_index(4, 3)].bottomWall, Color.red, Color.blue);

        //return;
        Cell rand_cell = new Cell(0,0);
        Tuple<int, int> idx = get_index(rand.Next(0, cellsX), rand.Next(0, cellsY));
        Debug.Log("Maze RANDOM starting index is " + idx.Item1 + " " + idx.Item2);
        cells[idx].bVisited = true;
        cells[idx].addWalls(ref wall_list);
        int iters = 0;
        while (wall_list.Count != 0)
        {
            int n;
            n = rand.Next(0, wall_list.Count);
            //if ( n >= wall_list.Count) n--;
            Wall wall = wall_list[n];
            //Debug.Log("visiting " + n + " wall");
            //wall_list.RemoveAt(wall_list.IndexOf(wall));
            //continue;

            if( wall.parent_cell2 != null )
            if ((wall.parent_cell.bVisited && !wall.parent_cell2.bVisited))
            {
                wall.bVisible = false;
                wall.parent_cell2.addNeiborWalls(wall.type, ref wall_list);
                //Debug.Log("1. wall_list.Count " + wall_list.Count);
                //copy(wall.neibours.begin(), wall.neibours.end(), std::back_inserter(wall_list));
                wall.parent_cell2.bVisited = true;
            }

            if ( wall.parent_cell2 != null )
            if ((!wall.parent_cell.bVisited && wall.parent_cell2.bVisited))
            {
                wall.bVisible = false;
                wall.parent_cell.addNeiborWalls(wall.type, ref wall_list);
                //Debug.Log("2. wall_list.Count " + wall_list.Count);
                //copy(wall.neibours.begin(), wall.neibours.end(), std::back_inserter(wall_list));
                wall.parent_cell.bVisited = true;
            }

            iters++;
            wall_list.RemoveAt(wall_list.IndexOf(wall));
        }
        Debug.Log("iters " + iters);

        //boundary walls
        for (int i = 0; i < cellsX; i++)
        {
            Cell c = cells[get_index(i, 0)];
            c.bottomWall.bVisible = true;
        }
        for (int i = 0; i < cellsY; i++)
        {
            Cell c = cells[get_index(cellsX, i)];
            c.leftWall.bVisible = true;
        }
        for (int i = 0; i < cellsX; i++)
        {
            Cell c = cells[get_index(i, cellsY)];
            c.bottomWall.bVisible = true;
        }

        
        //in
        Cell c_in = cells[get_index(0, 0)];
        c_in.bottomWall.bVisible = false;

        //out
        Cell c_out = cells[get_index(cellsX-1, cellsY)];
        c_out.bottomWall.bVisible = false;
        

    }
    
	static string tagWall = "wall";
	
    void CreateWallPrimitive(Vector3 position, Vector3 localScale, string name, Color color)
    {
        GameObject leftWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        leftWall.transform.position = position;
        leftWall.transform.localScale = localScale;
        leftWall.transform.rotation = Quaternion.AngleAxis(UnityEngine.Random.RandomRange(0, 0.1F), Vector3.up);
        leftWall.transform.parent = mesh_filter.transform;
        leftWall.transform.name = name;
        //var renderer = leftWall.GetComponent<MeshRenderer>();
        //renderer.material.SetColor("_Color", color);
        leftWall.layer = 8;
		leftWall.tag = tagWall;
    }

    void CreateSpherePrimitive(Vector3 position, string name, Color color)
    {
       //return;
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);

        if (waypoints != null)
            sphere.transform.parent = waypoints.transform;
        sphere.transform.position = position;
        sphere.transform.localScale = new Vector3(1,1,1);
        sphere.transform.rotation = Quaternion.AngleAxis(UnityEngine.Random.RandomRange(0, 0.1F), Vector3.up);
        //sphere.transform.parent = mesh_filter.transform;
        sphere.transform.name = name;
        var renderer = sphere.GetComponent<MeshRenderer>();
        renderer.material.SetColor("_Color", color);
        sphere.layer = 8;
    }

    float getSize()
    {
        return (transform.localScale.x * transform.localScale.x) * 4;
    }
    
    void DrawCells()
    {
        float maze_size = 10 * transform.localScale.x;
        float cell_size = maze_size / cellsX;
        size = maze_size;
        Debug.Log("cell_size " + cell_size);
        //Debug.Log("wall_width " + wall_width);
        Debug.Log("maze_size " + maze_size);        

        wall_width = cell_size * 0.3f;

        for (int i = 0; i < cellsX; i++)
        {
            for (int j = 0; j < cellsY; j++)
            {
                Tuple<int,int> idx = get_index(i, j);
                if( cells.ContainsKey(idx) )
                {
                    Cell cell = cells[idx];
                    float offseX =  maze_size / 2;
                    float offseZ =  maze_size / 2;
                    Vector3 C = new Vector3(transform.position.x + i * cell_size - offseX, 
                                            0,
                                            transform.position.z + (j+1)  * cell_size - offseZ);
                    Vector3 C2 = new Vector3(transform.position.x + i * cell_size - offseX + wall_width/2, 
                                             0,
                                             transform.position.z + (j ) * cell_size - offseZ - wall_width/2);

                    if (cell.leftWall.bVisible)
                    {
                        Vector3 position = new Vector3(C.x + wall_width / 2, 0, C.z - cell_size / 2);
                        Vector3 localScale = new Vector3(wall_width, wall_height, cell_size);
                        CreateWallPrimitive(position, localScale, "Wall " + Convert.ToString(j * cellsX + i), cell.leftWall.color);
                    }
                    if (cell.rightWall.bVisible)
                    {
                        Vector3 position = new Vector3(C.x + cell_size + wall_width / 2, 0, C.z - cell_size / 2);
                        Vector3 localScale = new Vector3(wall_width, wall_height, cell_size);
                        CreateWallPrimitive(position, localScale,  "Wall " + Convert.ToString(j * cellsX + i), cell.rightWall.color);
                    }

                    if (cell.topWall.bVisible)
                    {
                        Vector3 position = new Vector3(C.x + cell_size / 2 + wall_width / 2, 0, C.z - wall_width / 2);
                        Vector3 localScale = new Vector3(cell_size + wall_width, wall_height, wall_width);
                        CreateWallPrimitive(position, localScale, "top Wall " + Convert.ToString(j * cellsX + i), cell.topWall.color);

                    }
                    if (cell.bottomWall.bVisible)
                    {
                        Vector3 position = new Vector3(C.x + cell_size / 2 + wall_width/2,
                                                                  0,
                                                                  C.z - cell_size - wall_width / 2);
                        Vector3 localScale = new Vector3(cell_size + wall_width, wall_height, wall_width);
                        CreateWallPrimitive(position, localScale, "bottom Wall " + Convert.ToString(j * cellsX + i), cell.bottomWall.color);
                    }
                }

            }
        
        }

     
   }

    List<Cell> getNeibours(Cell cell)
    {
        List<Cell> neibours = new List<Cell>();
        if( cells.ContainsKey(get_index(cell.i + 1, cell.j)) )
            neibours.Add(cells[get_index(cell.i + 1, cell.j)]);
        if (cells.ContainsKey(get_index(cell.i - 1, cell.j)))
            neibours.Add(cells[get_index(cell.i - 1, cell.j)]);
        if (cells.ContainsKey(get_index(cell.i, cell.j + 1)))
            neibours.Add(cells[get_index(cell.i, cell.j + 1)]);
        if (cells.ContainsKey(get_index(cell.i, cell.j - 1)))
            neibours.Add(cells[get_index(cell.i, cell.j - 1)]);
        return neibours;
    }

    int getCellId(Cell c) { return c.j*cellsX + c.i; }

    public Vector3 getCellCenter(int id)
    {
        int i = id % cellsX;
        int j = id / cellsX;
        //float maze_size = transform.localScale.x * transform.localScale.x * 2;
        float maze_size = 10 * transform.localScale.x;
        float cell_size = maze_size / cellsX ;
        float offseX = maze_size / 2;
        float offseZ = maze_size / 2;
        Vector3 C = new Vector3(i * cell_size - offseX + cell_size/2, 0, (j) * cell_size - offseZ + cell_size / 2);
        return C;
    }
  
    private string String(int v)
    {
        throw new NotImplementedException();
    }

    void MovePlayerInCell(int i, int j)
    {
        GameObject player = GameObject.Find("Player");
        float maze_size = getSize();
        float offseX = maze_size / 2;
        float offseZ = maze_size / 2;
        float cell_size = maze_size / cellsX;
        Vector3 C = new Vector3(i * cell_size - offseX, 0, j * cell_size - offseZ);
        player.transform.position = C + new Vector3(cell_size / 2, 0, cell_size / 2);
    }

    // Test case cell 1,1 is closed 
    // check that out_angles array is zero size
    // check that paths array is zero size
    void TestAllWallsClosed()
    {
		foreach(var c in cells)
		{
			Cell c_test = c.Value;
			c_test.leftWall.bVisible = true;
			c_test.bottomWall.bVisible = true;
			c_test.topWall.bVisible = true;
			c_test.rightWall.bVisible = true;
			//Vector3 C = getCellCenter(c_test.j*cellsX + c_test.i);
			//CreateSpherePrimitive(C+new Vector3(cell_size/2, 0, cell_size/2), "center " + Convert.ToString(1 * cellsX + 1), Color.red);
		}
        //MovePlayerInCell(cellsX - 1, cellsY-1);
        

    }

    // Test case cell 1,1 right wall is opened other walls is closed
    // check that out_angles array has one element {90}
    // check that paths array has element {22}
    void TestRightWallOpened()
    {
        Cell c_test = cells[get_index(1, 1)];
        c_test.leftWall.bVisible = true;
        c_test.bottomWall.bVisible = true;
        c_test.topWall.bVisible = true;
        c_test.rightWall.bVisible = false;

        //MovePlayerInCell(1, 1);
    }

    // Test case cell 1,1 left wall is opened other walls is closed
    // check that out_angles array has one element {270}
    // check that paths array has element {20}
    void TestLeftWallOpened()
    {
        Cell c_test = cells[get_index(1, 1)];
        c_test.leftWall.bVisible = false;
        c_test.bottomWall.bVisible = true;
        c_test.topWall.bVisible = true;
        c_test.rightWall.bVisible = true;

        //MovePlayerInCell(1, 1);
    }

    // Test case cell 1,1 top wall is opened other walls is closed
    // check that out_angles array has one element {0}
    // check that paths array has element {41}
    void TestTopWallOpened()
    {
        Cell c_test = cells[get_index(1, 1)];
        c_test.leftWall.bVisible = true;
        c_test.bottomWall.bVisible = true;
        c_test.topWall.bVisible = false;
        c_test.rightWall.bVisible = true;

        MovePlayerInCell(1, 1);
    }

    // Test case cell 1,1 bottom wall is opened other walls is closed
    // check that out_angles array has one element {180}
    // check that paths array has element {1}
    void TestBottomWallOpened()
    {
        Cell c_test = cells[get_index(1, 1)];
        c_test.leftWall.bVisible = true;
        c_test.bottomWall.bVisible = false;
        c_test.topWall.bVisible = true;
        c_test.rightWall.bVisible = true;

        MovePlayerInCell(1, 1);
    }

    // Test case cell 63 
    // check that paths array has elements {83,43,62}
    void Test63()
    {
        //CreateCells(cellsX, cellsY);
        //CreateMaze();
        //Save(@"Maze1.txt");
        Load(@"Maze1.txt");
        int id = 63;
        MovePlayerInCell(id % cellsX, id / cellsX);
    }

    // Test case cell 371 
    // check that out_angles array has elements {180}
    // check that paths array has elements {163}
    void Test371()
    {
        Load(@"Maze1.txt");
        int id = 371;
        MovePlayerInCell(id % cellsX, id / cellsX);
    }

    void RunTests()
    {

        //TestRightWallOpened();
        //TestLeftWallOpened();
        //TestTopWallOpened();
        //TestBottomWallOpened();
        //Test63();
        //Test371();
        CreateCells(cellsX, cellsY);

        //CreateMaze();
        Load("Maze3.txt");
        //Save("Maze3.txt");
        //TestAllWallsClosed();
    }


    // Start is called before the first frame update
    void Start()
    {
        return;
        bMazeGenerated = false;
        mesh_filter = GetComponent<MeshFilter>();
        waypoints = GameObject.Find("Waypoints");

        wall_height = 2;

        RunTests();

        DrawCells();

       
        bMazeGenerated = true;


    }

    public void RegenerateMaze()
    {

        bMazeGenerated = false;
        mesh_filter = GetComponent<MeshFilter>();
        waypoints = GameObject.Find("Waypoints");

        wall_height = 2;

        CreateCells(cellsX, cellsY);

        CreateMaze();

        DrawCells();

       
        bMazeGenerated = true;


    }
   
   

    // Update is called once per frame
    void Update()
    {

        
      
        n_frame++;
    }

    ulong  n_frame=0;
    GameObject waypoints;
    public int cellsX;
    public int cellsY;
    float wall_width;
    float wall_height;
    float size;
    public bool bMazeGenerated = false;
    //This declared because of note in documentation on function CreatePrimitive 
    private MeshFilter mesh_filter;
    private Dictionary<Tuple<int, int>, Cell> cells;
    public List<Wall> wall_list;

}


