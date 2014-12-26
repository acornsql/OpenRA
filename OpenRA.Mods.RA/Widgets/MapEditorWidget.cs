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
using System.Drawing;
using System.Linq;
using OpenRA.FileFormats;
using OpenRA.Graphics;
using OpenRA.Mods.Common;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Graphics;
using OpenRA.Mods.Common.Widgets;
using OpenRA.Mods.RA.Traits;
using OpenRA.Orders;
using OpenRA.Traits;
using OpenRA.Widgets;

namespace OpenRA.Mods.RA.Widgets
{
	public class MapEditorWidget : Widget
	{
		public ushort SelectedTileId;
		public ResourceTypeInfo SelectedResourceTypeInfo;
		public string SelectedActor;
		public string SelectedOwner;
		public IActorPreview[] SelectedActorPreview;

		World world;
		WorldRenderer worldRenderer;
		Ruleset modRules;

		TerrainTemplatePreviewWidget dragTilePreview;
		SpriteWidget dragLayerPreview;
		ActorPreviewWidget dragActorPreview;

		ResourceLayer resourceLayer;

		[ObjectCreator.UseCtor]
		public MapEditorWidget(World world, WorldRenderer worldRenderer, Ruleset modRules)
		{
			this.world = world;
			this.worldRenderer = worldRenderer;
			this.modRules = modRules;

			dragTilePreview = Ui.Root.Get<TerrainTemplatePreviewWidget>("DRAG_TILE_PREVIEW");
			dragLayerPreview = Ui.Root.Get<SpriteWidget>("DRAG_LAYER_PREVIEW");
			dragTilePreview.GetScale = () => worldRenderer.Viewport.Zoom;

			dragActorPreview = Ui.Root.Get<ActorPreviewWidget>("DRAG_ACTOR_PREVIEW");
			resourceLayer = world.WorldActor.Trait<ResourceLayer>();
		}

		ushort draggedTileId;
		ResourceTypeInfo draggedResourceTypeInfo;
		string draggedActor;
		Rectangle draggedTemplateBounds;

