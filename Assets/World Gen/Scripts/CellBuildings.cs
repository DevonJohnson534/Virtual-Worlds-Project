using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Voronoi;
public class CellBuildings
{
    public static List<Cell> getConstructionZones(Cell c)
    {
        List<Cell> rectangles = new List<Cell>();
        Cell first = constructionZone(c, c.longestBorderIndex());
        rectangles.Add(first);

        if (validSize(first))
        {
            List<Cell> candidates = new List<Cell>();
            candidates.Add(branchPlotting(c, first, 1, rectangles));
            candidates.Add(branchPlotting(c, first, 2, rectangles));
            candidates.Add(branchPlotting(c, first, 3, rectangles));

            foreach(Cell s in candidates)
            {
                if(c != null || validSize(c))
                {
                    rectangles.Add(s);
                }
            }

        }


        return rectangles;
    }



    private static Cell branchPlotting(Cell superCell, Cell branchedFrom, int borderIndex, List<Cell> all)
    {
        Line knownBorder = new Line(branchedFrom.boundaries[borderIndex]);

        if(knownBorder.getLength() <= 2)
        {
            return null;
        }


        Vector2 dir = knownBorder.getDirection();
        float safeLen = superCell.longestBorderLength();

        Line backwards = new Line(knownBorder.midpoint(), (knownBorder.start + (safeLen * -dir)));
        Line forwards = new Line(knownBorder.midpoint(), (knownBorder.end + (safeLen * dir)));

        Vector2? intersect1 = null;
        Vector2? intersect2 = null;

        foreach(Line l in superCell.boundaries)
        {
            Vector2? ibackward = backwards.intersection(l, true);
            Vector2? iforward = forwards.intersection(l, true);

            if (ibackward.HasValue)
            {
                intersect1 = ibackward.Value;
            }
            else if (iforward.HasValue)
            {
                intersect2 = iforward.Value;
            }
        }

        Line crossingLine = new Line(intersect1.Value, intersect2.Value);
        Debug.Log("split: " + crossingLine.toString());

        Cell subCell = superCell.splitCell(crossingLine, true);

        if (branchedFrom.inCell(subCell.getCenter()))
        {

            subCell = superCell.splitCell(crossingLine, false);
        }

        //Streets.materializeCell(subCell, -1);

        int indexForBranch = 0;
        for(int i = 0; i < subCell.boundaries.Length; i++)
        {
            if (subCell.boundaries[i].equals(crossingLine))
            {
                indexForBranch = i;
            }
        }

        return constructionZone(subCell, indexForBranch);
    }


    public static bool validSize(Cell cell)
    {
        Line[] boundaries = cell.boundaries;
        return (boundaries[0].getLength() > 2 && boundaries[1].getLength() > 2);
    }


    public static Cell constructionZone(Cell cell, int knownLineIndex)
    {
        Line[] boundaries = cell.boundaries;
        Line known = new Line(boundaries[knownLineIndex]);

        Line[] adjacentEdges = newEdges(known, knownLineIndex, boundaries, true, true);

        Line last = adjacentEdges[0];
        Line next = adjacentEdges[1];

        bool skipLastCheck = true;
        bool skipNextCheck = true;

        if (!last.valid)
        {
            known.resizeFromStart(known.getLength() / 4);
            skipLastCheck = false;
        }

        if (!next.valid)
        {
            known.resize(known.getLength() / -4);
            skipNextCheck = false;
        }

        adjacentEdges = newEdges(known, knownLineIndex, boundaries, skipLastCheck, skipNextCheck);
        last = adjacentEdges[0];
        next = adjacentEdges[1];


        minimize(last, next);

        Line missing = new Line(next.end, last.end);
        Line flipped = new Line(last.end, last.start);
        Line[] lines = new Line[] { known, next, missing, flipped };
        return new Cell(lines);

    }


    private static void minimize(Line a, Line b)
    {
        float aLength = a.getLength();
        float bLength = b.getLength();

        if (aLength <= bLength)
        {
            float dif = bLength - aLength;
            b.resize(-dif);
        }
        else
        {
            float dif = aLength - bLength;
            a.resize(-dif);
        }
    }


    private static Line[] newEdges(Line knownEdge, int knownIndex, Line[] boundaries, bool skipLast, bool skipNext)
    {
        Vector2 start = knownEdge.start;
        Vector2 end = knownEdge.end;
        Vector2 direction = knownEdge.getDirection();
        Vector2 inward = new Vector2(direction.y, -direction.x);

        float length = knownEdge.getLength();

        Line lastEdge = new Line(start, start + (inward * 3 * length));
        Line nextEdge = new Line(end, end + (inward * 3 * length));

        bool foundLast = false;
        bool foundNext = false;

        int startIndex = skipNext ? knownIndex + 2 : knownIndex + 1;
        int endIndex = skipLast ? (boundaries.Length + knownIndex - 1): boundaries.Length + knownIndex;

        for (int i = startIndex; i < endIndex; i++)
        {
            int index = i % boundaries.Length;

            Vector2? intersectCheckLast = lastEdge.intersection(boundaries[index], true);
            Vector2? intersectCheckNext = nextEdge.intersection(boundaries[index], true);

            if (!foundLast && intersectCheckLast.HasValue)
            {
                lastEdge.end = intersectCheckLast.Value;
                foundLast = true;
            }

            if (!foundNext && intersectCheckNext.HasValue)
            {
                nextEdge.end = intersectCheckNext.Value;
                foundNext = true;
            }
        }

        lastEdge.valid = foundLast;
        nextEdge.valid = foundNext;

        Line[] newEdges = new Line[2];

        newEdges[0] = lastEdge;
        newEdges[1] = nextEdge;

        return newEdges;

    }


}
