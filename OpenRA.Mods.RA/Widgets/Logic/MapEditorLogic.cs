#region Copyright & License Information
/*
 * Copyright 2007-2014 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see COPYING.
 */

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using OpenRA.GameRules;
using OpenRA.Graphics;
using OpenRA.Widgets;

namespace OpenRA.Mods.RA.Widgets.Logic
{
	public class MapEditorLogic
	{
		[ObjectCreator.UseCtor]
		public MapEditorLogic(Widget widget, WorldRenderer worldRenderer)
		{
			var tabContainer = widget.Get("MAP_EDITOR_TAB_CONTAINER");
			var tilesTab = tabContainer.Get<ButtonWidget>("TILES_TAB");
			var overlaysTab = tabContainer.Get<ButtonWidget>("OVERLAYS_TAB");
			var actorsTab = tabContainer.Get<ButtonWidget>("ACTORS_TAB");

			var tileContainer = widget.Parent.Get<ContainerWidget>("TILE_WIDGETS");
			var layerContainer = widget.Parent.Get<ContainerWidget>("LAYER_WIDGETS");
			var actorContainer = widget.Parent.Get<ContainerWidget>("ACTOR_WIDGETS");

			tilesTab.OnClick = () =>
			{
				tileContainer.Visible = true;
				layerContainer.Visible = false;
				actorContainer.Visible = false;

				tilesTab.Highlighted = true;
				overlaysTab.Highlighted = false;
				actorsTab.Highlighted = false;
			};

			overlaysTab.OnClick = () =>
			{
				tileContainer.Visible = false;
				layerContainer.Visible = true;
				actorContainer.Visible = false;

				tilesTab.Highlighted = false;
				overlaysTab.Highlighted = true;
				actorsTab.Highlighted = false;
			};

			actorsTab.OnClick = () =>
			{
				tileContainer.Visible = false;
				layerContainer.Visible = false;
				actorContainer.Visible = true;

				tilesTab.Highlighted = false;
				overlaysTab.Highlighted = false;
				actorsTab.Highlighted = true;
			};
		}
	}
}