		public override bool HandleMouseInput(MouseInput mi)
		{
			if (mi.Event == MouseInputEvent.Up)
				return false;

			var mouseWorldPixel = worldRenderer.Viewport.ViewToWorldPx(mi.Location);
			var mouseWorldPosition = worldRenderer.Position(mouseWorldPixel);
			var cell = world.Map.CellContaining(mouseWorldPosition);

			var underCursor = world.ScreenMap.ActorsAt(mi).FirstOrDefault();

			if (draggedTileId != SelectedTileId)
			{
				draggedTileId = SelectedTileId;
				dragTilePreview.Template = world.TileSet.Templates.First(t => t.Value.Id == draggedTileId).Value;
				draggedTemplateBounds = worldRenderer.Theater.TemplateBounds(dragTilePreview.Template, Game.modData.Manifest.TileSize, world.Map.TileShape);
				dragTilePreview.Visible = true;
				dragLayerPreview.Visible = false;
				dragActorPreview.Visible = false;
			}

			if (draggedResourceTypeInfo != SelectedResourceTypeInfo)
			{
				draggedResourceTypeInfo = SelectedResourceTypeInfo;
				var variant = draggedResourceTypeInfo.Variants.FirstOrDefault();
				var sequenceProvier = modRules.Sequences[world.TileSet.Id];
				var sequence = sequenceProvier.GetSequence("resources", variant);
				var frame = sequence.Frames != null ? sequence.Frames.Last() : draggedResourceTypeInfo.MaxDensity - 1;
				dragLayerPreview.GetSprite = () => sequence.GetSprite(frame);
				dragLayerPreview.Visible = true;
				dragTilePreview.Visible = false;
				dragActorPreview.Visible = false;
			}

			if (draggedActor != SelectedActor)
			{
				draggedActor = SelectedActor;
				dragActorPreview.Preview = SelectedActorPreview;
				dragTilePreview.Visible = false;
				dragLayerPreview.Visible = false;
				dragActorPreview.Visible = true;
			}

			if (mi.Button == MouseButton.Right)
			{
				dragTilePreview.Visible = false;
				dragLayerPreview.Visible = false;
				dragActorPreview.Visible = false;

				if (underCursor != null)
				{
					var key = world.Map.Actors.Value.FirstOrDefault(a =>
						a.Value.InitDict.Get<LocationInit>().value == underCursor.Location);
					if (key.Key != null)
						world.Map.Actors.Value.Remove(key.Key);

					if (underCursor.Info.Name == "mpspawn" && world.Map.Players.Any(p => p.Key.StartsWith("Multi")))
						world.Map.Players.Remove(world.Map.Players.Last().Key);

					underCursor.Destroy();
				}

				if (world.Map.MapResources.Value[cell].Type != 0)
				{
					world.Map.MapResources.Value[cell] = new ResourceTile();
					resourceLayer.Destroy(cell);
					resourceLayer.Update();
				}
			}

			if (dragTilePreview.Visible || dragLayerPreview.Visible || dragActorPreview.Visible)
			{
				if (world.Map.Contains(cell))
				{
					if (mi.Button == MouseButton.Left)
					{
						var random = new Random();

						if (dragTilePreview.Visible)
						{
							var tileset = modRules.TileSets[world.Map.Tileset];
							var template = tileset.Templates.First(t => t.Value.Id == draggedTileId).Value;
							if (!(template.Size.Length > 1 && mi.Event == MouseInputEvent.Move))
							{
								var i = 0;
								for (var y = 0; y < template.Size.Y; y++)
								{
									for (var x = 0; x < template.Size.X; x++)
									{
										if (template.Contains(i) && template[i] != null)
										{
											var index = template.PickAny ? (byte)random.Next(0, template.TilesCount) : (byte)i;
											world.Map.MapTiles.Value[cell + new CVec(x, y)] = new TerrainTile(draggedTileId, index);
											world.Map.MapHeight.Value[cell + new CVec(x, y)] = (byte)(world.Map.MapHeight.Value[cell + new CVec(x, y)] + template[index].Height);
										}

										i++;
									}
								}
							}
						}

						if (dragLayerPreview.Visible)
						{
							var type = (byte)SelectedResourceTypeInfo.ResourceType;
							var index = (byte)random.Next(SelectedResourceTypeInfo.MaxDensity);
							world.Map.MapResources.Value[cell] = new ResourceTile(type, index);
							resourceLayer.Update();
						}

						if (dragActorPreview.Visible && mi.Event == MouseInputEvent.Down)
						{
							var newActorReference = new ActorReference(SelectedActor);
							newActorReference.Add(new LocationInit(cell));
							newActorReference.Add(new OwnerInit(SelectedOwner));
							var initDict = newActorReference.InitDict;

							if (!initDict.Contains<SkipMakeAnimsInit>())
								initDict.Add(new SkipMakeAnimsInit());

							world.CreateActor(SelectedActor, initDict);
							var actorName = NextActorName();
							world.Map.Actors.Value.Add(actorName, newActorReference);
							if (SelectedActor == "mpspawn")
							{
								world.Map.MakeDefaultPlayers();
								var player = new Player(world, null, null, world.Map.Players.Last().Value);
								world.AddPlayer(player);
								var fakePaletteName = "player" + player.InternalName;
								if (!worldRenderer.PaletteExists(fakePaletteName))
									worldRenderer.AddPalette(fakePaletteName, new ImmutablePalette(worldRenderer.Palette("player").Palette), true);
							}
						}
					}

					var center = world.Map.CenterOfCell(cell);
					var cellScreenPosition = worldRenderer.ScreenPxPosition(center);
					var cellScreenPixel = worldRenderer.Viewport.WorldToViewPx(cellScreenPosition);

					var zoom = worldRenderer.Viewport.Zoom;
					dragTilePreview.Bounds.X = cellScreenPixel.X + (int)(zoom * draggedTemplateBounds.X);
					dragTilePreview.Bounds.Y = cellScreenPixel.Y + (int)(zoom * draggedTemplateBounds.Y);
					dragTilePreview.Bounds.Width = (int)(zoom * draggedTemplateBounds.Width);
					dragTilePreview.Bounds.Height = (int)(zoom * draggedTemplateBounds.Height);

					cellScreenPosition = worldRenderer.ScreenPxPosition(center);
					cellScreenPixel = worldRenderer.Viewport.WorldToViewPx(cellScreenPosition);

					dragLayerPreview.Bounds.X = cellScreenPixel.X;
					dragLayerPreview.Bounds.Y = cellScreenPixel.Y;

					dragActorPreview.Bounds.X = cellScreenPixel.X;
					dragActorPreview.Bounds.Y = cellScreenPixel.Y;
				}
			}

			return true;
		}

		public override bool HandleKeyPress(KeyInput e)
		{
			Console.WriteLine("Got key " + e.Key);
			if (e.Key == Keycode.NUMBER_0 || e.Key == Keycode.NUMBER_9)
			{
				if (e.Event != KeyInputEvent.Down)
					return true;

				var mouseWorldPixel = worldRenderer.Viewport.ViewToWorldPx(Viewport.LastMousePos);
				var mouseWorldPosition = worldRenderer.Position(mouseWorldPixel);
				var cell = world.Map.CellContaining(mouseWorldPosition);
				var delta = e.Key == Keycode.NUMBER_0 ? 1 : -1;

				var height = world.Map.MapHeight.Value[cell];
				if ((height > 0 || delta > 0) && (height < 254 || delta < 0))
					world.Map.MapHeight.Value[cell] = (byte)(height + delta);

				Console.WriteLine("height at {0} is now {1}. Delta was {2}", cell, world.Map.MapHeight.Value[cell], delta);

				return true;
			}

			return false;
		}

		string NextActorName()
		{
			var id = world.Actors.Count();
			var possibleName = "Actor" + id.ToString();

			while(world.Map.Actors.Value.ContainsKey(possibleName))
			{
				id++;
				possibleName = "Actor" + id.ToString();
			}

			return possibleName;
		}
	}
}
