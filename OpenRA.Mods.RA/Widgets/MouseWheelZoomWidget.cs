#region Copyright & License Information
/*
 * Copyright 2007-2014 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see COPYING.
 */
#endregion

using OpenRA.Graphics;
using OpenRA.Widgets;

namespace OpenRA.Mods.RA.Widgets
{
	public class MouseWheelZoomWidget : Widget
	{
		readonly WorldRenderer worldRenderer;

		[ObjectCreator.UseCtor]
		public MouseWheelZoomWidget(WorldRenderer worldRenderer)
		{
			this.worldRenderer = worldRenderer;
		}

		public override bool HandleMouseInput(MouseInput mi)
		{
			if (mi.Button == MouseButton.WheelUp)
				worldRenderer.Viewport.Zoom += 0.5f;

			if (mi.Button == MouseButton.WheelDown)
				worldRenderer.Viewport.Zoom -= 0.5f;

			return base.HandleMouseInput(mi);
		}
	}
}
