using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public int Size;
    public BoxCollider2D Panel;
    public GameObject token;
    private Node[,] NodeMatrix;
    private int startPosx, startPosy;
    private int endPosx, endPosy;
    private List<GameObject> nodeObjects = new List<GameObject>();

    void Awake()
    {
        Instance = this;
        Calculs.CalculateDistances(Panel, Size);
    }

    private void Start()
    {
        startPosx = Random.Range(0, Size);
        startPosy = Random.Range(0, Size);
        do
        {
            endPosx = Random.Range(0, Size);
            endPosy = Random.Range(0, Size);
        } while (endPosx == startPosx || endPosy == startPosy);

        NodeMatrix = new Node[Size, Size];
        CreateNodes();
        CalculateNodes();
    }

    public void CreateNodes()
    {
        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                NodeMatrix[i, j] = new Node(i, j, Calculs.CalculatePoint(i, j));
                NodeMatrix[i, j].Heuristic = Calculs.CalculateHeuristic(NodeMatrix[i, j], endPosx, endPosy);
            }
        }

        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                SetWays(NodeMatrix[i, j], i, j);
            }
        }

        DebugMatrix();
    }

    public void DebugMatrix()
    {
        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                GameObject gameObject = Instantiate(token, NodeMatrix[i, j].RealPosition, Quaternion.identity);
                nodeObjects.Add(gameObject);
                if (i == startPosx && j == startPosy)
                {
                    gameObject.GetComponent<SpriteRenderer>().color = Color.magenta;
                }
                else if (i == endPosx && j == endPosy)
                {
                    gameObject.GetComponent<SpriteRenderer>().color = Color.cyan;
                }
            }
        }
    }

    public void SetWays(Node node, int x, int y)
    {
        node.WayList = new List<Way>();
        if (x > 0)
        {
            node.WayList.Add(new Way(NodeMatrix[x - 1, y], Calculs.LinearDistance));
            if (y > 0)
            {
                node.WayList.Add(new Way(NodeMatrix[x - 1, y - 1], Calculs.DiagonalDistance));
            }
        }
        if (x < Size - 1)
        {
            node.WayList.Add(new Way(NodeMatrix[x + 1, y], Calculs.LinearDistance));
            if (y > 0)
            {
                node.WayList.Add(new Way(NodeMatrix[x + 1, y - 1], Calculs.DiagonalDistance));
            }
        }
        if (y > 0)
        {
            node.WayList.Add(new Way(NodeMatrix[x, y - 1], Calculs.LinearDistance));
        }
        if (y < Size - 1)
        {
            node.WayList.Add(new Way(NodeMatrix[x, y + 1], Calculs.LinearDistance));
            if (x > 0)
            {
                node.WayList.Add(new Way(NodeMatrix[x - 1, y + 1], Calculs.DiagonalDistance));
            }
            if (x < Size - 1)
            {
                node.WayList.Add(new Way(NodeMatrix[x + 1, y + 1], Calculs.DiagonalDistance));
            }
        }
    }

    public void CalculateNodes()
    {
        StartCoroutine(AStarCoroutine());
    }

    private IEnumerator AStarCoroutine()
    {
        List<Node> openList = new List<Node>();
        List<Node> closedList = new List<Node>();

        Node startNode = NodeMatrix[startPosx, startPosy];
        Node endNode = NodeMatrix[endPosx, endPosy];

        openList.Add(startNode);
        startNode.GCost = 0;
        startNode.Heuristic = Calculs.CalculateHeuristic(startNode, endPosx, endPosy);

        while (openList.Count > 0)
        {
            Node currentNode = openList[0];
            foreach (Node node in openList)
            {
                if (node.FCost < currentNode.FCost || (node.FCost == currentNode.FCost && node.Heuristic < currentNode.Heuristic))
                {
                    currentNode = node;
                }
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            if (currentNode != startNode && currentNode != endNode)
            {
                int index = currentNode.PositionX * Size + currentNode.PositionY;
                nodeObjects[index].GetComponent<SpriteRenderer>().color = Color.yellow;
                yield return new WaitForSeconds(0.5f);
            }

            if (currentNode == endNode)
            {
                RetracePath(startNode, endNode);
                yield break;
            }

            foreach (Way way in currentNode.WayList)
            {
                Node neighbor = way.NodeDestiny;

                if (closedList.Contains(neighbor))
                    continue;

                float tentativeGCost = currentNode.GCost + way.Cost;

                if (!openList.Contains(neighbor) || tentativeGCost < neighbor.GCost)
                {
                    neighbor.GCost = tentativeGCost;
                    neighbor.Heuristic = Calculs.CalculateHeuristic(neighbor, endPosx, endPosy);
                    neighbor.NodeParent = currentNode;

                    if (!openList.Contains(neighbor))
                        openList.Add(neighbor);
                }
            }
        }
    }

    private void RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.NodeParent;
        }
        path.Reverse();

        foreach (Node node in path)
        {
            if (node != startNode && node != endNode)
            {
                int index = node.PositionX * Size + node.PositionY;
                nodeObjects[index].GetComponent<SpriteRenderer>().color = Color.green;
            }
        }
    }
}
