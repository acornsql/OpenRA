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
using System.Collections.Generic;
using System.Linq;
using OpenRA.Graphics;
using OpenRA.Mods.Common.Graphics;
using OpenRA.Widgets;

namespace OpenRA.Mods.Common.Widgets
{
	public class ActorPreviewWidget : Widget
	{
		public bool Animate = false;

		public IActorPreview[] Preview;

		protected readonly WorldRenderer worldRenderer;

		[ObjectCreator.UseCtor]
		public ActorPreviewWidget(WorldRenderer worldRenderer)
		{
			this.worldRenderer = worldRenderer;
		}

		protected ActorPreviewWidget(ActorPreviewWidget other)
			: base(other)
		{
			Preview = other.Preview;
			worldRenderer = other.worldRenderer;
		}

		public override Widget Clone() { return new ActorPreviewWidget(this); }

		IRenderable[] renderables;
		public override void PrepareRenderables()
		{
			var comparer = new RenderableComparer(worldRenderer);
			var pos = worldRenderer.Position(worldRenderer.Viewport.ViewToWorldPx(RenderOrigin + new int2(RenderBounds.Size.Width / 2, RenderBounds.Size.Height / 2)));

			renderables = Preview
				.SelectMany(p => p.Render(worldRenderer, pos))
				.OrderBy(r => r, comparer)
				.ToArray();

			for (var i = 0; i < renderables.Length; i++)
				renderables[i].BeforeRender(worldRenderer);
		}

		public override void Draw()
		{
			for (var i = 0; i < renderables.Length; i++)
				renderables[i].Render(worldRenderer);

			Game.Renderer.Flush();
		}

		public override void Tick()
		{
			if (Animate)
				foreach (var p in Preview)
					p.Tick();
		}
	}
}
