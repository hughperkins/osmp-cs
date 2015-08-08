using System;
using Metaverse.Common;

namespace Metaverse.Scripting 
{
	public class SingleBuildFile : ABuildFile
	{
		IScriptFile _file;
		
		public override string Path 
		{ 
			get { return @"\default.build"; }
		}

		public SingleBuildFile( IScriptFile file ) {
			
			_file = file;

			return;
		}

		public IScriptFile[] GetCompilableFiles() {
		
			return new IScriptFile[]{ _file };
		}

		public override byte[] GetData() {

			return new byte[]{};
		}
	}
}
