 #region Copyright & License Information
 /*
  * Copyright 2007-2015 The OpenRA Developers (see AUTHORS)
  * This file is part of OpenRA, which is free software. It is made
  * available to you under the terms of the GNU General Public License
  * as published by the Free Software Foundation. For more information,
  * see COPYING.
  */
 #endregion

using System;
using System.Drawing;
using System.Linq;
using OpenRA.Graphics;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Traits
{
	[Desc("Modifies the alpha channel for transparency. Attach this to the world actor.")]
	public class AlphaPaletteEffectInfo : ITraitInfo
	{
		[Desc("Ranges from 0-255.")]
		public readonly int Alpha = 255;

		[Desc("The palettes to modify.")]
		public readonly string[] Palettes = { };

		public object Create(ActorInitializer init) { return new AlphaPaletteEffect(init.Self, this); }
	}

	public class AlphaPaletteEffect : IPaletteModifier
	{
		readonly AlphaPaletteEffectInfo info;
		readonly int alpha;

		public AlphaPaletteEffect(Actor self, AlphaPaletteEffectInfo info)
		{
			this.info = info;
			alpha = info.Alpha.Clamp(0, 255);
		}

		public void AdjustPalette(IReadOnlyDictionary<string, MutablePalette> palettes)
		{
			foreach (var kvp in palettes)
			{
				if (!info.Palettes.Contains(kvp.Key))
					continue;

				var palette = kvp.Value;

				for (var i = 1; i < 256; i++)
				{
					var from = palette.GetColor(i);
					palette.SetColor(i, Color.FromArgb(alpha, from.R, from.G, from.B));
				}
			}
		}
	}
}
