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
using System.Linq;
using OpenRA.Graphics;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Traits
{
	[Desc("Renders crates with both water and land variants.")]
	class WithCrateBodyInfo : ITraitInfo, Requires<RenderSpritesInfo>
	{
		public readonly string[] Images = { "crate" };

		[Desc("Easteregg sequences to use in december.")]
		public readonly string[] XmasImages = { };

		[SequenceReference] public readonly string IdleSequence = "idle";
		[SequenceReference] public readonly string WaterSequence;
		[SequenceReference] public readonly string LandSequence = "land";

		public object Create(ActorInitializer init) { return new WithCrateBody(init.Self, this); }
	}

	class WithCrateBody : INotifyParachuteLanded
	{
		readonly Actor self;
		readonly Animation anim;
		readonly WithCrateBodyInfo info;

		public WithCrateBody(Actor self, WithCrateBodyInfo info)
		{
			this.self = self;
			this.info = info;

			var rs = self.Trait<RenderSprites>();
			var images = info.XmasImages.Any() && DateTime.Today.Month == 12 ? info.XmasImages : info.Images;

			anim = new Animation(self.World, images.Random(Game.CosmeticRandom));
			anim.Play(info.IdleSequence);
			rs.Add("crate", anim);
		}

		public void OnLanded()
		{
			var seq = self.World.Map.GetTerrainInfo(self.Location).IsWater ? info.WaterSequence : info.LandSequence;
			anim.PlayRepeating(seq);
		}
	}
}
