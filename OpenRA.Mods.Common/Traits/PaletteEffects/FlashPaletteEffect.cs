#region Copyright & License Information
/*
 * Copyright 2007-2015 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see COPYING.
 */
#endregion

using System.Drawing;
using System.Linq;
using OpenRA.Graphics;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Traits
{
	[Desc("Used for stormy weather effects.")]
	class FlashPaletteEffectInfo : ITraitInfo
	{
		public readonly string[] ExcludePalettes = { "cursor", "chrome", "colorpicker", "fog", "shroud" };

		public readonly float LightningChance = 0.99f;

		public object Create(ActorInitializer init) { return new FlashPaletteEffect(this); }
	}

	class FlashPaletteEffect : IPaletteModifier, ITick
	{
		public bool Lightning = false;

		readonly FlashPaletteEffectInfo info;

		public FlashPaletteEffect(FlashPaletteEffectInfo info)
		{
			this.info = info;
		}

		public void Tick(Actor self)
		{
			Lightning = self.World.SharedRandom.NextFloat() > info.LightningChance;
		}

		public void AdjustPalette(IReadOnlyDictionary<string, MutablePalette> palettes)
		{
			foreach (var kvp in palettes)
			{
				if (info.ExcludePalettes.Contains(kvp.Key))
					continue;

				var palette = kvp.Value;

				for (var x = 0; x < 256; x++)
				{
					if (Lightning)
						palette.SetColor(x, Color.White);
				}
			}
		}
	}
}
