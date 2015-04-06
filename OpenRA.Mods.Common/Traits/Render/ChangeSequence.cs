#region Copyright & License Information
/*
 * Copyright 2007-2015 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see COPYING.
 */
#endregion

using OpenRA.Traits;

namespace OpenRA.Mods.Common.Traits
{
	class ChangeSequenceInfo : UpgradableTraitInfo, Requires<RenderSimpleInfo>, ITraitInfo
	{
		public readonly string InitialSequence = "";
		public readonly string TransitionalSequence = "";
		public readonly string FinalSequence = "";

		public object Create(ActorInitializer init) { return new ChangeSequence(this, init.Self); }
	}

	class ChangeSequence : UpgradableTrait<ChangeSequenceInfo>
	{
		readonly ChangeSequenceInfo info;
		readonly RenderSimple renderSimple;

		public ChangeSequence(ChangeSequenceInfo info, Actor self) :
			base(info)
		{
			this.info = info;
			renderSimple = self.Trait<RenderSimple>();
		}

		protected override void UpgradeEnabled(Actor self)
		{
			renderSimple.DefaultAnimation.PlayThen(info.TransitionalSequence,
				() => renderSimple.DefaultAnimation.PlayRepeating(info.FinalSequence));
		}

		protected override void UpgradeDisabled(Actor self)
		{
			renderSimple.DefaultAnimation.PlayThen(info.TransitionalSequence,
				() => renderSimple.DefaultAnimation.PlayRepeating(info.InitialSequence));
		}

	}
}
