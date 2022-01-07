using Sandbox;

namespace Maze
{
	public partial class Level : BaseNetworkable
	{
		public int Width => 16;
		public WallEntity Walls { get; internal set; }

		public void InitClient()
		{
			Log.Info( "level: before assert" );
			Host.AssertClient();
			Log.Info( "level: after assert" );

			Walls = new();
			_ = new BillboardEntity { Position = CellToWorld( 1, 1 ) + CellCenter }; // TEST
		}
	}
}
