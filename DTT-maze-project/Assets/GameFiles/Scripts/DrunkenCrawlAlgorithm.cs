using UnityEngine;

public class DrunkenCrawlAlgorithm : Maze
{
    public override void Generate()
    {
        for (int i = 0; i < 2; i++)
        {
            CrawlV();

        }
        for (int i = 0; i < 3; i++)
        {

            CrawlH();
        }

    }

    private void CrawlH()
    {
        bool done = false;

        // Playing with starting value will give random generate on vertical axis
        int x = Random.Range(1, xCellsAmount - 1);
        int z = 1;

        // This will remove the center cell because we set it here to zero in data structure
        while (!done)
        {
            map[x, z] = 0;

            // To prevent going diagonale and get isolated
            if (Random.Range(0, 100) < 50)
            {
                // incrase and decrease to take out cells in random direactions between -1 - 0 and 1
                x += Random.Range(-1, 2);
            }
            else
            {
                // incrase and decrease to take out cells in random direactions between -1 - 0 and 1
                // When it goes outside of boundier we will have path
                z += Random.Range(0, 2);
            }
            done |= (x < 1 || x >= xCellsAmount - 1 || z < 1 || z >= zCellsAmount - 1);
            // |= is a bitwise OR assignment operator
            // since done is a bool, it works just like logical OR
            // (x < 0 || x >= xCellsAmount || z < 0 || z >= zCellsAmount)
            /*
             This checks whether the current position (x, z) is outside the maze boundaries:
            x < 0 → gone left past the edge
            x >= xCellsAmount → gone right past the edge
            z < 0 → gone backward past the edge
            z >= zCellsAmount → gone forward past the edge
             */
        }
    }

    private void CrawlV()
    {
        bool done = false;

        int x = 1;
        // Playing with starting value will give random generate on vertical axis
        int z = Random.Range(1, zCellsAmount - 1);

        // This will remove the center cell because we set it here to zero in data structure
        while (!done)
        {
            map[x, z] = 0;

            // To prevent going diagonale and get isolated
            if (Random.Range(0, 100) < 50)
            {
                // incrase and decrease to take out cells in random direactions between -1 - 0 and 1
                x += Random.Range(0, 2);
            }
            else
            {
                // incrase and decrease to take out cells in random direactions between -1 - 0 and 1
                // When it goes outside of boundier we will have path
                z += Random.Range(-1, 2);
            }
            done |= (x < 1 || x >= xCellsAmount - 1 || z < 1 || z >= zCellsAmount - 1);
        }
    }


}
