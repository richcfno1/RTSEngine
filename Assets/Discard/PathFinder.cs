using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFinder : MonoBehaviour
{
    public struct Node
    {
        public int x;
        public int y;
        public int z;
    }

    public struct NodeWithPriority
    {
        public Node node;
        public float priority;
    }

    public class NodeComparer : IComparer<NodeWithPriority>
    {
        public int Compare(NodeWithPriority x, NodeWithPriority y)
        {
            return Comparer<float>.Default.Compare(x.priority, y.priority);
        }
    }

    public float minX;
    public float maxX;
    public float minY;
    public float maxY;
    public float minZ;
    public float maxZ;
    public float gridRadius;
    public float speed;
    public List<GameObject> others;

    private float rangeX;
    private float rangeY;
    private float rangeZ;
    private int gridSizeX;
    private int gridSizeY;
    private int gridSizeZ;

    private Vector3 finalDestination;
    private List<Vector3> moveBeacons = new List<Vector3>();
    

    // Start is called before the first frame update
    void Start()
    {
        rangeX = maxX - minX;
        rangeY = maxY - minY;
        rangeZ = maxZ - minZ;
        gridSizeX = Mathf.RoundToInt(rangeX / gridRadius + 1);
        gridSizeY = Mathf.RoundToInt(rangeY / gridRadius + 1);
        gridSizeZ = Mathf.RoundToInt(rangeZ / gridRadius + 1);

        finalDestination = new Vector3(25, 25, 25);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            moveBeacons.Clear();
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            FindPath(finalDestination);
        }
        if (moveBeacons.Count != 0)
        {
            RaycastHit hit;
            Ray ray = new Ray(finalDestination, (transform.position - finalDestination).normalized);
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider == null || hit.transform == transform)
                {
                    float moveDistance = speed * Time.deltaTime;
                    Vector3 moveDirection = finalDestination - transform.position;
                    if (moveDirection.magnitude <= moveDistance)
                    {
                        transform.position = finalDestination;
                        moveBeacons.Clear();
                    }
                    else
                    {
                        Collider[] intersectObjects = Physics.OverlapSphere(moveBeacons[0], gridRadius);
                        if (intersectObjects.Length != 0)
                        {
                            FindPath(finalDestination);
                        }
                        else
                        {
                            transform.position += moveDirection.normalized * moveDistance;
                        }
                    }
                }
                else
                {
                    float moveDistance = speed * Time.deltaTime;
                    Vector3 moveDirection = moveBeacons[0] - transform.position;
                    if (moveDirection.magnitude <= moveDistance)
                    {
                        transform.position = moveBeacons[0];
                        moveBeacons.RemoveAt(0);
                    }
                    else
                    {
                        transform.position += moveDirection.normalized * moveDistance;
                    }
                }
            }
        }
        
    }

    private Node VectorToGrid(Vector3 vector)
    {
        if (vector.x < minX && vector.x > maxX && vector.y < minY && vector.y > maxY && vector.z < minZ && vector.z > maxZ)
        {
            Debug.LogError("Vector out of map!");
        }
        int gridX = Mathf.RoundToInt((vector.x - minX) / gridRadius);
        int gridY = Mathf.RoundToInt((vector.y - minY) / gridRadius);
        int gridZ = Mathf.RoundToInt((vector.z - minZ) / gridRadius);
        return new Node
        {
            x = gridX,
            y = gridY,
            z = gridZ
        };
    }

    private Vector3 GridToVector(int x, int y, int z)
    {
        return new Vector3(minX + x * gridRadius, minY + y * gridRadius, minZ + z * gridRadius);
    }

    private List<Node> GetNeighbors(Node currentNode)
    {
        List<Node> result = new List<Node>();
        for (int i = -1; i <= 1; i++)
        {
            int newX = currentNode.x + i;
            if (newX < 0 || newX >= gridSizeX)
            {
                continue;
            }
            for (int j = -1; j <= 1; j++)
            {
                int newY = currentNode.y + j;
                if (newY < 0 || newY >= gridSizeY)
                {
                    continue;
                }
                for (int k = -1; k <= 1; k++)
                {
                    int newZ = currentNode.z + k;
                    if (newZ < 0 || newZ >= gridSizeZ)
                    {
                        continue;
                    }
                    if (i == 0 && j == 0 && k == 0)
                    {
                        continue;
                    }
                    bool isNotCollide = true;
                    Collider[] intersectObjects = Physics.OverlapSphere(GridToVector(newX, newY, newZ), gridRadius);
                    foreach (Collider obj in intersectObjects)
                    {
                        if (others.Contains(obj.gameObject))
                        {
                            isNotCollide = false;
                            break;
                        }
                    }
                    if (isNotCollide)
                    {
                        result.Add(new Node
                        {
                            x = newX,
                            y = newY,
                            z = newZ
                        });
                    }
                }
            }
        }
        return result;
    }

    private void FindPath(Vector3 destination)
    {
        if (destination.x < minX && destination.x > maxX && destination.y < minY && destination.y > maxY && destination.z < minZ && destination.z > maxZ)
        {
            Debug.LogError("Destination out of map!");
        }
        Vector3 currentPosition = transform.position;
        if (currentPosition.x < minX && currentPosition.x > maxX && currentPosition.y < minY && currentPosition.y > maxY && currentPosition.z < minZ && currentPosition.z > maxZ)
        {
            Debug.LogError("Current position out of map!");
        }
        SortedSet<NodeWithPriority> toSearch = new SortedSet<NodeWithPriority>(new NodeComparer());
        Node start = VectorToGrid(currentPosition);
        Node end = VectorToGrid(destination);
        toSearch.Add(new NodeWithPriority
        {
            node = start,
            priority = 0
        });
        Dictionary<Node, Node> pathHistory = new Dictionary<Node, Node>();
        pathHistory.Add(start, new Node
        {
            x = int.MaxValue,
            y = int.MaxValue,
            z = int.MaxValue
        });
        Dictionary<Node, float> costHistory = new Dictionary<Node, float>();
        costHistory.Add(start, 0);
        Node nearsetResult = end;
        while (toSearch.Count != 0)
        {
            Node current = toSearch.Min.node;
            toSearch.Remove(toSearch.Min);
            nearsetResult = current;
            if (current.x == end.x && current.y == end.y && current.z == end.z)
            {
                break;
            }
            foreach (Node i in GetNeighbors(current))
            {
                float newCost = costHistory[current] + Mathf.Sqrt(Mathf.Pow(i.x - current.x, 2) + Mathf.Pow(i.y - current.y, 2) + Mathf.Pow(i.z - current.z, 2));
                if (!costHistory.ContainsKey(i))
                {
                    costHistory.Add(i, newCost);
                    toSearch.Add(new NodeWithPriority
                    {
                        node = i,
                        priority = Mathf.Sqrt(Mathf.Pow(i.x - end.x, 2) + Mathf.Pow(i.y - end.y, 2) + Mathf.Pow(i.z - end.z, 2))
                    });
                    if (pathHistory.ContainsKey(i))
                    {
                        pathHistory[i] = current;
                    }
                    else
                    {
                        pathHistory.Add(i, current);
                    }
                }
                else
                {
                    if (newCost < costHistory[i])
                    {
                        costHistory[i] = newCost;
                        toSearch.Add(new NodeWithPriority
                        {
                            node = i,
                            priority = Mathf.Sqrt(Mathf.Pow(i.x - end.x, 2) + Mathf.Pow(i.y - end.y, 2) + Mathf.Pow(i.z - end.z, 2))
                        });
                        if (pathHistory.ContainsKey(i))
                        {
                            pathHistory[i] = current;
                        }
                        else
                        {
                            pathHistory.Add(i, current);
                        }
                    }
                }
            }
        }
        if (!pathHistory.ContainsKey(end))
        {
            Debug.Log("Failed");
            end = nearsetResult;
        }
        List<Node> result = new List<Node>();
        while (true)
        {
            if (end.x == int.MaxValue)
            {
                break;
            }
            result.Add(end);
            end = pathHistory[end];
        }
        result.Reverse();
        string answer = "Path is ";
        foreach (Node i in result)
        {
            answer += GridToVector(i.x, i.y, i.z).ToString() + " ";
            moveBeacons.Add(GridToVector(i.x, i.y, i.z));
        }
        moveBeacons.RemoveAt(0);
        moveBeacons.Add(finalDestination);
    }
}
