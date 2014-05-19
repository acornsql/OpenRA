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
using System.IO;
using System.Linq;
using OpenRA.FileSystem;
using OpenRA.Widgets;

namespace OpenRA.Mods.RA.Widgets.Logic
{
	public class InstallMusicLogic
	{
		[ObjectCreator.UseCtor]
		public InstallMusicLogic(Widget widget, Ruleset modRules)
		{
			var installMusicContainer = widget.Get("INSTALL_MUSIC_PANEL");

			Action after = () =>
			{
				var args = new string[] { "Game.Mod=" + Game.Settings.Game.Mod };
				Game.InitializeWithMod(new Arguments(args));
			};

			var cancelButton = installMusicContainer.GetOrNull<ButtonWidget>("CANCEL_BUTTON");
			if (cancelButton != null)
			{
				cancelButton.OnClick = () =>
				{
					var args = new string[] { "Game.Mod=" + Game.Settings.Game.Mod };
					Game.InitializeWithMod(new Arguments(args));
				};
			}

			var ripFromDiscButton = installMusicContainer.GetOrNull<ButtonWidget>("RIP_FROM_CD_BUTTON");
			if (ripFromDiscButton != null)
			{
				ripFromDiscButton.OnClick = () =>
				{
					Ui.OpenWindow("INSTALL_FROMCD_PANEL", new WidgetArgs() {
						{ "continueLoading", after },
					});
				};
			}

			var downloadButton = installMusicContainer.GetOrNull<ButtonWidget>("DOWNLOAD_BUTTON");
			if (downloadButton != null)
			{
				var installData = Game.modData.Manifest.ContentInstaller;
				downloadButton.IsVisible = () => !string.IsNullOrEmpty(installData["MusicPackageMirrorList"]);
				var musicInstallData = new Dictionary<string, string> { };
				musicInstallData["PackageMirrorList"] =  installData["MusicPackageMirrorList"];
				downloadButton.OnClick = () =>
				{
					Ui.OpenWindow("INSTALL_DOWNLOAD_PANEL", new WidgetArgs() {
						{ "afterInstall", after },
						{ "installData", musicInstallData },
					});
				};
			}
		}
	}
}
