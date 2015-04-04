#region Copyright & License Information
/*
 * Copyright 2007-2015 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see COPYING.
 */
#endregion

using System.Collections.Generic;
using System.Linq;
using OpenRA.Graphics;
using OpenRA.Mods.Common.Graphics;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Traits
{
	[Desc("Play an animation when a unit exits or blocks the exit after production finished.")]
	class WithProductionDoorInfo : ITraitInfo, IRenderActorPreviewSpritesInfo, Requires<RenderSpritesInfo>, Requires<IBodyOrientationInfo>, Requires<BuildingInfo>
	{
		public readonly string IdleSequence = "idle-door";
		public readonly string BuildSequence = "build-door";

		public object Create(ActorInitializer init) { return new WithProductionDoor(init, this); }

		public IEnumerable<IActorPreview> RenderPreviewSprites(ActorPreviewInitializer init, RenderSpritesInfo rs, string image, int facings, PaletteReference p)
		{
			var anim = new Animation(init.World, image, () => 0);
			anim.PlayRepeating(IdleSequence);

			var bi = init.Actor.Traits.Get<BuildingInfo>();
			var offset = FootprintUtils.CenterOffset(init.World, bi).Y + 512; // Additional 512 units move from center -> top of cell
			yield return new SpriteActorPreview(anim, WVec.Zero, offset, p, rs.Scale);
		}
	}

	class WithProductionDoor : INotifyBuildComplete, ITick, INotifyProduction, INotifySold, ISync, INotifyDamageStateChanged
	{
		WithProductionDoorInfo info;
		RenderSprites renderSprites;
		Animation door;
		[Sync] bool isOpen;
		[Sync] CPos openExit;
		bool buildComplete;

		public WithProductionDoor(ActorInitializer init, WithProductionDoorInfo info)
		{
			this.info = info;
			renderSprites = init.Self.Trait<RenderSprites>();
			door = new Animation(init.World, renderSprites.GetImage(init.Self));
			var buildingInfo = init.Self.Info.Traits.Get<BuildingInfo>();

			var offset = FootprintUtils.CenterOffset(init.World, buildingInfo).Y + 512;
			renderSprites.Add("door", new AnimationWithOffset(door, null, () => !buildComplete, offset));
		}

		public void BuildingComplete(Actor self)
		{
			door.Play(RenderSprites.NormalizeSequence(door, self.GetDamageState(), info.IdleSequence));
			buildComplete = true;
		}

		public void Tick(Actor self)
		{
			if (isOpen && !self.World.ActorMap.GetUnitsAt(openExit).Any(a => a != self))
			{
				isOpen = false;
				door.PlayBackwardsThen(RenderSprites.NormalizeSequence(door, self.GetDamageState(), info.BuildSequence),
					() => door.Play(RenderSprites.NormalizeSequence(door, self.GetDamageState(), info.IdleSequence)));
			}
		}

		public void DamageStateChanged(Actor self, AttackInfo e)
		{
			if (door.CurrentSequence != null)
				door.ReplaceAnim(RenderSprites.NormalizeSequence(door, e.DamageState, door.CurrentSequence.Name));
		}

		public void UnitProduced(Actor self, Actor other, CPos exit)
		{
			door.PlayThen(RenderSprites.NormalizeSequence(door, self.GetDamageState(), info.BuildSequence), () => { isOpen = true; openExit = exit; });
		}

		public void Selling(Actor self) { renderSprites.Remove("door"); }
		public void Sold(Actor self) { }
	}
}
