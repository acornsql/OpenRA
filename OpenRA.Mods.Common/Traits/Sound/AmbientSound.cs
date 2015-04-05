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
	[Desc("Play a looping audio.")]
	class AmbientSoundInfo : ITraitInfo
	{
		public readonly string SoundFile = "";

		public object Create(ActorInitializer init) { return new AmbientSound(this); }
	}

	class AmbientSound
	{
		public AmbientSound(AmbientSoundInfo info)
		{
			if (!string.IsNullOrEmpty(info.SoundFile))
				Sound.PlayLooped(info.SoundFile);
		}
	}
}
