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
using OpenRA.Scripting;
using OpenRA.Mods.RA.Scripting;
using OpenRA.Traits;

namespace OpenRA.Mods.RA
{
	public class CheckMapScripts : ILintPass
	{
		public void Run(Action<string> emitError, Action<string> emitWarning, Map map)
		{
			var worldActorInfo = map.Rules.Actors["world"];
			if (worldActorInfo == null)
				return;

			var scriptContextInfo = worldActorInfo.Traits.GetOrDefault<LuaScriptInfo>();
			if (scriptContextInfo == null)
				return;

			var scripts = scriptContextInfo.Scripts ?? new string[0];
			var context = new ScriptContext(null, null, scripts);

			if (context.FatalErrorOccurred)
				emitError("Lua script failed to load.");
		}
	}
}
