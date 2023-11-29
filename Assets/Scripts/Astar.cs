﻿using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Astar
{
    /// <summary>
    /// TODO: Implement this function so that it returns a list of Vector2Int positions which describes a path from the startPos to the endPos
    /// Note that you will probably need to add some helper functions
    /// </summary>
    /// <param name="startPos"></param>
    /// <param name="endPos"></param>
    /// <param name="grid"></param>
    /// <returns></returns>
    public List<Vector2Int> FindPathToTarget(Vector2Int startPos, Vector2Int endPos, Cell[,] grid)
    {
        var startNode = new Node(startPos, null, 0,  CalculateHScore(startPos, endPos));

        List<Node> open = new() { startNode };
        Dictionary<Vector2Int, Node> checkedNodes = new();
        
        while (open.Count > 0)
        {   
            Node currentNode = GetLowestFScoreNode(open);
            var gridPos = currentNode.position;
            
            if (currentNode.position == endPos)
            {
                // We have found the path so calculate the complete path and return it
                Debug.Log("found path");
                return ReconstructPath(checkedNodes, currentNode.position);
            }

            open.Remove(currentNode);
            
            // Get the neighbours of the current cell and put them into a dictionary (done so i could remove them easily)
            var temp = grid[gridPos.x, gridPos.y].GetNeighbours(grid);
            Dictionary<Vector2Int, Cell> neighbours = temp.ToDictionary(keySelector: cell => cell.gridPosition, elementSelector: cell => cell);
            
            // remove a neighbour if a wall is in the way
            if (grid[gridPos.x, gridPos.y].HasWall(Wall.LEFT))
                neighbours.Remove(gridPos + Vector2Int.left);
            if (grid[gridPos.x, gridPos.y].HasWall(Wall.RIGHT))
                neighbours.Remove(gridPos + Vector2Int.right);
            if (grid[gridPos.x, gridPos.y].HasWall(Wall.UP))
                neighbours.Remove(gridPos + Vector2Int.up);
            if (grid[gridPos.x, gridPos.y].HasWall(Wall.DOWN))
                neighbours.Remove(gridPos + Vector2Int.down);
            
            foreach (var neighbour in neighbours)
            {
                // Continue if the neighbour cell is already checked
                if (checkedNodes.ContainsKey(neighbour.Value.gridPosition))
                    continue;
                
                // Calculate the tentative GScore
                int tentativeGScore =
                    (int)(currentNode.GScore + Vector2Int.Distance(currentNode.position, neighbour.Value.gridPosition));
                
                // Create a node for the neighbour
                Node neighbourNode = new Node(neighbour.Value.gridPosition, null,
                    int.MaxValue, CalculateHScore(neighbour.Value.gridPosition, endPos));
                
                // if the path is better than the previous one add it to the list
                if (tentativeGScore < neighbourNode.GScore)
                {
                    checkedNodes.Add(neighbourNode.position, currentNode);
                    neighbourNode.GScore = tentativeGScore;
                    neighbourNode.parent = currentNode;
                    if (!open.Contains(neighbourNode))
                    {
                        open.Add(neighbourNode);
                    }
                }
            }
        }

        // Open set is empty but goal was never reached
        return null;
    }
    
    // Helper functions

    private List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Node> closed, Vector2Int current)
    {
        // create a stack (a list would also be okay but that needs to be reversed before you return it so this is neater)
        Stack<Vector2Int> path = new();
        path.Push(current);
        // keep track of the visited tiles so you dont visit one twice
        List<Vector2Int> visited = new List<Vector2Int> { current };

        while (closed.TryGetValue(current, out Node currentNode) && !visited.Contains(currentNode.position))
        {
            // set the current location to the new position from the list and add it to the path && visited
            current = currentNode.position;
            path.Push(current);
            visited.Add(current);
        }
        
        return path.ToList();
    }

    private int CalculateHScore(Vector2Int gridPosition, Vector2Int endPos)
    {
        return (int)Vector2Int.Distance(gridPosition, endPos);
    }

    private Node GetLowestFScoreNode(List<Node> nodes)
    {
        return nodes.OrderBy( node => node.FScore ).First();
    }
    
    /// <summary>
    /// This is the Node class you can use this class to store calculated FScores for the cells of the grid, you can leave this as it is
    /// </summary>
    public class Node
    {
        public Vector2Int position; //Position on the grid
        public Node parent; //Parent Node of this node

        public float FScore { //GScore + HScore
            get { return GScore + HScore; }
        }
        public float GScore; //Current Travelled Distance
        public float HScore; //Distance estimated based on Heuristic

        public Node() { }
        public Node(Vector2Int position, Node parent, int GScore, int HScore)
        {
            this.position = position;
            this.parent = parent;
            this.GScore = GScore;
            this.HScore = HScore;
        }
    }
}
