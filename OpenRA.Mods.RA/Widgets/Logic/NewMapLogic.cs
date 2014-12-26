#region Copyright & License Information
/*
 * Copyright 2007-2013 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see COPYING.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using OpenRA;
using OpenRA.FileFormats;
using OpenRA.Traits;
using OpenRA.Widgets;

namespace OpenRA.Mods.RA.Widgets.Logic
{
	public class NewMapLogic
	{
		Widget panel;

		[ObjectCreator.UseCtor]
		public NewMapLogic(Action onExit, Action<String> onSelect, Ruleset modRules, Widget widget, World world)
		{
			panel = widget;

			panel.Get<ButtonWidget>("CANCEL_BUTTON").OnClick = () => { Ui.CloseWindow(); onExit(); };

			var tilesetDropDown = panel.Get<DropDownButtonWidget>("TILESET");
			var tilesets = modRules.TileSets.Select(t => t.Key).ToList();
			Func<string, ScrollItemWidget, ScrollItemWidget> setupItem = (option, template) =>
			{
				var item = ScrollItemWidget.Setup(template,
					() => tilesetDropDown.Text == option,
					() => { tilesetDropDown.Text = option; });
				item.Get<LabelWidget>("LABEL").GetText = () => option;
				return item;
			};
			tilesetDropDown.Text = tilesets.First();
			tilesetDropDown.OnClick = () =>
				tilesetDropDown.ShowDropDown("LABEL_DROPDOWN_TEMPLATE", 210, tilesets, setupItem);

			var widthTextField = panel.Get<TextFieldWidget>("WIDTH");
			var heightTextField = panel.Get<TextFieldWidget>("WIDTH");

			panel.Get<ButtonWidget>("CREATE_BUTTON").OnClick = () =>
			{
				var tileset = modRules.TileSets[tilesetDropDown.Text];
				var map = Map.FromTileset(tileset);

				// TODO: forbid bad values
				int width;
				int.TryParse(widthTextField.Text, out width);
				int height;
				int.TryParse(heightTextField.Text, out height);
				map.Resize(width, height);
				var borderSize = width / 8; // TODO: unhardcode
				map.ResizeCordon(borderSize, borderSize, width-borderSize, height-borderSize);

				map.Players.Clear();
				map.MakeDefaultPlayers();

				map.FixOpenAreas(modRules);

				var mod = Game.modData.Manifest.Mod;
				map.RequiresMod = mod.Id;

				var mapDir = new[] { Platform.SupportDir, "maps", mod.Id }.Aggregate(Path.Combine); // TODO: unhardcode to MapFolders
				Directory.CreateDirectory(mapDir);
				var tempLocation = new [] { mapDir, "temp" }.Aggregate(Path.Combine) + ".oramap";
				map.Save(tempLocation); // TODO: load it right away and save later properly

				var newMap = new Map(tempLocation);
				Game.modData.MapCache[newMap.Uid].UpdateFromMap(newMap, MapClassification.User);

				ConnectionLogic.Connect(System.Net.IPAddress.Loopback.ToString(),
					Game.CreateLocalServer(newMap.Uid),
					"",
					() => { Game.LoadMapForEditing(newMap.Uid); },
					() => { Game.CloseServer(); onExit(); });
				onSelect(newMap.Uid);
			};
		}
	}
}
