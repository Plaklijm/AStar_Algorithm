using System.Collections.Generic;
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
        Dictionary<Vector2Int, Node> closed = new();
        
        while (open.Count > 0)
        {   
            open.Sort((a, b) => a.FScore.CompareTo(b.FScore));
            Node currentNode = open[0];
            open.RemoveAt(0);
            
            var gridPos = currentNode.position;
            closed.Add(currentNode.position, currentNode);
            
            if (currentNode.position == endPos)
            {
                // We have found the path so calculate the complete path and return it
                return ReconstructPath(closed, currentNode);
            }
            
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
                if (closed.ContainsKey(neighbour.Value.gridPosition))
                    continue;
                
                // Calculate the tentative GScore
                int tentativeGScore =
                    (int)(currentNode.GScore + Vector2Int.Distance(currentNode.position, neighbour.Value.gridPosition));
                
                // if the path is better than the previous one add it to the list
                if (open.Any(node => node.position == neighbour.Value.gridPosition))
                {
                    Node existingNode = open.Find(node => node.position == neighbour.Value.gridPosition);
                    if (tentativeGScore < existingNode.GScore)
                    {
                        open.Remove(existingNode);
                        Node updatedNode = new Node(neighbour.Value.gridPosition, currentNode, tentativeGScore, CalculateHScore(neighbour.Value.gridPosition, endPos));
                        open.Add(updatedNode);
                    }
                }
                else
                {
                    Node neighbourNode = new Node(neighbour.Value.gridPosition, currentNode,
                        CalculateGScore(currentNode, neighbour.Value.gridPosition),
                        CalculateHScore(neighbour.Value.gridPosition, endPos));
                    open.Add(neighbourNode);
                }
            }
        }

        // Open set is empty but goal was never reached
        return null;
    }
    
    // Helper functions

    private List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Node> tilesToCheck, Node goalNode)
    {
        List<Vector2Int> totalPath = new List<Vector2Int>();

        while (goalNode != null)
        {
            totalPath.Insert(0, goalNode.position);

            if (goalNode.parent != null && tilesToCheck.TryGetValue(goalNode.parent.position, out var parentNode))
                goalNode = parentNode;
            else
                break;
        }

        return totalPath;
    }

    private int CalculateHScore(Vector2Int gridPosition, Vector2Int endPos)
    {
        return (int)Vector2Int.Distance(gridPosition, endPos);
    }

    private int CalculateGScore(Node current, Vector2Int neighborPosition)
    {
        return (int)(current.GScore + Vector2Int.Distance(current.position, neighborPosition));
    }

    private Node GetLowestFScoreNode(Dictionary<Vector2Int, Node> nodes)
    {
        return nodes.Values.OrderBy( node => node.FScore ).First();
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
