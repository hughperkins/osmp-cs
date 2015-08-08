using System;
using Metaverse.Common;

namespace Metaverse.Scripting 
{
	
	public class AssemblyScriptGenerator : IScriptGenerator
	{
		string _pathToCompiledAssembly;

		public AssemblyScriptGenerator( string pathToCompiledAssembly ) {
		
			_pathToCompiledAssembly = pathToCompiledAssembly;

			return;
		}

		public IScript Generate() {
			
			return null;
		}
	}

}
