using System;
using System.Collections;
using Metaverse.Common;

namespace Metaverse.Scripting 
{
	
	public interface IScriptPackage
	{
		string Name { get; }
		
		IScript CompiledScript { get; }
		
		bool Compiled { get; }
		
		IDictionary Files { get; }

		ABuildFile GetBuildFile();
		void CreateFile( string name, byte[] data );
		void ReplaceFile( string name, byte[] data );
		void DeleteFile( string name );
	}
}
