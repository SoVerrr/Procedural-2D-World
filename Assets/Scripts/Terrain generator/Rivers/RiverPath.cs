using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Procedural
{
    public static class RiverPath
    {


        private static int FindDistance(Vector3Int target, Vector3Int current)
        {
            int dstX = Mathf.Abs(current.x - target.x);
            int dstY = Mathf.Abs(current.y - target.y);

            if (dstX > dstY)
                return 14 * dstY + 10 * (dstX - dstY);
            return 14 * dstX + 10 * (dstY - dstX);
        }

        private static List<Vector3Int> GetNodeNeighbors(Vector3Int node)
        {
            List<Vector3Int> neighbors = new List<Vector3Int>();

            int x = node.x;
            int y = node.y;

            neighbors.Add(new Vector3Int(x - 1, y - 1, 0));
            neighbors.Add(new Vector3Int(x - 1, y, 0));
            neighbors.Add(new Vector3Int(x - 1, y + 1, 0));
            neighbors.Add(new Vector3Int(x, y - 1, 0));
            neighbors.Add(new Vector3Int(x, y + 1, 0));
            neighbors.Add(new Vector3Int(x + 1, y - 1, 0));
            neighbors.Add(new Vector3Int(x + 1, y, 0));
            neighbors.Add(new Vector3Int(x + 1, y + 1, 0));

            return neighbors;
        }

        public static List<Vector3Int> AStarPathfind(Vector3Int startNode, Vector3Int targetNode, int offsetRange = 0)
        {
            List<Vector3Int> openSet = new List<Vector3Int>();
            List<Vector3Int> closedSet = new List<Vector3Int>();

            openSet.Add(startNode);

            while (openSet.Count > 0)
            {
                Vector3Int currentNode = openSet[0];
                for (int i = 1; i < openSet.Count; i++) //Select the node with lowest FCost or equal FCost and lower HCost
                {
                    int currentGCost = FindDistance(currentNode, startNode); // GCost - distance from current node to start node
                    int currentHCost = FindDistance(currentNode, targetNode); // HCost - distance from current node to target node
                    int currentFCost = currentGCost + currentHCost; // FCost - sum of G and H costs

                    int openGCost = FindDistance(openSet[i], startNode);
                    int openHCost = FindDistance(openSet[i], targetNode);
                    int openFCost = openGCost + openHCost;

                    if (openFCost < currentFCost || openFCost == currentFCost && openHCost < currentHCost)
                    {
                        currentNode = openSet[i];
                    }
                }

                openSet.Remove(currentNode);
                closedSet.Add(currentNode);

                if (currentNode == targetNode) //If current node is target return because path was found
                    return closedSet;

                List<Vector3Int> neighbors = GetNodeNeighbors(currentNode);
                foreach (Vector3Int neighbor in neighbors)
                {
                    if (closedSet.Contains(neighbor)) //Skip neighbors that have already been analyzed or are non walkable
                        continue;
                    openSet.Add(neighbor);
                }
            }
            return new List<Vector3Int>();
        }

    }
}