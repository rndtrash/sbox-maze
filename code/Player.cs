using Sandbox;
using System;
using System.Threading.Tasks;

namespace Maze
{
	public partial class Player : AnimEntity
	{
		enum State
		{
			Shittin,
			Poopin,
			Moving,
			Flipping
		};

		public float Height => 64f;

		public bool OnGround { get; internal set; } = true;
		public bool StopMovement { get; internal set; } = false;
		public float TargetRoll { get; internal set; } = 0.0f;

		[Net, Predicted]
		public PawnController Controller { get; set; }

		[Net, Predicted]
		public PawnController DevController { get; set; }

		[AdminCmd( "maze_forceflip" )]
		public async static void ForceFlip()
		{
			Log.Info( "alright" );
			await (Local.Pawn as Player).Flip();
		}

		public async Task Flip()
		{
			//StopMovement = true;
			TargetRoll = 180.0f;
			await Task.Delay( 1000 );
			StopMovement = false;
		}

		public virtual PawnController GetActiveController()
		{
			if ( DevController != null ) return DevController;

			return Controller;
		}

		public override void Simulate( Client cl )
		{
			var controller = GetActiveController();
			controller?.Simulate( cl, this, null );

			//EyePos = EyePos.WithX( MathF.Sin( Time.Now / 2 * MathF.PI ) * 64 ).WithY( MathF.Cos( Time.Now / 2 * MathF.PI ) * 64 );
			//EyeRot = Rotation.FromYaw(EyeRot.Yaw() + 30 * Time.Delta );
		}

		public override void FrameSimulate( Client cl )
		{
			base.FrameSimulate( cl );

			var controller = GetActiveController();
			controller?.FrameSimulate( cl, this, null );
		}

		public override void OnKilled()
		{
			return;
		}

		public virtual void Respawn()
		{
			Host.AssertServer();

			Velocity = Vector3.Zero;

			ResetInterpolation();

			Transform = Transform.Zero;
			Position = new Vector3(64, 64, 64);

			Controller = new NoclipController();

			Camera = new FirstPersonCamera();

			EnableAllCollisions = true;
			EnableDrawing = true;
			EnableHideInFirstPerson = true;
			EnableShadowInFirstPerson = true;
		}

		/// <summary>
		/// Called from the gamemode, clientside only.
		/// </summary>
		public override void BuildInput( InputBuilder input )
		{
			if ( input.StopProcessing )
				return;

			ActiveChild?.BuildInput( input );

			GetActiveController()?.BuildInput( input );

			if ( input.StopProcessing )
				return;
		}


		/// <summary>
		/// Called after the camera setup logic has run. Allow the player to
		/// do stuff to the camera, or using the camera. Such as positioning entities
		/// relative to it, like viewmodels etc.
		/// </summary>
		public override void PostCameraSetup( ref CameraSetup setup )
		{
			Host.AssertClient();

			if ( ActiveChild != null )
			{
				ActiveChild.PostCameraSetup( ref setup );
			}
		}

		public override void StartTouch( Entity other )
		{
			if ( IsClient ) return;

			if ( other is PickupTrigger )
			{
				StartTouch( other.Parent );
				return;
			}
		}
	}
}
