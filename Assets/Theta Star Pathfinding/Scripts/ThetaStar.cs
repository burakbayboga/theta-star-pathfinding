using System.Collections.Generic;
using UnityEngine;

// The main controller class. Call the FindPath method with start point and end points as parameters
// and get the shortest smooth path.
[RequireComponent(typeof(MapFiller))]
public class ThetaStar : MonoBehaviour
{
	public static ThetaStar instance;

	public GameObject processedNodeObject;

	// applies path smoothing when true (by removing unnecessary nodes)
	public bool smoothPath = true;

	// layer of non-walkable nodes (walls, obstacles etc.)
	public LayerMask wallLayermask;
	
	private MapFiller mapFiller;
	private ThetaStarNode[,] map;

	// The list of nodes that have had their costs calculated. The list is ordered by their costs.
	private PriorityQueue openList;

	// The list of nodes that have already been visited.
	private List<ThetaStarNode> closedList;

	// The list of nodes that are reachable from the currently analyzed node.
	private List<ThetaStarNode> neighbors;

	private Vector3 startPoint;
	private Vector3 endPoint;

	private ThetaStarNode endNode;
	private ThetaStarNode startNode;

	private void Awake()
	{
		// Make singleton to make access easier and remove the need for scene references.
		instance = this;
		mapFiller = GetComponent<MapFiller>();

		// Get the grid map from the helper class.
		map = mapFiller.GetMap();
	}

	// Call this method to get a path. Has 2 parameters.
	// Param1 -> _startPoint (Vector3): world space position of the starting point
	// Param2 -> _endPoint (Vector3): world space position of the end point
	// returns: A list of type Vector3. World space positions of the path. Follow these positions
	// in order to get to the end point from the start point. 
	public List<Vector3> FindPath(Vector3 _startPoint, Vector3 _endPoint)
	{
		startPoint = _startPoint;
		endPoint = _endPoint;

		// get the grid coordinates for the given start and end points
		if (!GetMapCoordsForPosition(startPoint, out Vector2Int startCoords))
		{
			Debug.LogError("No valid grid node found for the start point - please check that start point is not deep inside unwalkable objects");
			// Invalid start point, either outside of map or deep inside unwalkable objects
			return new List<Vector3>{ startPoint };
		}
		if (!GetMapCoordsForPosition(endPoint, out Vector2Int endCoords))
		{
			Debug.LogError("No valid grid node found for the end point - please check that end point is not deep inside unwalkable objects");
			// Invalid end point, either outside of map or deep inside unwalkable objects
			return new List<Vector3>{ startPoint };
		}

		// Initialize the lists for the algorithm
		openList = new PriorityQueue();
		closedList = new List<ThetaStarNode>();
		neighbors = new List<ThetaStarNode>();

		// Initialize start and end nodes
		startNode = map[startCoords.x, startCoords.y];
		endNode = map[endCoords.x, endCoords.y];
		endNode.gCost = 0f;
		endNode.hCost = (endNode.mapPos - startNode.mapPos).magnitude;
		endNode.parent = endNode;
		openList.Enqueue(endNode);

		while (!openList.IsEmpty())
		{
			ThetaStarNode currentNode = openList.Dequeue();
			closedList.Add(currentNode);
			//Instantiate(processedNodeObject, mapFiller.MapToWorld(currentNode.mapPos), Quaternion.identity);

			if (currentNode == startNode)
			{
				// Shortest path found. Generate the path and exit the function.
				return ExtractPath(currentNode, endNode);
			}

			// Update the neighbors list for the currently analyzed node
			UpdateNeighbors(currentNode);

			for (int i = 0; i < neighbors.Count; i++)
			{
				ThetaStarNode neighborNode = neighbors[i];
				if (!closedList.Contains(neighborNode))
				{
					if (!openList.Contains(neighborNode))
					{
						neighborNode.gCost = float.MaxValue;
						neighborNode.parent = null;
					}
					ProcessNode(currentNode, neighborNode);
				}
			}
		}

		// If the target is unreachable and a path does not exist, return the original start point.
		return new List<Vector3>{ startPoint };
	}

