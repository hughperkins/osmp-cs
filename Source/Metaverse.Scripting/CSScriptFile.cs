using System;
using Metaverse.Common;

namespace Metaverse.Scripting 
{
	public class  CSScriptFile : IScriptFile
	{
		string _path;
		string _source;
		
		public CSScriptFile( string path, string source ) {
			_path = path;
			_source = source;
		}
			
		
		public ScriptFileType	Type 
		{ 
			get { return ScriptFileType.Source; }
		}
		
		public string Path 
		{ 
			get { return _path; }
		}

		public byte[] GetData() {
			
			return null;
		}
	}
}
