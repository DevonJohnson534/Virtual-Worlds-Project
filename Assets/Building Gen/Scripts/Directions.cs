using System;

namespace BuildingGen
{
	public class Directions
	{
		public const int X_NEG = 0; //left
		public const int Y_NEG = 1; //down
		public const int Z_NEG = 2; //back
		public const int X_POS = 3; //right
		public const int Y_POS = 4; //up
		public const int Z_POS = 5; //forward
		public static readonly int[] HorizontalDirs = { X_NEG, Z_POS, X_POS, Z_NEG }; //ordered for clockwise rotating

		public static int OppositeDir(int dir)
		{
			//gives the opposite direction. ie, X_NEG gives X_POS
			return (dir + 3) % 6;
		}

		public static bool IsHorizontal(int dir)
		{
			return (dir != Y_POS) && (dir != Y_NEG);
		}

		public static EdgeInfo[] RotateEdges(EdgeInfo[] input, int numRotations)
		{
			//each rotation is 90 degrees clockwise around the y axis. numRotations is the number of rotations.
			EdgeInfo[] output = new EdgeInfo[6];

			for (int i = 0; i < input.Length; i++)
			{
				if (!IsHorizontal(i)) //rotate vertical edges
				{
					output[i] = input[i].CreateCopy();
					if (output[i].rot != -1)
					{
						output[i].rot = (output[i].rot + numRotations) % 4;
					}
				}
				else //rotate horizontal edges
				{
					int newDir = HorizontalDirs[(Array.IndexOf(HorizontalDirs, i) + numRotations) % 4];
					output[newDir] = input[i].CreateCopy();
				}
			}

			return output;
		}
	}
}