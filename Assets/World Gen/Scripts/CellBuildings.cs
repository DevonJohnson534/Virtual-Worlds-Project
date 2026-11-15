using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Voronoi;
public class CellBuildings
{




    public static Line[] constructionZone(Cell cell)
    {
        Line[] boundaries = cell.boundaries;
        int longIndex = cell.longestBorder();
        Line longest = boundaries[longIndex];

        Line[] adjacentEdges = newEdges(longest, longIndex, boundaries, true, true);

        Line last = adjacentEdges[0];
        Line next = adjacentEdges[1];

        bool skipLastCheck = true;
        bool skipNextCheck = true;

        if (!last.valid)
        {
            longest.resizeFromStart(longest.getLength() / 3);
            skipLastCheck = false;
        }

        if (!next.valid)
        {
            longest.resize(longest.getLength() / -3);
            skipNextCheck = false;
        }

        adjacentEdges = newEdges(longest, longIndex, boundaries, skipLastCheck, skipNextCheck);

        last = adjacentEdges[0];
        next = adjacentEdges[1];

        minimize(last, next);

        Line missing = new Line(next.end, last.end);
        Line flipped = new Line(last.end, last.start);
        return new Line[] { longest, next, missing, flipped };

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
        int endIndex = skipLast ? boundaries.Length + knownIndex - 1 : boundaries.Length + knownIndex;

        for (int i = startIndex; i < endIndex; i++)
        {
            int index = i % boundaries.Length;

            Vector2? intersectCheckLast = lastEdge.intersection(boundaries[index]);
            Vector2? intersectCheckNext= nextEdge.intersection(boundaries[index]);

            if (!foundLast && intersectCheckLast.HasValue)
            {
                lastEdge.end = intersectCheckLast.Value;
            }

            if (!foundNext && intersectCheckNext.HasValue)
            {
                nextEdge.end = intersectCheckNext.Value;
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
