using Sandbox;
using Sandbox.UI;

namespace Maze
{
	public partial class BillboardEntity : RenderEntity
	{
		public virtual Material SpriteMaterial { get; set; } = Material.Load( "materials/directx.vmat" );
		public float SpriteScale { get; set; } = 128f;

		public override void DoRender( SceneObject obj )
		{
			if ( Game.Current.State == Game.GameState.Invalid || Local.Pawn.Camera is not Camera camera )
				return;

			var vb = Render.GetDynamicVB();
			var normal = -camera.Rotation.Angles().Direction.Normal;
			var w = normal.Cross( Vector3.Down ).Normal;
			var h = normal.Cross( w ).Normal;
			var halfSpriteSize = SpriteScale / 2;

			// vb.AddQuad( new Ray( default, normal ), w * halfSpriteSize, h * halfSpriteSize ); // ffs why tf does it flip the plane
			Position = Position.WithZ( 64 * Game.Current.WallHeight );
			AddQuad( vb, new Ray( default, normal ), w * halfSpriteSize, h * halfSpriteSize * Game.Current.WallHeight );
			Render.CullMode = CullMode.FrontFace;
			vb.Draw( SpriteMaterial );
		}

		protected static void AddQuad( VertexBuffer self, Ray origin, Vector3 width, Vector3 height )
		{
			self.Default.Normal = origin.Direction;
			self.Default.Tangent = new Vector4( width.Normal, 1 );

			self.AddQuad(
				origin.Origin - width + height,
				origin.Origin + width + height,
				origin.Origin + width - height,
				origin.Origin - width - height
				);
		}
	}
}
