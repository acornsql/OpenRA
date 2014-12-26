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
using OpenRA.Traits;
using OpenRA.Widgets;

namespace OpenRA.Mods.RA.Widgets.Logic
{
	public class LayerSelectorLogic
	{
		MapEditorWidget editor;
		Ruleset modRules;
		World world;

		[ObjectCreator.UseCtor]
		public LayerSelectorLogic(Widget widget, World world, Ruleset modRules)
		{
			this.modRules = modRules;
			this.world = world;

			editor = widget.Parent.Get<MapEditorWidget>("MAP_EDITOR");
			IntializeLayerPreview(widget);
		}

		void IntializeLayerPreview(Widget widget)
		{
			var layerTemplateList = widget.Get<ScrollPanelWidget>("LAYERTEMPLATE_LIST");
 			layerTemplateList.Layout = new GridLayout(layerTemplateList);
 			var layerPreviewTemplate = layerTemplateList.Get<ScrollItemWidget>("LAYERPREVIEW_TEMPLATE");
 			layerTemplateList.RemoveChildren();

			var resources = modRules.Actors["world"].Traits.WithInterface<ResourceTypeInfo>();
			foreach (var resource in resources)
			{
				var newResourcePreviewTemplate = ScrollItemWidget.Setup(layerPreviewTemplate,
					() => resource == editor.SelectedResourceTypeInfo,
					() => editor.SelectedResourceTypeInfo = resource);
				newResourcePreviewTemplate.Bounds.X = 0;
				newResourcePreviewTemplate.Bounds.Y = 0;
				var layerPreview = newResourcePreviewTemplate.Get<SpriteWidget>("LAYER_PREVIEW");
				var variant = resource.Variants.FirstOrDefault();
				var sequenceProvier = modRules.Sequences[world.TileSet.Id];
				var sequence = sequenceProvier.GetSequence("resources", variant);
				var frame = sequence.Frames != null ? sequence.Frames.Last() : resource.MaxDensity - 1;
				layerPreview.GetSprite = () => sequence.GetSprite(frame);
				layerPreview.IsVisible = () => true;
				var tileWidth = Game.modData.Manifest.TileSize.Width;
				var tileHeight = Game.modData.Manifest.TileSize.Height;
				layerPreview.Bounds.Width = tileWidth;
				layerPreview.Bounds.Height = tileHeight;
				newResourcePreviewTemplate.Bounds.Width = tileWidth + (layerPreview.Bounds.X * 2);
				newResourcePreviewTemplate.Bounds.Height = tileHeight + (layerPreview.Bounds.Y * 2);
				newResourcePreviewTemplate.IsVisible = () => true;
				layerTemplateList.AddChild(newResourcePreviewTemplate);
			}
		}
	}
}
