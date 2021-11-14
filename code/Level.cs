﻿using Sandbox;

namespace Maze
{
	public partial class Level : BaseNetworkable
	{
		public int Width => 16;
		public WallEntity Walls { get; internal set; }

		public Level()
		{
			//
		}

		public void Init()
		{
			Walls = new();
		}
	}
}
