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
using OpenRA.Mods.Common.Widgets;
using OpenRA.Traits;
using OpenRA.Widgets;

namespace OpenRA.Mods.Common.Traits
{
	public class LoadWidgetAtGameStartInfo : ITraitInfo
	{
		public readonly string IngameRoot = "INGAME_ROOT";
		public readonly string WorldRoot = "WORLD_ROOT";
		public readonly string PlayerRoot = "PLAYER_ROOT";
		public readonly string MenuRoot = "MENU_ROOT";

		public readonly string ShellmapWidget = "MAINMENU";
		public readonly string EditorWidgets = "EDITOR_WIDGETS";
		public readonly string ObserverWidgets = "OBSERVER_WIDGETS";
		public readonly string PlayerWidgets = "PLAYER_WIDGETS";

		public readonly string SidebarTicker = "SIDEBAR_TICKER";
		public readonly string ChatPanel = "CHAT_PANEL";
		public readonly string LeaveMapWidget = "LEAVE_MAP_WIDGET";

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
			// Clear any existing widget state
			if (info.ClearRoot)
				Ui.ResetAll();

			var widget = world.Type == WorldType.Shellmap ? info.ShellmapWidget : info.IngameRoot;
			Game.LoadWidget(world, widget, Ui.Root, new WidgetArgs());

			if (world.Type == WorldType.Editor)
			{
				var ingameRoot = Ui.Root.Get(info.IngameRoot);
				var worldRoot = ingameRoot.Get(info.WorldRoot);
				var playerRoot = worldRoot.Get(info.PlayerRoot);
				Game.LoadWidget(world, info.EditorWidgets, playerRoot, new WidgetArgs());
			}
			else if (world.Type == WorldType.Regular)
			{
				var ingameRoot = Ui.Root.Get(info.IngameRoot);
				var worldRoot = ingameRoot.Get(info.WorldRoot);
				var playerRoot = worldRoot.Get(info.PlayerRoot);

				if (world.LocalPlayer == null)
					Game.LoadWidget(world, info.ObserverWidgets, playerRoot, new WidgetArgs());
				else
				{
					var playerWidgets = Game.LoadWidget(world, info.PlayerWidgets, playerRoot, new WidgetArgs());
					var sidebarTicker = playerWidgets.Get<LogicTickerWidget>(info.SidebarTicker);

					sidebarTicker.OnTick = () =>
					{
						// Switch to observer mode after win/loss
						if (world.ObserveAfterWinOrLose && world.LocalPlayer.WinState != WinState.Undefined)
							Game.RunAfterTick(() =>
							{
								playerRoot.RemoveChildren();
								Game.LoadWidget(world, info.ObserverWidgets, playerRoot, new WidgetArgs());
							});
					};

					Game.LoadWidget(world, info.ChatPanel, worldRoot, new WidgetArgs());

					world.GameOver += () =>
					{
						var menuRoot = ingameRoot.Get(info.MenuRoot);
						worldRoot.RemoveChildren();
						menuRoot.RemoveChildren();
						Game.LoadWidget(world, info.LeaveMapWidget, menuRoot, new WidgetArgs());
					};
				}
			}
		}
	}
}