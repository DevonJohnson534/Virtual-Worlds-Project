using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Voronoi
{
    public class VoronoiDiagram
    {
        float limit;

        public VoronoiDiagram(float limit)
        {
            this.limit = limit;
        }
        public List<Cell> getCells(List<Vector2> points)
        {
            Vector2[] corners = new Vector2[4]; // big square

            corners[0] = new Vector2(-limit, -limit);
            corners[1] = new Vector2(-limit, limit);
            corners[2] = new Vector2(limit, limit);
            corners[3] = new Vector2(limit, -limit);

            Cell limits = new Cell(corners, points[0]);

            List<Cell> cells = new List<Cell>(points.Count);

            foreach (Vector2 currentPoint in points)
            {
                Cell cell = new Cell(limits.boundaries, currentPoint);

                foreach (Vector2 point in points)
                {
                    if (point == currentPoint)
                    {
                        continue;
                    }

                    Vector2 midpoint = (currentPoint + point) / 2;
                    Vector2 difference = currentPoint - point;
                    Vector2 perpendicular = new Vector2(-difference.y, difference.x);
                    perpendicular.Normalize();

                    Vector2 bisectorPos = midpoint + (4 * limit * perpendicular);
                    Vector2 bisectorNeg = midpoint - (4 * limit * perpendicular);

                    Line bisector = new Line(bisectorPos, bisectorNeg);

                    List<Vector2> bisectorIntersections = new List<Vector2>(2);
                    List<int> intersectedIndicies = new List<int>(2);

                    for (int i = 0; i < cell.boundaries.Length; i++)
                    {
                        Line cellBoundary = cell.boundaries[i];
                        Vector2? firstIntersection = bisector.intersection(cellBoundary, true);
                        Vector2? secondIntersection = bisector.intersection(cellBoundary, true);

                        if (firstIntersection.HasValue)
                        {
                            bisectorIntersections.Insert(0, (Vector2)firstIntersection);
                            intersectedIndicies.Insert(0, i);
                        }

                        if (secondIntersection.HasValue)
                        {
                            bisectorIntersections.Add((Vector2)secondIntersection);
                            intersectedIndicies.Add(i);
                        }
                    }

                    bisectorIntersections = bisectorIntersections.Distinct().ToList();

                    if (bisectorIntersections.Count == 2)
                    {
                        Line newSegment = new Line(bisectorIntersections[0], bisectorIntersections[1]);

                        int split1 = intersectedIndicies[0];
                        int split2 = intersectedIndicies[1];
                        Cell subCell = new Cell(splitCell(newSegment, cell, split1, split2, true));

                        if (subCell.inCell(currentPoint))
                        {
                            cell.boundaries = subCell.boundaries;
                        }
                        else
                        {
                            cell.boundaries = splitCell(newSegment, cell, split1, split2, false);
                        }
                    }
                }
                cells.Add(cell);
            }

            return cells;
        }



        private Line[] splitCell(Line bisection, Cell cell, int split1, int split2, bool rightSplit)
        {
            List<Line> newCell = new List<Line>();
            if (rightSplit)
            {
                newCell.Add(new Line(cell.boundaries[split2].start, bisection.end));
                newCell.Add(flipIfNeeded(newCell[newCell.Count - 1], bisection));
                newCell.Add(new Line(bisection.start, cell.boundaries[split1].end));
            }
            else
            {
                newCell.Add(new Line(cell.boundaries[split1].start, bisection.start));
                newCell.Add(flipIfNeeded(newCell[newCell.Count - 1], bisection));
                newCell.Add(new Line(bisection.end, cell.boundaries[split2].end));
            }

            int startIndex = rightSplit ? split1 : split2;
            int endIndex = rightSplit ? split2 : split1;
            startIndex = (startIndex + 1) % cell.boundaries.Length;

            for (int i = startIndex; i % cell.boundaries.Length != endIndex; i++)
            {
                newCell.Add(cell.boundaries[i % cell.boundaries.Length]);
            }

            return newCell.ToArray();
        }


        private Line flipIfNeeded(Line a, Line b)
        {
            if (a.end == b.start)
            {
                return b;
            }

            return new Line(b.end, b.start);
        }

    }
}
