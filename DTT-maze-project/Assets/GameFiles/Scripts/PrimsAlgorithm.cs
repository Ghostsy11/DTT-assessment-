using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PrimsAlgorithm : Maze
{

    [Header("Maze Animation Settings")]
    [Tooltip("Delay between each cell step in seconds")]
    public float generationStepDelay = 0.01f;

    [Tooltip("Time it takes for a corridor cube to fade out")]
    public float fadeOutDuration = 0.5f;

    public bool mazeAnimation = false;

    private void Start()
    {
        BuildMaze();
    }
    public override void Generate()
    {
        if (!mazeAnimation)
        {

            int x = 2;
            int z = 2;

            map[x, z] = 0;
            List<MapLocation> walls = new List<MapLocation>();
            walls.Add(new MapLocation(x + 1, z));
            Debug.Log(map[x, z]);
            walls.Add(new MapLocation(x - 1, z));
            Debug.Log(map[x, z]);
            walls.Add(new MapLocation(x, z + 1));
            Debug.Log(map[x, z]);
            walls.Add(new MapLocation(x, z - 1));
            Debug.Log(map[x, z]);

            // Loop counter to prevent infint loop
            int countLoop = 0;

            while (walls.Count > 0 && countLoop < 5000)
            {

                int rWalls = Random.Range(0, walls.Count);
                // picked up a wall
                x = walls[rWalls].x;
                z = walls[rWalls].z;

                walls.RemoveAt(rWalls);

                if (CountCellNeighbours(x, z) == 1)
                {
                    Debug.Log(map[x, z]);
                    map[x, z] = 0;
                    walls.Add(new MapLocation(x + 1, z));
                    walls.Add(new MapLocation(x - 1, z));
                    walls.Add(new MapLocation(x, z + 1));
                    walls.Add(new MapLocation(x, z - 1));
                }


                countLoop++;
            }

            PrintMap();
        }
        else
        {
            StartCoroutine(GenerateAnimated());
        }
    }

    IEnumerator GenerateAnimated()
    {
        int x = 2;
        int z = 2;

        map[x, z] = 0;
        yield return PaintCell(new Vector2Int(x, z), Color.black);

        List<MapLocation> walls = new List<MapLocation>
    {
        new MapLocation(x + 1, z),
        new MapLocation(x - 1, z),
        new MapLocation(x, z + 1),
        new MapLocation(x, z - 1)
    };

        int countLoop = 0;

        while (walls.Count > 0 && countLoop < 5000)
        {
            int rWalls = Random.Range(0, walls.Count);
            x = walls[rWalls].x;
            z = walls[rWalls].z;
            walls.RemoveAt(rWalls);

            Vector2Int current = new Vector2Int(x, z);

            // Highlight current wall cell being evaluated
            yield return PaintCell(current, Color.black);

            if (CountCellNeighbours(x, z) == 1)
            {
                map[x, z] = 0;
                yield return PaintCell(current, Color.white); // Mark as carved corridor

                List<MapLocation> newWalls = new List<MapLocation>
            {
                new MapLocation(x + 1, z),
                new MapLocation(x - 1, z),
                new MapLocation(x, z + 1),
                new MapLocation(x, z - 1)
            };

                foreach (var wall in newWalls)
                {
                    Vector2Int wallPos = new Vector2Int(wall.x, wall.z);
                    if (!walls.Exists(w => w.x == wall.x && w.z == wall.z) &&
                        IsInBounds(wall.x, wall.z) && map[wall.x, wall.z] == 1)
                    {
                        walls.Add(wall);
                        yield return PaintCell(wallPos, Color.yellow);
                    }
                }
            }
            else
            {
                yield return PaintCell(current, Color.red); // Dead end or already visited
            }

            countLoop++;
            yield return new WaitForSeconds(generationStepDelay); // Control speed of animation
        }

        // Smoothly fade out corridor cubes
        foreach (var kvp in cellObjects)
        {
            Vector2Int pos = kvp.Key;
            if (map[pos.x, pos.y] == 0)
            {
                StartCoroutine(Yanimation(kvp.Value, 5f));
            }
        }


        yield return null;
    }

    IEnumerator PaintCell(Vector2Int pos, Color color)
    {
        if (cellObjects.TryGetValue(pos, out GameObject obj))
        {
            obj.GetComponent<Renderer>().material.color = color;
        }
        yield return null;
    }

    bool IsInBounds(int x, int z)
    {
        return x > 0 && z > 0 && x < xCellsAmount - 1 && z < zCellsAmount - 1;
    }

    IEnumerator FadeOutAndDestroy(GameObject obj, float time)
    {
        Vector3 startScale = obj.transform.localScale;
        Color startColor = obj.GetComponent<Renderer>().material.color;
        float elapsed = 0f;

        while (elapsed < time)
        {
            float t = elapsed / time;

            // Shrink
            obj.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);

            // Fade color (if material supports transparency)
            Color faded = startColor;
            faded.a = Mathf.Lerp(startColor.a, 0f, t);
            obj.GetComponent<Renderer>().material.color = faded;

            elapsed += Time.deltaTime;
            yield return null;
        }

        obj.SetActive(false);
    }

    IEnumerator Yanimation(GameObject obj, float time)
    {
        Vector3 startPos = obj.transform.position;
        Vector3 endPos = new Vector3(startPos.x, startPos.y + 10f, startPos.z);

        float elapsed = 0f;

        while (elapsed < time)
        {
            float t = elapsed / time;

            // Move upward smoothly
            obj.transform.position = Vector3.Lerp(startPos, endPos, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        obj.transform.position = endPos;

        // Now fade out
        yield return StartCoroutine(FadeOutAndDestroy(obj, fadeOutDuration));

    }

    public void PrintMap()
    {
        for (int z = 0; z < zCellsAmount; z++)
        {
            string row = "";
            for (int x = 0; x < xCellsAmount; x++)
            {
                row += map[x, z] == 1 ? "#" : " ";
            }
            Debug.Log(row);
        }
    }
}