	// Generate the final path from the processed nodes.
	// Returns a list of type Vector3, which correspond the world space positions.
	private List<Vector3> ExtractPath(ThetaStarNode currentNode, ThetaStarNode endNode)
	{
		List<Vector3> path = new List<Vector3>();
		while (currentNode != endNode)
		{
			path.Add(mapFiller.MapToWorld(currentNode.mapPos));
			currentNode = currentNode.parent;
		}
		// add the final node
		path.Add(mapFiller.MapToWorld(currentNode.mapPos));

		// Add the original target position for increased accuracy, which might slightly offset from the final node.
		path.Add(endPoint);

		// Check if the first node of the path can be skipped, usually as a result of the first 
		// 2 nodes being in opposite directions.
		if (path.Count > 1)
		{
			if (!Physics.Linecast(startPoint, path[1], wallLayermask))
			{
				path.RemoveAt(0);
			}
		}

		// Check if the second-to-last node of the path can be skipped, usually as a result overshooting
		// the original target position.
		if (path.Count > 1)
		{
			if (!Physics.Linecast(endPoint, path[path.Count - 2], wallLayermask))
			{
				path.RemoveAt(path.Count - 2);
			}
		}

		return path;
	}

	// Calculate the cost of a given neighbor node and add it to the open list to check later
	private void ProcessNode(ThetaStarNode currentNode, ThetaStarNode neighborNode)
	{
		//Instantiate(processedNodeObject, mapFiller.MapToWorld(neighborNode.mapPos), Quaternion.identity);
		float oldCost = neighborNode.fCost;
		CalculateCost(currentNode, neighborNode);
		if (neighborNode.fCost < oldCost)
		{
			if (openList.Contains(neighborNode))
			{
				openList.Remove(neighborNode);
			}
			openList.Enqueue(neighborNode);
		}
	}

	// Calculates the "cost" of a given node.
	// Apply path smoothing if there is line of sight between nodes.
	private void CalculateCost(ThetaStarNode currentNode, ThetaStarNode neighborNode)
	{
		// get world space positions of nodes to check line of sight
		Vector3 currentParentPos = mapFiller.MapToWorld(currentNode.parent.mapPos);
		Vector3 neighborNodePos = mapFiller.MapToWorld(neighborNode.mapPos);

		// check if there is line of sight between nodes
		if (smoothPath && !Physics.Linecast(currentParentPos, neighborNodePos, wallLayermask))
		{
			float localCost = (currentNode.parent.mapPos - neighborNode.mapPos).magnitude;
			// check if neighbor node cost should be updated
			if (currentNode.parent.gCost + localCost < neighborNode.gCost)
			{
				neighborNode.gCost = currentNode.parent.gCost + localCost;
				neighborNode.parent = currentNode.parent;
				neighborNode.hCost = (neighborNode.mapPos - startNode.mapPos).magnitude;
			}
		}
		else
		{
			float localCost = (currentNode.mapPos - neighborNode.mapPos).magnitude;
			// check if neighbor node cost should be updated
			if (currentNode.gCost + localCost < neighborNode.gCost)
			{
				neighborNode.gCost = currentNode.gCost + localCost;
				neighborNode.parent = currentNode;
				neighborNode.hCost = (neighborNode.mapPos - startNode.mapPos).magnitude;
			}
		}
	}

	// Update the neighbors list for the current node
	private void UpdateNeighbors(ThetaStarNode node)
	{
		Vector2Int mapCoords = node.mapPos;
		neighbors.Clear();
		for (int i = -1; i < 2; i++)
		{
			for (int j = -1; j < 2; j++)
			{
				if (mapFiller.IsCoordValid(mapCoords.x + i, mapCoords.y + j))
				{
					neighbors.Add(map[mapCoords.x + i, mapCoords.y + j]);
				}
			}
		}
	}

	// Given a world space position, returns the corresponding grid coordinate on the map
	private bool GetMapCoordsForPosition(Vector3 pos, out Vector2Int validCoords)
	{
		Vector2Int mapCoords = mapFiller.WorldToMap(pos);

		if (mapFiller.IsCoordValid(mapCoords.x, mapCoords.y))
		{
			validCoords = mapCoords;
			return true;
		}
		else
		{
			// check neighbor nodes for a valid grid coordinate
			if (CheckCoordValidityInRange(mapCoords, 1, out validCoords))
			{
				return true;
			}
			else if (CheckCoordValidityInRange(mapCoords, 2, out validCoords))
			{
				// if no neighbor was found, check further nodes to find a valid grid coordinate
				return true;
			}
		}

		Debug.LogError("Map Coord Find Fail");
		return false;
	}

	// Check if a valid grid node exists within a given range of a given node
	private bool CheckCoordValidityInRange(Vector2Int centerCoords, int range, out Vector2Int validCoords)
	{
		for (int i = -range; i <= range; i++)
		{
			for (int j = -range; j <= range; j++)
			{
				if (mapFiller.IsCoordValid(centerCoords.x + i, centerCoords.y + j))
				{
					validCoords = centerCoords + new Vector2Int(i, j);
					return true;
				}
			}
		}

		validCoords = new Vector2Int(-1, -1);
		return false;
	}
}
