#region Copyright & License Information
/*
 * Copyright 2007-2014 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see COPYING.
 */
#endregion

using OpenRA.Graphics;
using OpenRA.Traits;
using OpenRA.Widgets;

namespace OpenRA.Mods.Common.Traits
{
	public class LoadWidgetAtGameStartInfo : ITraitInfo
	{
		public readonly string Widget = null;
		public readonly bool ClearRoot = true;
		public object Create(ActorInitializer init) { return new LoadWidgetAtGameStart(this); }
	}

	public class LoadWidgetAtGameStart : IWorldLoaded
	{
		readonly LoadWidgetAtGameStartInfo info;
		public LoadWidgetAtGameStart(LoadWidgetAtGameStartInfo info)
		{
			this.info = info;
		}

		public void WorldLoaded(World world, WorldRenderer wr)
		{
			// HACK to make shellmaps not load "MAINMENU" instead
			var widget = world.Paused && world.PauseStateLocked ? "INGAME_ROOT" : info.Widget;

			// Clear any existing widget state
			if (info.ClearRoot)
				Ui.ResetAll();

			Game.LoadWidget(world, widget, Ui.Root, new WidgetArgs());
		}
	}
}