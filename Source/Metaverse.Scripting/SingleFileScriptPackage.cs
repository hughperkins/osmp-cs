using System;
using System.Collections;
using Metaverse.Common;

namespace Metaverse.Scripting 
{
	using Metaverse.Common;
	
	public class SingleFileScriptPackage : IScriptPackage
	{
		private string				_name;
		private CSScriptFile		_file;
		private	IScriptGenerator	_generator;
		private bool				_compiled;
		
		public string Name {
			get {
				return _name;
			}
		}
		
		public SingleFileScriptPackage( string name, CSScriptFile file ) {
			
			_name = name;
			_file = file;
			
			return;
		}

		public IScript CompiledScript {
			get {
				if( _compiled ) {
					return _generator.Generate();
				}
				else {
					throw new Exception( "Script package is not compiled, should not be asking for compiled script" );
				}
			}
		}
		
		public bool Compiled {
		
			get {
				return _compiled;
			}
		}
		
		public IDictionary Files {

			get {
				return (IDictionary)new ArrayList( new IScriptFile[]{ _file } );
			}
		}
		
		public IScriptCompiler ExecuteCompilerRequestor() {
			
			return new SingleFileProjectCompiler();
		}

		public IScriptGenerator ExecuteCompiler( IScriptCompiler compiler ) {

			_generator = compiler.Compile( this );
			_compiled = true;

			return _generator;
		}

		public ABuildFile GetBuildFile() {

			return new SingleBuildFile( _file );
		}
		
		public void CreateFile( string name, byte[] data ) {
			
			throw new Exception( "SingleFileScript package does not allow the creation of new files" );
		}

		public void ReplaceFile( string name, byte[] data ) {
			
			throw new Exception( "SingleFileScript package does not allow the replacing of files" );
		}

		public void DeleteFile( string name ) {
			
			throw new Exception( "SingleFileScript package does not allow the deleting of files" );
		}
	}
}
