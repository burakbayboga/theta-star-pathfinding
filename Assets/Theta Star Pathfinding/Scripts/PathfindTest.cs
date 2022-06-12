using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathfindTest : MonoBehaviour
{
    public Transform startPoint;
	public Transform endPoint;

	private GameObject pathPointPrefab;
	private GameObject pathLinePrefab;

	private List<GameObject> pathObjects = new List<GameObject>();

	void Start()
	{
		pathPointPrefab = Resources.Load("Path Point") as GameObject;
		pathLinePrefab = Resources.Load("Path Line") as GameObject;

		GetPath();
	}

	private void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			RaycastHit[] hits = Physics.RaycastAll(Camera.main.ScreenToWorldPoint(Input.mousePosition), Camera.main.transform.forward, 100f, 1 << 27);
			if (hits.Length > 0)
			{
				SetStartPoint(hits[0].point);
				GetPath();
			}
		}
		else if (Input.GetMouseButtonDown(1))
		{
			RaycastHit[] hits = Physics.RaycastAll(Camera.main.ScreenToWorldPoint(Input.mousePosition), Camera.main.transform.forward, 100f, 1 << 27);
			if (hits.Length > 0)
			{
				SetEndPoint(hits[0].point);
				GetPath();
			}
		}
		else if (Input.GetKeyDown(KeyCode.Space))
		{
			//GetPath();
		}
	}

	private void GetPath()
	{
		List<Vector3> path = ThetaStar.instance.FindPath(startPoint.position, endPoint.position);
		while (pathObjects.Count > 0)
		{
			Destroy(pathObjects[0]);
			pathObjects.RemoveAt(0);
		}
		LineRenderer line = Instantiate(pathLinePrefab).GetComponent<LineRenderer>();
		line.SetPosition(0, startPoint.position);
		line.SetPosition(1, path[0]);
		pathObjects.Add(line.gameObject);
		for (int i = 0; i < path.Count - 1; i++)
		{
			pathObjects.Add(Instantiate(pathPointPrefab, path[i], Quaternion.identity));
			line = Instantiate(pathLinePrefab).GetComponent<LineRenderer>();
			line.SetPosition(0, path[i]);
			line.SetPosition(1, path[i + 1]);
			pathObjects.Add(line.gameObject);
		}

		pathObjects.Add(Instantiate(pathPointPrefab, path[path.Count - 1], Quaternion.identity));
	}

	private void SetStartPoint(Vector3 position)
	{
		startPoint.position = position;
	}

	private void SetEndPoint(Vector3 position)
	{
		endPoint.position = position;
	}
}
