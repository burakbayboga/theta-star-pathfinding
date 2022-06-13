using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// The nodes on the grid map
public class ThetaStarNode
{
	// Coordinates of the node on the grid map
	public Vector2Int mapPos;

	// gCost: The cost of a node to the start point
	public float gCost;

	// hCost: Heuristic cost of a node to the end point. It must be less than or equal to the actual cost.
	public float hCost;

	// fCost: The sum of gCost and hCost
	public float fCost
	{
		get
		{
			return gCost + hCost;
		}
	}

	// parent: The node that the algorithm reached this node from.
	public ThetaStarNode parent;

	// isWalkable: If true, it is a valid node for pathfinding
	public bool isWalkable;

	public ThetaStarNode(Vector2Int _mapPos)
	{
		mapPos = _mapPos;
		isWalkable = true;
		parent = null;
	}
}

// A list to take the lowest code node. Used for the open nodes list.
public class PriorityQueue
{
	private List<ThetaStarNode> queue;

	public PriorityQueue()
	{
		queue = new List<ThetaStarNode>();
	}

	// Adds a given node to the list
	public void Enqueue(ThetaStarNode node)
	{
		queue.Add(node);
	}

	// Returns the node with the lowest cost and removes it from the list
	public ThetaStarNode Dequeue()
	{
		queue = queue.OrderBy(n => n.fCost).ToList();
		ThetaStarNode node = queue[0];
		queue.RemoveAt(0);

		return node;
	}

	// Returns true if the list contains the given node
	public bool Contains(ThetaStarNode node)
	{
		return queue.Contains(node);
	}

	// Returns true if the list is empty
	public bool IsEmpty()
	{
		return queue.Count == 0;
	}

	// Removes a given node from the list
	public void Remove(ThetaStarNode node)
	{
		queue.Remove(node);
	}
}
