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
	public class InstallVideosLogic
	{
		[ObjectCreator.UseCtor]
		public InstallVideosLogic(Widget widget, Ruleset modRules)
		{
			var installVideosContainer = widget.Get("INSTALL_FMV_PANEL");

			Action after = () =>
			{
				var args = new string[] { "Game.Mod=" + Game.Settings.Game.Mod };
				Game.InitializeWithMod(new Arguments(args));
			};

			var cancelButton = installVideosContainer.GetOrNull<ButtonWidget>("CANCEL_BUTTON");
			if (cancelButton != null)
			{
				cancelButton.OnClick = () =>
				{
					var args = new string[] { "Game.Mod=" + Game.Settings.Game.Mod };
					Game.InitializeWithMod(new Arguments(args));
				};
			}

			var ripFromDiscButton = installVideosContainer.GetOrNull<ButtonWidget>("RIP_FROM_CD_BUTTON");
			if (ripFromDiscButton != null)
			{
				ripFromDiscButton.OnClick = () =>
				{
					Ui.OpenWindow("INSTALL_FROMCD_PANEL", new WidgetArgs() {
						{ "continueLoading", after },
					});
				};
			}
		}
	}
}
