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
using System.Drawing;
using System.Linq;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Widgets;
using OpenRA.Mods.D2k.Widgets;
using OpenRA.Mods.RA;
using OpenRA.Mods.RA.Widgets;
using OpenRA.Mods.RA.Widgets.Logic;
using OpenRA.Traits;
using OpenRA.Widgets;

namespace OpenRA.Mods.D2k.Widgets.Logic
{
	public class PowerBarLogic
	{
		[ObjectCreator.UseCtor]
		public PowerBarLogic(Widget widget, World world)
		{
			var powerManager = world.LocalPlayer.PlayerActor.Trait<PowerManager>();
			var powerBar = widget.Get<ResourceBarWidget>("POWERBAR");
			powerBar.IndicatorCollection = "power-" + world.LocalPlayer.Country.Race;
			powerBar.GetProvided = () => powerManager.PowerProvided;
			powerBar.GetUsed = () => powerManager.PowerDrained;
			powerBar.TooltipFormat = "Power Usage: {0}/{1}";
			powerBar.GetBarColor = () =>
			{
				if (powerManager.PowerState == PowerState.Critical)
					return Color.Red;
				if (powerManager.PowerState == PowerState.Low)
					return Color.Orange;
				return Color.LimeGreen;
			};
		}
	}
}
