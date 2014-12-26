#region Copyright & License Information
/*
 * Copyright 2007-2014 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see COPYING.
 */
#endregion

using System.Collections.Generic;
using OpenRA.Graphics;

namespace OpenRA.Mods.Common.Traits
{
	[Desc("Invisible during games.")]
	class RenderEditorOnlyInfo : RenderSimpleInfo
	{
		public override object Create(ActorInitializer init) { return new RenderEditorOnly(init.self); }
	}

	class RenderEditorOnly : RenderSimple
	{
		public RenderEditorOnly(Actor self) : base(self) { }

		public override IEnumerable<IRenderable> Render(Actor self, WorldRenderer wr)
		{
			if (self.World.Type == WorldType.Editor)
				return base.Render(self, wr);
			else
				return SpriteRenderable.None;
		}
	}
}
