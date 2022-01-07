namespace Maze
{
	public partial class Level
	{
		public static class Side
		{
			public const uint North = 1;
			public const uint East = 2;
			public const uint South = 4;
			public const uint West = 8;
		};

		public readonly Vector2 CellCenter = new(-64, 64);

		public Vector2 CellToWorld( int x, int y )
		{
			return new Vector2( x * -128, y * 128 );
		}
	}
}
