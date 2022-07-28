using UnityEngine;

public class MapFiller : MonoBehaviour
{
	public bool testNodeWalkableStates = false;
	public string obstacleTag;
	public string groundTag;

	public float nodeDistance;

	private int mapSizeX;
	private int mapSizeY;
	private ThetaStarNode[,] map;

	private Vector3 groundPos;
	private float halfMapSizeX;
	private float halfMapSizeY;

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
		Transform groundTransform = GameObject.FindGameObjectWithTag(groundTag).GetComponent<Transform>();
		groundPos = groundTransform.position;
		mapSizeX = (int)(groundTransform.localScale.x / nodeDistance) + 1;
		mapSizeY = (int)(groundTransform.localScale.y / nodeDistance) + 1;
		//halfMapSizeX = (mapSizeX - 1f) / 2f;
		//halfMapSizeY = (mapSizeY - 1f) / 2f;
		halfMapSizeX = groundTransform.localScale.x / 2f;
		halfMapSizeY = groundTransform.localScale.y / 2f;

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
		return new Vector3(mapCoords.x * nodeDistance - halfMapSizeX, 0.5f, mapCoords.y * nodeDistance - halfMapSizeY) + groundPos;
	}

	// Convert a given world space position into a grid map coordinate
	public Vector2Int WorldToMap(Vector3 worldPos)
	{
		return new Vector2Int((int)Mathf.Round((worldPos.x - groundPos.x + halfMapSizeX) / nodeDistance), (int)Mathf.Round((worldPos.z - groundPos.z + halfMapSizeY) / nodeDistance));
	}

	// Fill unwalkable nodes on the grid map (walls, obstacles etc.)
	private void FillMap()
	{
		GameObject[] walls = GameObject.FindGameObjectsWithTag(obstacleTag);

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
		min.x = Mathf.Ceil(min.x / nodeDistance) * nodeDistance;
		min.z = Mathf.Ceil(min.z / nodeDistance) * nodeDistance;
		max.x = Mathf.Floor(max.x / nodeDistance) * nodeDistance;
		max.z = Mathf.Floor(max.z / nodeDistance) * nodeDistance;
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
