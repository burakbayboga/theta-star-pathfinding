using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThetaStar : MonoBehaviour
{
	public static ThetaStar instance;

	public GameObject processedNodeObject;
	public bool smoothPath;
	
	private MapFiller mapFiller;
	private ThetaStarNode[,] map;
	private int mapSizeX;
	private int mapSizeY;

	private PriorityQueue openList;
	private List<ThetaStarNode> closedList;
	private List<ThetaStarNode> neighbors;

	private Vector2Int startCoords;
	private Vector2Int endCoords;

	private Vector3 startPoint;
	private Vector3 endPoint;

	private int wallLayermask = 1 << 28;

	private ThetaStarNode endNode;
	private ThetaStarNode startNode;

	private void Awake()
	{
		instance = this;
		mapFiller = GetComponent<MapFiller>();
		map = mapFiller.GetMap(out mapSizeX, out mapSizeY);
	}

	public List<Vector3> FindPath(Vector3 _startPoint, Vector3 _endPoint)
	{
		startPoint = _startPoint;
		endPoint = _endPoint;
		startCoords = GetMapCoordsForPosition(startPoint);
		endCoords = GetMapCoordsForPosition(endPoint);

		openList = new PriorityQueue();
		closedList = new List<ThetaStarNode>();
		neighbors = new List<ThetaStarNode>();

		startNode = map[startCoords.x, startCoords.y];
		endNode = map[endCoords.x, endCoords.y];
		endNode.gCost = 0f;
		endNode.hCost = (endNode.mapPos - startNode.mapPos).magnitude;
		endNode.parent = endNode;
		openList.Enqueue(endNode);


		//StartCoroutine(SearchPath());
		while (!openList.IsEmpty())
		{
			ThetaStarNode currentNode = openList.Dequeue();
			closedList.Add(currentNode);
			//Instantiate(processedNodeObject, mapFiller.MapToWorld(currentNode.mapPos), Quaternion.identity);

			if (currentNode == startNode)
			{
				// path found
				return ExtractPath(currentNode, endNode);
			}

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

		return new List<Vector3>{ startPoint };
	}

	private IEnumerator SearchPath()
	{
		while (!openList.IsEmpty())
		{
			ThetaStarNode currentNode = openList.Dequeue();
			closedList.Add(currentNode);
			//Instantiate(processedNodeObject, mapFiller.MapToWorld(currentNode.mapPos), Quaternion.identity);

			if (currentNode == startNode)
			{
				// path found
				print("PATH FOUND");
				ExtractPath(currentNode, endNode);
				yield break;
			}

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
			yield return new WaitForSeconds(0.15f);
			//yield return null;
		}
	}

	private List<Vector3> ExtractPath(ThetaStarNode currentNode, ThetaStarNode endNode)
	{
		List<Vector3> path = new List<Vector3>();
		while (currentNode != endNode)
		{
			path.Add(mapFiller.MapToWorld(currentNode.mapPos));
			currentNode = currentNode.parent;
		}
		path.Add(mapFiller.MapToWorld(currentNode.mapPos));
		path.Add(endPoint);

		if (path.Count > 1)
		{
			if (!Physics.Linecast(startPoint, path[1], wallLayermask))
			{
				path.RemoveAt(0);
			}
		}

		if (path.Count > 1)
		{
			if (!Physics.Linecast(endPoint, path[path.Count - 2], wallLayermask))
			{
				path.RemoveAt(path.Count - 2);
			}
		}



		return path;
	}

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

	private void CalculateCost(ThetaStarNode currentNode, ThetaStarNode neighborNode)
	{
		Vector3 currentParentPos = mapFiller.MapToWorld(currentNode.parent.mapPos);
		Vector3 neighborNodePos = mapFiller.MapToWorld(neighborNode.mapPos);

		if (smoothPath && !Physics.Linecast(currentParentPos, neighborNodePos, wallLayermask))
		{
			float localCost = (currentNode.parent.mapPos - neighborNode.mapPos).magnitude;
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
			if (currentNode.gCost + localCost < neighborNode.gCost)
			{
				neighborNode.gCost = currentNode.gCost + localCost;
				neighborNode.parent = currentNode;
				neighborNode.hCost = (neighborNode.mapPos - startNode.mapPos).magnitude;
			}
		}
	}

	private void UpdateNeighbors(ThetaStarNode node)
	{
		Vector2Int mapCoords = node.mapPos;
		neighbors.Clear();
		for (int i = -1; i < 2; i++)
		{
			for (int j = -1; j < 2; j++)
			{
				if (IsCoordValid(mapCoords.x + i, mapCoords.y + j))
				{
					neighbors.Add(map[mapCoords.x + i, mapCoords.y + j]);
				}
			}
		}
	}

	private Vector2Int GetMapCoordsForPosition(Vector3 pos)
	{
		Vector2Int mapCoords = mapFiller.WorldToMap(pos);

		if (IsCoordValid(mapCoords.x, mapCoords.y))
		{
			return mapCoords;
		}
		else
		{
			for (int i = -1; i < 2; i += 2)
			{
				for (int j = -1; j < 2; j += 2)
				{
					if (IsCoordValid(mapCoords.x + i, mapCoords.y + j))
					{
						return mapCoords + new Vector2Int(i, j);
					}
				}
			}
		}

		Debug.LogError("Map Coord Find Fail");
		return Vector2Int.zero;
	}

	private bool IsCoordValid(int xCoord, int yCoord)
	{
		return xCoord >= 0 && xCoord < mapSizeX
				&& yCoord >= 0 && yCoord < mapSizeY
				&& map[xCoord, yCoord].isWalkable;
	}
}
