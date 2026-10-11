using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Search;
using UnityEditor.UI;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class Tile
{
	public TilePrototype prototype;

	public int yPlusEdge;
	public int yMinusEdge;

	public int zPlusEdge;
	public int zMinusEdge;

	public int xPlusEdge;
	public int xMinusEdge;

	public List<Tile> yPlusNeighbors;
	public List<Tile> yMinusNeighbors;

	public List<Tile> zPlusNeighbors;
	public List<Tile> zMinusNeighbors;

	public List<Tile> xPlusNeighbors;
	public List<Tile> xMinusNeighbors;

	private int yRotations;

	public Tile(int yRots, TilePrototype proto)
	{
		prototype = proto;
		yRotations = yRots;

		//TODO maybe all the edges should be in a list like this throughout the code instead of seperate vars.

		//rotate edges
		Queue<int> pEdges = new Queue<int>(new int[]{ proto.zPlusEdge, proto.xPlusEdge, proto.zMinusEdge, proto.xMinusEdge });
		for (int i =0;i<yRotations;i++){
			pEdges.Enqueue(pEdges.Dequeue());
		}
		zPlusEdge = pEdges.Dequeue();
		xPlusEdge = pEdges.Dequeue();
		zMinusEdge = pEdges.Dequeue();
		xMinusEdge = pEdges.Dequeue();

		yPlusEdge = proto.yPlusEdge;
		yMinusEdge = proto.yMinusEdge;

		yPlusNeighbors = new();
		yMinusNeighbors = new();
		zPlusNeighbors = new();
		zMinusNeighbors = new();
		xPlusNeighbors = new();
		xMinusNeighbors = new();
	}

	public void ConsiderNeighboring(Tile other)
	{
		//TODO do something better than the weird tuple matrix?
		//compare edges with another tile, and add to the neighbors list for both this and the other tile if possible
		(int, int, List<Tile>, List<Tile>)[] edgeNeighborPairs ={
			(zPlusEdge, other.zMinusEdge, zPlusNeighbors, other.zMinusNeighbors),
			(zMinusEdge, other.zPlusEdge, zMinusNeighbors, other.zPlusNeighbors),
			(xPlusEdge, other.xMinusEdge, xPlusNeighbors, other.xMinusNeighbors),
			(xMinusEdge, other.xPlusEdge, xMinusNeighbors, other.xPlusNeighbors),
			(yPlusEdge, other.yMinusEdge, yPlusNeighbors, other.yMinusNeighbors),
			(yMinusEdge, other.yPlusEdge, yMinusNeighbors, other.yPlusNeighbors)};
		foreach ((int, int, List<Tile>, List<Tile>) p in edgeNeighborPairs)
		{
			if (p.Item1 == p.Item2 || p.Item1 == -1 || p.Item2 == -1) // -1 means it connects to anything
			{
				if (!p.Item3.Contains(other)) p.Item3.Add(other);
				if (!p.Item4.Contains(this)) p.Item4.Add(this);
			}
		}
	}

	public Quaternion getRotation()
	{
		//TODO create rotations in a way that doesn't use a queue and make this positive
		return Quaternion.Euler(new Vector3(0, yRotations * -90, 0));
	}

	public override string ToString()
	{
		return prototype.name + "_" + yRotations + "_tile";
	}
}
