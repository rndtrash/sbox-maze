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

		bool ready = false;

		public WallEntity()
		{
			Transmit = TransmitType.Always;
		}

		protected void AddWall( int cellX, int cellY, uint flags )
		{
			if ( flags == 0 )
				return;

			int worldX = cellX * 128;
			int worldY = cellY * 128;

			if ( (flags | Level.Side.North) > 0 )
			{
				wallVB.AddQuad( new Vector3( worldX - 128, worldY, 128 ), new( worldX, worldY, 128 ), new( worldX, worldY, 0 ), new( worldX - 128, worldY, 0 ) );
			}

			if ( (flags | Level.Side.East) > 0 )
			{
				wallVB.AddQuad( new Vector3( worldX - 128, worldY + 128, 128 ), new( worldX - 128, worldY, 128 ), new( worldX - 128, worldY, 0 ), new( worldX - 128, worldY + 128, 0 ) );
			}

			if ( (flags | Level.Side.South) > 0 )
			{
				wallVB.AddQuad( new Vector3( worldX, worldY + 128, 128 ), new( worldX - 128, worldY + 128, 128 ), new( worldX - 128, worldY + 128, 0 ), new( worldX, worldY + 128, 0 ) );
			}

			if ( (flags | Level.Side.West) > 0 )
			{
				wallVB.AddQuad( new Vector3( worldX, worldY, 128 ), new( worldX, worldY + 128, 128 ), new( worldX, worldY + 128, 0 ), new( worldX, worldY, 0 ) );
			}
		}

		public override void Spawn()
		{
			Host.AssertClient();

			base.Spawn();

			//RenderBounds = new BBox( new Vector3( -12800, -12800, -12800 ), new Vector3( 12800, 12800, 12800 ) );

			// implying that the walls are created as soon as the level was made
			wallVB = new();
			wallVB.Init( true );

			AddWall( 0, 0, Level.Side.North | Level.Side.East | Level.Side.South | Level.Side.West );

			specialWallVB = new();
			specialWallVB.Init( true );

			ready = true;
		}

		public override void DoRender( SceneObject obj )
		{
			base.DoRender( obj );

			if ( !ready )
				return;

			Render.Set( "wallheight", Game.Current.WallHeight );

			//Render.CullMode = CullMode.Backface;
			wallVB.Draw( wallMaterial );

			//Render.CullMode = CullMode.FrontFace;
			specialWallVB.Draw( speciaWallMaterial );
		}
	}
}
