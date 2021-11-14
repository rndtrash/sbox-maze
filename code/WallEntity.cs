using Sandbox;
using System;

namespace Maze
{
	public partial class WallEntity : RenderEntity
	{
		Material wallMaterial = Material.Load( "materials/wall.vmat" );
		Material speciaWallMaterial = Material.Load( "materials/directx.vmat" );
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

		protected void AddWall( int cellX, int cellY, uint flags )
		{
			if ( flags == 0 )
				return;

			int worldX = cellX * 128;
			int worldY = cellY * 128;

			if ( (flags | Side.North) > 0 )
			{
				wallVB.AddQuad( new Vector3( worldX - 128, worldY, 128 ), new( worldX, worldY, 128 ), new( worldX, worldY, 0 ), new( worldX - 128, worldY, 0 ) );
			}

			if ( (flags | Side.East) > 0 )
			{
				wallVB.AddQuad( new Vector3( worldX - 128, worldY + 128, 128 ), new( worldX - 128, worldY, 128 ), new( worldX - 128, worldY, 0 ), new( worldX - 128, worldY + 128, 0 ) );
			}

			if ( (flags | Side.South) > 0 )
			{
				wallVB.AddQuad( new Vector3( worldX, worldY + 128, 128 ), new( worldX - 128, worldY + 128, 128 ), new( worldX - 128, worldY + 128, 0 ), new( worldX, worldY + 128, 0 ) );
			}

			if ( (flags | Side.West) > 0 )
			{
				wallVB.AddQuad( new Vector3( worldX, worldY, 128 ), new( worldX, worldY + 128, 128 ), new( worldX, worldY + 128, 0 ), new( worldX, worldY, 0 ) );
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

			AddWall( 0, 0, Side.North | Side.East | Side.South | Side.West );

			specialWallVB = new();
			specialWallVB.Init( true );

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

			//Render.CullMode = CullMode.Backface;
			wallVB.Draw( wallMaterial );

			//Render.CullMode = CullMode.FrontFace;
			specialWallVB.Draw( speciaWallMaterial );
		}
	}
}
