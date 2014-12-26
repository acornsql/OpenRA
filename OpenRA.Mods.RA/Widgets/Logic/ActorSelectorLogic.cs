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
using System.Linq;
using OpenRA.FileFormats;
using OpenRA.Graphics;
using OpenRA.Mods.Common;
using OpenRA.Mods.Common.Graphics;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Widgets;
using OpenRA.Mods.RA.Render;
using OpenRA.Traits;
using OpenRA.Widgets;
using OpenRA.Primitives;

namespace OpenRA.Mods.RA.Widgets.Logic
{
	public class ActorSelectorLogic
	{
		MapEditorWidget editor;
		Ruleset modRules;

		[ObjectCreator.UseCtor]
		public ActorSelectorLogic(Widget widget, World world, WorldRenderer worldRenderer, Ruleset modRules)
		{
			this.modRules = modRules;
			editor = widget.Parent.Get<MapEditorWidget>("MAP_EDITOR");

			var ownersDropDown = widget.Get<DropDownButtonWidget>("OWNERS_DROPDOWN");
			var ownernames = world.Map.Players.Values.Select(p => p.Name);
			Func<string, ScrollItemWidget, ScrollItemWidget> setupItem = (option, template) =>
			{
				var item = ScrollItemWidget.Setup(template,
					() => ownersDropDown.Text == option,
					() => { ownersDropDown.Text = option; editor.SelectedOwner = option; });
				item.Get<LabelWidget>("LABEL").GetText = () => option;
				return item;
			};
			ownersDropDown.OnClick = () =>
				ownersDropDown.ShowDropDown("LABEL_DROPDOWN_TEMPLATE", 270, ownernames, setupItem);
			ownersDropDown.Text = ownernames.First();
			editor.SelectedOwner = ownersDropDown.Text;

			IntializeActorPreview(widget, world, worldRenderer);
		}

		void IntializeActorPreview(Widget widget, World world, WorldRenderer worldRenderer)
		{
			var actorTemplateList = widget.Get<ScrollPanelWidget>("ACTORTEMPLATE_LIST");
			actorTemplateList.Layout = new GridLayout(actorTemplateList);
			var actorPreviewTemplate = actorTemplateList.Get<ScrollItemWidget>("ACTORPREVIEW_TEMPLATE");
			actorTemplateList.RemoveChildren();

			var actors = modRules.Actors.Where(a => !a.Value.Name.Contains('^') && !a.Value.Name.Contains('.'))
				.Select(a => a.Value);

			foreach (var a in actors)
			{
				var actor = a;
				var filter = actor.Traits.GetOrDefault<EditorTilesetFilterInfo>();
				if (filter != null)
				{
					if (filter.ExcludeTilesets != null && filter.ExcludeTilesets.Contains(world.TileSet.Id))
						continue;
					if (filter.RequireTilesets != null && !filter.RequireTilesets.Contains(world.TileSet.Id))
						continue;
				}

				var td = new TypeDictionary();
				td.Add(new FacingInit(92));

				var player = world.Players.FirstOrDefault(p => p.InternalName == editor.SelectedOwner) ?? world.Players.First();
				var init = new ActorPreviewInitializer(actor, player, worldRenderer, td);

				try
				{
					var preview = actor.Traits.WithInterface<IRenderActorPreviewInfo>()
						.SelectMany(rpi => rpi.RenderPreview(init))
						.ToArray();

					var newActorPreviewTemplate = ScrollItemWidget.Setup(actorPreviewTemplate,
						() => actor.Name == editor.SelectedActor,
						() => { editor.SelectedActor = actor.Name; editor.SelectedActorPreview = preview; });

					newActorPreviewTemplate.Bounds.X = 0;
					newActorPreviewTemplate.Bounds.Y = 0;

					var actorPreview = newActorPreviewTemplate.Get<ActorPreviewWidget>("ACTOR_PREVIEW");
					actorPreview.Preview = preview;

					var hackSize = new int2(50, 50);
					actorPreview.Bounds.Width = hackSize.X;
					actorPreview.Bounds.Height = hackSize.Y;
					newActorPreviewTemplate.Bounds.Width = hackSize.X + (actorPreview.Bounds.X * 2);
					newActorPreviewTemplate.Bounds.Height = hackSize.Y + (actorPreview.Bounds.Y * 2);
					newActorPreviewTemplate.IsVisible = () => true;
					actorTemplateList.AddChild(newActorPreviewTemplate);
				}
				catch
				{
					Log.Write("debug", "Map editor ignoring actor {0}, because of missing sprites for tileset {1}.",
						actor.Name, world.TileSet.Id);
					continue;
				}
			}
		}
	}
}
