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
using OpenRA.Mods.RA.Activities;
using OpenRA.Mods.RA.Traits;
using OpenRA.Mods.RA.Buildings;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.RA
{
	public class CrateSpawnerInfo : ITraitInfo
	{
		[Desc("Minimum number of crates")]
		public readonly int Minimum = 1;

		[Desc("Maximum number of crates")]
		public readonly int Maximum = 255;

		[Desc("Average time (seconds) between crate spawn")]
		public readonly int SpawnInterval = 180;

		[Desc("Which terrain types can we drop on?")]
		public readonly string[] ValidGround = { "Clear", "Rough", "Road", "Ore", "Beach" };

		[Desc("Which terrain types count as water?")]
		public readonly string[] ValidWater = { "Water" };

		[Desc("Chance of generating a water crate instead of a land crate")]
		public readonly float WaterChance = .2f;

		[ActorReference]
		[Desc("Crate actors to drop")]
		public readonly string[] CrateActors = { "crate" };

		[Desc("Chance of each crate actor spawning")]
		public readonly int[] CrateActorShares = { 10 };

		[ActorReference]
		[Desc("If a DeliveryAircraft: is specified, then this actor will deliver crates")]
		public readonly string DeliveryAircraft = null;

		[Desc("Number of facings that the delivery aircraft may approach from.")]
		public readonly int QuantizedFacings = 32;

		[Desc("Spawn and remove the plane this far outside the map.")]
		public readonly WRange Cordon = new WRange(5120);

		public object Create(ActorInitializer init) { return new CrateSpawner(this, init.self); }
	}

	public class CrateSpawner : ITick
	{
		int crates = 0;
		int ticks = 0;
		CrateSpawnerInfo info;
		Actor self;

		public CrateSpawner(CrateSpawnerInfo info, Actor self)
		{
			this.info = info;
			this.self = self;
		}

		public void Tick(Actor self)
		{
			if (!self.World.LobbyInfo.GlobalSettings.Crates)
				return;

			if (--ticks <= 0)
			{
				ticks = info.SpawnInterval * 25;

				var toSpawn = Math.Max(0, info.Minimum - crates)
					+ (crates < info.Maximum ? 1 : 0);

				for (var n = 0; n < toSpawn; n++)
					SpawnCrate(self);
			}
		}

		void SpawnCrate(Actor self)
		{
			var threshold = 100;
			var inWater = self.World.SharedRandom.NextFloat() < info.WaterChance;
			var pp = ChooseDropCell(self, inWater, threshold);

			if (pp == null)
				return;

			var p = pp.Value;
			var crateActor = ChooseCrateActor();

			self.World.AddFrameEndTask(w =>
			{
				if (info.DeliveryAircraft != null)
				{
					var crate = w.CreateActor(false, crateActor, new TypeDictionary { new OwnerInit(w.WorldActor.Owner) });
					var dropFacing = Util.QuantizeFacing(self.World.SharedRandom.Next(256), info.QuantizedFacings) * (256 / info.QuantizedFacings);
					var delta = new WVec(0, -1024, 0).Rotate(WRot.FromFacing(dropFacing));

					var altitude = self.World.Map.Rules.Actors[info.DeliveryAircraft].Traits.Get<PlaneInfo>().CruiseAltitude.Range;
					var target = self.World.Map.CenterOfCell(p) + new WVec(0, 0, altitude);
					var startEdge = target - (self.World.Map.DistanceToEdge(target, -delta) + info.Cordon).Range * delta / 1024;
					var finishEdge = target + (self.World.Map.DistanceToEdge(target, delta) + info.Cordon).Range * delta / 1024;

					var plane = w.CreateActor(info.DeliveryAircraft, new TypeDictionary
					{
						new CenterPositionInit(startEdge + new WVec(0, 0, altitude)),
						new OwnerInit(self.Owner),
						new FacingInit(dropFacing),
					});

					var drop = plane.Trait<ParaDrop>();
					drop.SetLZ(p, true);
					plane.Trait<Cargo>().Load(plane, crate);

					plane.CancelActivity();
					plane.QueueActivity(new Fly(plane, Target.FromPos(finishEdge)));
					plane.QueueActivity(new RemoveSelf());
				}
				else
				{
					w.CreateActor(crateActor, new TypeDictionary { new OwnerInit(w.WorldActor.Owner), new LocationInit(p) });
				}
			});
		}

		CPos? ChooseDropCell(Actor self, bool inWater, int maxTries)
		{
			for (var n = 0; n < maxTries; n++)
			{
				var p = self.World.Map.ChooseRandomCell(self.World.SharedRandom);

				// Is this valid terrain?
				var terrainType = self.World.Map.GetTerrainInfo(p).Type;
				if (!(inWater ? info.ValidWater : info.ValidGround).Contains(terrainType))
					continue;

				// Don't drop on any actors
				if (self.World.WorldActor.Trait<BuildingInfluence>().GetBuildingAt(p) != null
					|| self.World.ActorMap.GetUnitsAt(p).Any())
					continue;

				return p;
			}

			return null;
		}

		string ChooseCrateActor()
		{
			var crateShares = info.CrateActorShares;
			var n = self.World.SharedRandom.Next(crateShares.Sum());

			var cumulativeShares = 0;
			for (var i = 0; i < crateShares.Length; i++)
			{
				cumulativeShares += crateShares[i];
				if (n <= cumulativeShares)
					return info.CrateActors[i];
			}

			return null;
		}

		public void IncrementCrates()
		{
			crates++;
		}

		public void DecrementCrates()
		{
			crates--;
		}
	}
}
