using UnityEngine;

public class MapFiller : MonoBehaviour
{
	public bool testNodeWalkableStates = false;

	private int mapSizeX;
	private int mapSizeY;
	private ThetaStarNode[,] map;

	private Vector3 groundPos;
	private int halfMapSizeX;
	private int halfMapSizeY;

	private GameObject walkableNodeTestPrefab;
	private GameObject nonWalkableNodeTestPrefab;

	// Generate the grid map and return it
	public ThetaStarNode[,] GetMap()
	{
		InitMap();
		FillMap();
		if (testNodeWalkableStates)
		{
			FillTest();
		}

		return map;
	}

	// Initialize the map grid
	private void InitMap()
	{
		Transform groundTransform = GameObject.FindGameObjectWithTag("Theta Star Ground").GetComponent<Transform>();
		groundPos = groundTransform.position;
		mapSizeX = (int)groundTransform.localScale.x + 1;
		mapSizeY = (int)groundTransform.localScale.y + 1;
		halfMapSizeX = (mapSizeX - 1) / 2;
		halfMapSizeY = (mapSizeY - 1) / 2;

		map = new ThetaStarNode[mapSizeX, mapSizeY];

		for (int i = 0; i < mapSizeX; i++)
		{
			for (int j = 0; j < mapSizeY; j++)
			{
				Vector2Int mapPos = new Vector2Int(i, j);
				ThetaStarNode newNode = new ThetaStarNode(mapPos);
				map[i, j] = newNode;
			}
		}
	}

	// Instantiate objects to see which grid nodes are walkable and which grid nodes are not
	private void FillTest()
	{
		walkableNodeTestPrefab = Resources.Load("walkable test") as GameObject;
		nonWalkableNodeTestPrefab = Resources.Load("non walkable test") as GameObject;
		for (int i = 0; i < mapSizeX; i++)
		{
			for (int j = 0; j < mapSizeY; j++)
			{
				ThetaStarNode node = map[i, j];
				Vector3 pos = MapToWorld(node.mapPos);
				if (!map[i, j].isWalkable)
				{
					Instantiate(nonWalkableNodeTestPrefab, pos, Quaternion.identity);
				}
				else
				{
					Instantiate(walkableNodeTestPrefab, pos, Quaternion.identity);
				}
			}
		}
	}

	// Convert a given grid map coordinate into a world space position
	public Vector3 MapToWorld(Vector2Int mapCoords)
	{
		return new Vector3(mapCoords.x - halfMapSizeX, 0.5f, mapCoords.y - halfMapSizeY) + groundPos;
	}

	// Convert a given world space position into a grid map coordinate
	public Vector2Int WorldToMap(Vector3 worldPos)
	{
		return new Vector2Int((int)Mathf.Round(worldPos.x - groundPos.x + halfMapSizeX), (int)Mathf.Round(worldPos.z - groundPos.z + halfMapSizeY));
	}

	// Fill unwalkable nodes on the grid map (walls, obstacles etc.)
	private void FillMap()
	{
		GameObject[] walls = GameObject.FindGameObjectsWithTag("Theta Star Wall");

		for (int i = 0; i < walls.Length; i++)
		{
			Collider collider = walls[i].GetComponent<Collider>();
			Bounds bounds = collider.bounds;
			PlaceWallOnMap(bounds.min, bounds.max);
		}
	}

	// Place a given obstacle on the grid map
	private void PlaceWallOnMap(Vector3 min, Vector3 max)
	{
		min.x = Mathf.Ceil(min.x);
		min.z = Mathf.Ceil(min.z);
		max.x = Mathf.Floor(max.x);
		max.z = Mathf.Floor(max.z);
		Vector2Int minCoords = WorldToMap(min);
		Vector2Int maxCoords = WorldToMap(max);

		for (int i = minCoords.x; i <= maxCoords.x; i++)
		{
			for (int j = minCoords.y; j <= maxCoords.y; j++)
			{
				if (!IsCoordWithinBounds(i, j))
				{
					continue;
				}
				map[i, j].isWalkable = false;
			}
		}
	}
	
	// check if a given coordinate is within grid bounds
	private bool IsCoordWithinBounds(int xCoord, int yCoord)
	{
		return xCoord >= 0 && xCoord < mapSizeX
				&& yCoord >= 0 && yCoord < mapSizeY;
	}

	// check if a given coordinate is within grid bounds and walkable (not occupied by wall or obstacle)
	public bool IsCoordValid(int xCoord, int yCoord)
	{
		return IsCoordWithinBounds(xCoord, yCoord) && map[xCoord, yCoord].isWalkable;
	}
}
