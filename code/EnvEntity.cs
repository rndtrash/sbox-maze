using Sandbox;

namespace Maze
{
	public partial class EnvEntity : RenderEntity
	{
		Material floorMaterial = Material.Load( "materials/floor.vmat" );
		Material ceilingMaterial = Material.Load( "materials/ceiling.vmat" );
		VertexBuffer floorVB;
		VertexBuffer ceilingVB;

		public EnvEntity()
		{
			Transmit = TransmitType.Always;
		}

		public override void Spawn()
		{
			Host.AssertClient();

			base.Spawn();

			RenderBounds = new BBox( new Vector3( -12800, -12800, 128 ), new Vector3( -128, -128, 0 ) );
		}

		[Event( "maze.level" )]
		public void LevelLoaded()
		{
			floorVB = new();
			floorVB.Init( true );
			floorVB.AddQuad( Vector3.Zero, new( Game.Current.Level.Width * -128, 0 ), new( Game.Current.Level.Width * -128, Game.Current.Level.Width * 128 ), new( 0, Game.Current.Level.Width * 128 ) );

			ceilingVB = new();
			ceilingVB.Init( true );
			ceilingVB.AddQuad( new Vector3( 0, 0, 128 ), new( Game.Current.Level.Width * -128, 0, 128 ), new( Game.Current.Level.Width * -128, Game.Current.Level.Width * 128, 128 ), new( 0, Game.Current.Level.Width * 128, 128 ) );
		}

		public override void DoRender( SceneObject obj )
		{
			base.DoRender( obj );

			if ( floorVB == null || ceilingVB == null )
				return;

			Render.Set( "mazewidth", Game.Current.Level.Width );

			Render.CullMode = CullMode.FrontFace;
			floorVB.Draw( floorMaterial );

			Render.CullMode = CullMode.Backface;
			ceilingVB.Draw( ceilingMaterial );
		}
	}
}
