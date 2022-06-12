using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapFiller : MonoBehaviour
{
	public GameObject walkableTest;
	public GameObject nonWalkableTest;

	private int mapSizeX;
	private int mapSizeY;
	private ThetaStarNode[,] map;

	private Vector3 groundPos;
	private int halfMapSizeX;
	private int halfMapSizeY;


	void Start()
	{
		//GetMap(out int x, out int y);
	}

	public ThetaStarNode[,] GetMap(out int _mapSizeX, out int _mapSizeY)
	{
		InitMap();
		FillMap();
		//FillTest();

		_mapSizeX = mapSizeX;
		_mapSizeY = mapSizeY;
		return map;
	}

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

	private void FillTest()
	{
		for (int i = 0; i < mapSizeX; i++)
		{
			for (int j = 0; j < mapSizeY; j++)
			{
				ThetaStarNode node = map[i, j];
				Vector3 pos = MapToWorld(node.mapPos);
				if (!map[i, j].isWalkable)
				{
					Instantiate(nonWalkableTest, pos, Quaternion.identity);
				}
				else
				{
					Instantiate(walkableTest, pos, Quaternion.identity);
				}
			}
		}
	}

	public Vector3 MapToWorld(Vector2Int mapCoords)
	{
		return new Vector3(mapCoords.x - halfMapSizeX, 0.5f, mapCoords.y - halfMapSizeY) + groundPos;
	}

	public Vector2Int WorldToMap(Vector3 worldPos)
	{
		return new Vector2Int((int)Mathf.Round(worldPos.x - groundPos.x + halfMapSizeX), (int)Mathf.Round(worldPos.z - groundPos.z + halfMapSizeY));
	}

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
	
	private bool IsCoordWithinBounds(int xCoord, int yCoord)
	{
		return xCoord >= 0 && xCoord < mapSizeX
				&& yCoord >= 0 && yCoord < mapSizeY;
	}
}
