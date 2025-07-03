using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Maze : MonoBehaviour
{
    [SerializeField] GameObject cubeCell;
    [Tooltip("This is the z length")]
    [Range(0, 250)]
    public int zCellsAmount;
    [Tooltip("This is the x length")]
    [Range(0, 250)]
    public int xCellsAmount;

    public byte[,] map; // using byte is efficient way to store the maze layout in memory. It's faster and smaller than using int or bool[,].

    public int scale = 6;

    public Dictionary<Vector2Int, GameObject> cellObjects = new Dictionary<Vector2Int, GameObject>();
    public Color defaultColorCel = Color.red;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        BuildMaze();
    }
    // To determine rather its wall or corridor
    private void initializeMap()
    {
        // intialize to prevent null reference check in other to alloacting the memory
        map = new byte[xCellsAmount, zCellsAmount];
        for (int z = 0; z < zCellsAmount; z++)
        {
            for (int x = 0; x < xCellsAmount; x++)
            {
                // Intializing all cels to once
                map[x, z] = 1; // 1 = wall 0 = corridor
            }
        }
    }

    // Get randon cooridors
    public virtual void Generate()
    {
        for (int z = 0; z < zCellsAmount; z++)
        {
            for (int x = 0; x < xCellsAmount; x++)
            {
                // Get cells which is set to one than based on randomies it will kept at one or zero
                int wallOrCorridor = Random.Range(0, 100);
                if (wallOrCorridor > 50)
                {
                    map[x, z] = 0;
                }
            }
        }
    }

    //  Generate foundation of cube cells
    private void DrawMap()
    {

        for (int z = 0; z < zCellsAmount; z++)
        {
            for (int x = 0; x < xCellsAmount; x++)
            {
                // Only if cells is has value of one go ahead and genrate a cell
                if (map[x, z] == 1)
                {
                    //  * by scale to push cells away from each other
                    Vector3 cellPosition = new Vector3(x * scale, 0, z * scale);

                    // Creating a cellCube
                    GameObject child = Instantiate(cubeCell, cellPosition, Quaternion.identity);

                    // resizing it to fit into each other
                    child.transform.localScale = new Vector3(scale, scale, scale);

                    // Making it child of the parent object to keep things clear in hierarchy
                    child.transform.SetParent(transform, true);

                    child.GetComponent<MeshRenderer>().material.color = defaultColorCel;

                    Vector2Int pos = new Vector2Int(x, z);

                    cellObjects[pos] = child;
                }
            }
        }
    }


    public int CountCellNeighbours(int x, int z)
    {
        int count = 0;

        if (x <= 0 || x >= xCellsAmount - 1 || z <= 0 || z >= zCellsAmount - 1) return 5;

        if (map[x - 1, z] == 0)
        {
            count++;
        }
        if (map[x + 1, z] == 0)
        {
            count++;
        }
        if (map[x, z - 1] == 0)
        {
            count++;
        }
        if (map[x, z + 1] == 0)
        {
            count++;
        }
        return count;
    }

    public int CountDiagonalNeighbours(int x, int z)
    {
        int count = 0;
        if (x <= 0 || x >= xCellsAmount - 1 || z <= 0 || z >= zCellsAmount - 1) return 5;

        if (map[x - 1, z + 1] == 0)
        {
            count++;
        }
        if (map[x + 1, z + 1] == 0)
        {
            count++;
        }
        if (map[x - 1, z - 1] == 0)
        {
            count++;
        }
        if (map[x + 1, z - 1] == 0)
        {
            count++;
        }

        return count;
    }


    public int CountAllNeighbours(int x, int z)
    {


        return CountCellNeighbours(x, z) + CountDiagonalNeighbours(x, z);
    }
    public void BuildMaze()
    {
        initializeMap();
        Generate();
        DrawMap();
    }

}


public class MapLocation
{
    public int x;
    public int z;

    public MapLocation(int _x, int _z)
    {
        x = _x;
        z = _z;
    }

}
