using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ThetaStarNode
{
	public Vector2Int mapPos;
	public float gCost;
	public float hCost;
	public float fCost
	{
		get
		{
			return gCost + hCost;
		}
	}
	public ThetaStarNode parent;
	public bool isWalkable;

	public ThetaStarNode(Vector2Int _mapPos)
	{
		mapPos = _mapPos;
		isWalkable = true;
		parent = null;
	}
}

public class PriorityQueue
{
	private List<ThetaStarNode> queue;

	public PriorityQueue()
	{
		queue = new List<ThetaStarNode>();
	}

	public void Enqueue(ThetaStarNode node)
	{
		queue.Add(node);
	}

	public ThetaStarNode Dequeue()
	{
		queue = queue.OrderBy(n => n.fCost).ToList();
		ThetaStarNode node = queue[0];
		queue.RemoveAt(0);

		return node;
	}

	public bool Contains(ThetaStarNode node)
	{
		return queue.Contains(node);
	}

	public bool IsEmpty()
	{
		return queue.Count == 0;
	}

	public void Remove(ThetaStarNode node)
	{
		queue.Remove(node);
	}
}
