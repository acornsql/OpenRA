#region Copyright & License Information
/*
 * Copyright 2007-2015 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see COPYING.
 */
#endregion

using System;
using System.Linq;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Lint
{
	class CheckSequences : ILintPass
	{
		public void Run(Action<string> emitError, Action<string> emitWarning, Map map)
		{
			var sequences = MiniYaml.MergeLiberal(map.SequenceDefinitions, Game.ModData.Manifest.Sequences.Select(s => MiniYaml.FromFile(s)).Aggregate(MiniYaml.MergeLiberal));

			foreach (var actorInfo in map.Rules.Actors)
			{
				foreach (var renderInfo in actorInfo.Value.Traits.WithInterface<RenderSimpleInfo>())
				{
					var image = renderInfo.Image ?? actorInfo.Value.Name;
					if (!sequences.Any(s => s.Key == image.ToLowerInvariant()) && !actorInfo.Value.Name.Contains("^"))
						emitWarning("Sprite image {0} from actor {1} has no sequence definition.".F(image, actorInfo.Value.Name));
				}

				foreach (var traitInfo in actorInfo.Value.Traits.WithInterface<ITraitInfo>())
				{
					foreach (var field in traitInfo.GetType().GetFields())
					{
						if (field.HasAttribute<SequenceReferenceAttribute>())
						{
							var sequence = (string)field.GetValue(traitInfo);
							if (string.IsNullOrEmpty(sequence))
								continue;

							var renderInfo = actorInfo.Value.Traits.WithInterface<RenderSpritesInfo>().FirstOrDefault();
							var image = renderInfo.GetImage(actorInfo.Value, map.SequenceProvider, null); // TODO: check all possible races

							var sequenceReference = field.GetCustomAttributes<SequenceReferenceAttribute>(true).FirstOrDefault();
							if (sequenceReference != null && !string.IsNullOrEmpty(sequenceReference.ImageOverride))
								image = sequenceReference.ImageOverride;

							var definitions = sequences.FirstOrDefault(n => n.Key == image.ToLowerInvariant());
							if (definitions != null && !definitions.Value.Nodes.Any(n => n.Key.StartsWith(sequence))) // some sequences are suffixed
								emitWarning("Sprite image {0} from actor {1} does not define sequence {2} from {3}".F(image, actorInfo.Value.Name, sequence, traitInfo));
						}
					}
				}
			}
		}
	}
}
