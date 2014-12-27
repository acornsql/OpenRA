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
using System.IO;
using System.Linq;
using OpenRA.Widgets;

namespace OpenRA.Mods.RA.Widgets.Logic
{
	public class SaveMapLogic
	{
		[ObjectCreator.UseCtor]
		public SaveMapLogic(Widget widget, Action onExit, World world)
		{
			var newMap = world.Map;

			var title = widget.GetOrNull<TextFieldWidget>("TITLE");
			if (title != null)
				title.Text = newMap.Title;

			var description = widget.GetOrNull<TextFieldWidget>("DESCRIPTION");
			if (description != null)
				description.Text = newMap.Description;

			var author = widget.GetOrNull<TextFieldWidget>("AUTHOR");
			if (author != null)
				author.Text = newMap.Author;

			var visibilityDropdown = widget.GetOrNull<DropDownButtonWidget>("CLASS_DROPDOWN");
			if (visibilityDropdown != null)
			{
				var mapVisibility = new List<string>(Enum.GetNames(typeof(MapVisibility)));
				Func<string, ScrollItemWidget, ScrollItemWidget> setupItem = (option, template) =>
				{
					var item = ScrollItemWidget.Setup(template,
						() => visibilityDropdown.Text == option,
						() => { visibilityDropdown.Text = option; });
					item.Get<LabelWidget>("LABEL").GetText = () => option;
					return item;
				};
				visibilityDropdown.Text = Enum.GetName(typeof(MapVisibility), newMap.Visibility);
				visibilityDropdown.OnClick = () =>
					visibilityDropdown.ShowDropDown("LABEL_DROPDOWN_TEMPLATE", 210, mapVisibility, setupItem);
			}

			var userMapFolder = Game.modData.Manifest.MapFolders.First(f => f.Value == "User").Key;

			// Ignore optional flag
			if (userMapFolder.StartsWith("~"))
				userMapFolder = userMapFolder.Substring(1);

			var path = widget.GetOrNull<TextFieldWidget>("PATH");
			if (path != null)
				path.Text = Platform.ResolvePath(userMapFolder);

			var filename = widget.GetOrNull<TextFieldWidget>("FILENAME");
			if (filename != null)
				filename.Text = world.Map.Title.ToLower().Trim() + ".oramap";

			var close = widget.GetOrNull<ButtonWidget>("CLOSE");
			if (close != null)
				close.OnClick = () => { Ui.CloseWindow(); onExit(); };

			var save = widget.GetOrNull<ButtonWidget>("SAVE");
			if (save != null && !string.IsNullOrEmpty(path.Text))
			{
				save.OnClick = () => {
					newMap.Title = title.Text;
					newMap.Description = description.Text;
					newMap.Author = author.Text;
					newMap.Visibility = (MapVisibility)Enum.Parse(typeof(MapVisibility), visibilityDropdown.Text);
					var combinedPath = Path.Combine(path.Text, filename.Text);
					newMap.Save(combinedPath);
					Game.Debug("Saved current map as {0}", combinedPath);
				};
			}
		}
	}
}
