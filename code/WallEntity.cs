using Sandbox;
using System;

namespace Maze
{
	public partial class WallEntity : RenderEntity
	{
		Material wallMaterial = Material.Load( "materials/wall.vmat" );
		Material speciaWallMaterial = Material.Load( "materials/ceiling.vmat" );
		VertexBuffer wallVB;
		VertexBuffer specialWallVB;

		TimeSince tsReset;
		float wallHeight = 0;
		float wallTargetHeight = 0;
		bool ready = false;

		public WallEntity()
		{
			Transmit = TransmitType.Always;
		}

		[Event( "maze.state" )]
		public void OnStateChange( Game.GameState s )
		{
			if ( !ready )
				return;

			switch ( s )
			{
				case Game.GameState.Intro:
					tsReset = 0;
					wallHeight = 0;
					wallTargetHeight = 1;
					break;
				case Game.GameState.InGame:
					wallHeight = wallTargetHeight = 1;
					break;
				case Game.GameState.Outro:
					tsReset = 0;
					wallHeight = 1;
					wallTargetHeight = 0;
					break;
				default:
					return;
			}
		}

		public override void Spawn()
		{
			Host.AssertClient();

			base.Spawn();

			RenderBounds = new BBox( new Vector3( -12800, -12800, 0 ), new Vector3( 12800, 12800, 128 ) );
			
			// implying that the walls are created as soon as the level was made
			wallVB = new();
			wallVB.Init( true );
			wallVB.AddQuad( new Vector3(128, 0, 0), new( 128, 0, 128 ), new( 128, 128, 128 ), new( 128, 128, 0 ) );

			specialWallVB = new();
			specialWallVB.Init( true );
			specialWallVB.AddQuad( new Vector3( 0, 0, 128 ), new( Game.Current.Level.Width * 128, 0, 128 ), new( Game.Current.Level.Width * 128, Game.Current.Level.Width * 128, 128 ), new( 0, Game.Current.Level.Width * 128, 128 ) );

			ready = true;
			OnStateChange( Game.Current.State );
		}

		[Event.Frame]
		public void UpdateWalls()
		{
			if ( !ready )
				return;

			if ( !wallHeight.AlmostEqual( wallTargetHeight ) )
			{
				wallHeight = MathF.Abs( wallTargetHeight > 0 ? tsReset / Game.Current.IntroDelay : 1 - tsReset / Game.Current.IntroDelay );
				Log.Info( $"{wallHeight} {wallTargetHeight} {tsReset} {Game.Current.IntroDelay}" );
				if ( wallHeight.AlmostEqual( wallTargetHeight ) || tsReset >= Game.Current.IntroDelay )
				{
					wallHeight = wallTargetHeight;
				}
			}
		}

		public override void DoRender( SceneObject obj )
		{
			base.DoRender( obj );

			if ( !ready )
				return;

			Render.Set( "wallheight", wallHeight );

			Render.CullMode = CullMode.Backface;
			wallVB.Draw( wallMaterial );

			Render.CullMode = CullMode.FrontFace;
			specialWallVB.Draw( speciaWallMaterial );
		}
	}
}
