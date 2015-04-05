#region Copyright & License Information
/*
 * Copyright 2007-2015 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see COPYING.
 */
#endregion

using System.Linq;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Traits
{
	[Desc("Attach this to the World actor.")]
	class ThunderInfo : ITraitInfo, Requires<FlashPaletteEffectInfo>
	{
		public readonly string[] SoundFiles = { };

		public object Create(ActorInitializer init) { return new Thunder(init, this); }
	}

	class Thunder : ITick
	{
		readonly ThunderInfo info;
		readonly FlashPaletteEffect flashPaletteEffect;

		public Thunder(ActorInitializer init, ThunderInfo info)
		{
			this.info = info;
			flashPaletteEffect = init.Self.Trait<FlashPaletteEffect>();
		}

		public void Tick(Actor self)
		{
			if (flashPaletteEffect.Lightning && info.SoundFiles.Any())
				Sound.Play(info.SoundFiles.Random(Game.CosmeticRandom));
		}
	}
}
