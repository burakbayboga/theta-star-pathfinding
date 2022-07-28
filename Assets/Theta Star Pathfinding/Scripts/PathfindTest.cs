using System.Collections.Generic;
using UnityEngine;

// This is the test class for the demo scene. It can be used to copy certain behaviors.
public class PathfindTest : MonoBehaviour
{
    public Transform startPoint;
	public Transform endPoint;
	public LayerMask groundLayermask;

	private GameObject pathPointPrefab;
	private GameObject pathLinePrefab;

	// This is used for mobile testing to differentiate between setting start and end points with touch input
	private bool settingStartPoint = true;

	private List<GameObject> pathObjects = new List<GameObject>();

	void Start()
	{
		pathPointPrefab = Resources.Load("Path Point") as GameObject;
		pathLinePrefab = Resources.Load("Path Line") as GameObject;

		//GetPath();
	}

	private void Update()
	{
#if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
		CheckTouchInput();
#else
		CheckMouseInput();
#endif
	}

	private void CheckTouchInput()
	{
		if (Input.touchCount > 0)
		{
			Touch touch = Input.GetTouch(0);
			if (touch.phase == TouchPhase.Began)
			{
				RaycastHit[] hits = Physics.RaycastAll(Camera.main.ScreenToWorldPoint(touch.position), Camera.main.transform.forward, 100f, groundLayermask);
				if (hits.Length > 0)
				{
					if (settingStartPoint)
					{
						SetStartPoint(hits[0].point);
					}
					else
					{
						SetEndPoint(hits[0].point);
					}
					settingStartPoint = !settingStartPoint;
					GetPath();
				}
			}
		}
	}

	private void CheckMouseInput()
	{
		if (Input.GetMouseButtonDown(0))
		{
			// Get the position for the start point
			RaycastHit[] hits = Physics.RaycastAll(Camera.main.ScreenToWorldPoint(Input.mousePosition), Camera.main.transform.forward, 100f, groundLayermask);
			if (hits.Length > 0)
			{
				SetStartPoint(hits[0].point);
				GetPath();
			}
		}
		else if (Input.GetMouseButtonDown(1))
		{
			// Get the position for the end point
			RaycastHit[] hits = Physics.RaycastAll(Camera.main.ScreenToWorldPoint(Input.mousePosition), Camera.main.transform.forward, 100f, groundLayermask);
			if (hits.Length > 0)
			{
				SetEndPoint(hits[0].point);
				GetPath();
			}
		}
	}

	// Get a path and draw it on the scene using spheres and lines
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
