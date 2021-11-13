
using Sandbox;

namespace Maze
{
	public partial class Game : GameBase
	{
		public enum GameState
		{
			Invalid,
			Intro,
			InGame,
			Outro
		}
		public float IntroDelay => 3.5f;

		public static Game Current { get; protected set; }
		[Net, Change] public Level Level { get; internal set; }
		[Net, Change] public GameState State { get; internal set; }

		public Game()
		{
			Current = this;

			Transmit = TransmitType.Always;

			if ( IsServer )
			{
				Level = new();
				Log.Info( $"{Level == null}" );
			}
			else
			{
				_ = new EnvEntity();
			}
		}

		public void OnLevelChanged()
		{
			Host.AssertClient();

			Level.Init();
			Sound.FromScreen( "intro" );

			Event.Run( "maze.level" );
		}

		public void OnStateChanged()
		{
			Host.AssertClient();

			Event.Run( "maze.state", State );
		}

		public override void Spawn()
		{
			base.Spawn();

			State = GameState.Intro;
		}

		/// <summary>
		/// A client has joined the server. Make them a pawn to play with
		/// </summary>
		public override void ClientJoined( Client client )
		{
			var player = new Maze.Player();
			client.Pawn = player;

			player.Respawn();

			Log.Info( $"player {player.Position}" );
		}

		/// <summary>
		/// Called when the game is shutting down
		/// </summary>
		public override void Shutdown()
		{
			if ( Current == this )
				Current = null;
		}

		/// <summary>
		/// Client has disconnected from the server. Remove their entities etc.
		/// </summary>
		public override void ClientDisconnect( Client cl, NetworkDisconnectionReason reason )
		{
			if ( cl.Pawn.IsValid() )
			{
				cl.Pawn.Delete();
				cl.Pawn = null;
			}

		}

		/// <summary>
		/// Called each tick.
		/// Serverside: Called for each client every tick
		/// Clientside: Called for each tick for local client. Can be called multiple times per tick.
		/// </summary>
		public override void Simulate( Client cl )
		{
			if ( !cl.Pawn.IsValid() ) return;

			// Block Simulate from running clientside
			// if we're not predictable.
			if ( !cl.Pawn.IsAuthority ) return;

			cl.Pawn.Simulate( cl );
		}

		/// <summary>
		/// Called each frame on the client only to simulate things that need to be updated every frame. An example
		/// of this would be updating their local pawn's look rotation so it updates smoothly instead of at tick rate.
		/// </summary>
		public override void FrameSimulate( Client cl )
		{
			Host.AssertClient();

			if ( !cl.Pawn.IsValid() ) return;

			// Block Simulate from running clientside
			// if we're not predictable.
			if ( !cl.Pawn.IsAuthority ) return;

			cl.Pawn?.FrameSimulate( cl );
		}

		/// <summary>
		/// Should we send voice data to this player
		/// </summary>
		public override bool CanHearPlayerVoice( Client source, Client dest )
		{
			Host.AssertServer();

			return false;
		}

		/// <summary>
		/// Which camera should we be rendering from?
		/// </summary>
		public virtual ICamera FindActiveCamera()
		{
			if ( Local.Client.DevCamera != null ) return Local.Client.DevCamera;
			if ( Local.Client.Camera != null ) return Local.Client.Camera;
			if ( Local.Pawn != null ) return Local.Pawn.Camera;

			return null;
		}

		[Predicted]
		public Camera LastCamera { get; set; }

		/// <summary>
		/// Called to set the camera up, clientside only.
		/// </summary>
		public override CameraSetup BuildCamera( CameraSetup camSetup )
		{
			var cam = FindActiveCamera();

			if ( LastCamera != cam )
			{
				LastCamera?.Deactivated();
				LastCamera = cam as Camera;
				LastCamera?.Activated();
			}

			cam?.Build( ref camSetup );

			PostCameraSetup( ref camSetup );

			return camSetup;
		}

		/// <summary>
		/// Clientside only. Called every frame to process the input.
		/// The results of this input are encoded\ into a user command and
		/// passed to the PlayerController both clientside and serverside.
		/// This routine is mainly responsible for taking input from mouse/controller
		/// and building look angles and move direction.
		/// </summary>
		public override void BuildInput( InputBuilder input )
		{
			Event.Run( "buildinput", input );

			// the camera is the primary method here
			LastCamera?.BuildInput( input );

			Local.Pawn?.BuildInput( input );
		}

		/// <summary>
		/// Called after the camera setup logic has run. Allow the gamemode to 
		/// do stuff to the camera, or using the camera. Such as positioning entities 
		/// relative to it, like viewmodels etc.
		/// </summary>
		public override void PostCameraSetup( ref CameraSetup camSetup )
		{
			if ( Local.Pawn != null )
			{
				// VR anchor default is at the pawn's location
				VR.Anchor = Local.Pawn.Transform;

				Local.Pawn.PostCameraSetup( ref camSetup );
			}

			//
			// Position any viewmodels
			//
			BaseViewModel.UpdateAllPostCamera( ref camSetup );

			CameraModifier.Apply( ref camSetup );
		}

		/// <summary>
		/// Called right after the level is loaded and all entities are spawned.
		/// </summary>
		public override void PostLevelLoaded()
		{
			return;
		}

		public override void OnVoicePlayed( long playerId, float level )
		{
			return;
		}
	}

}
