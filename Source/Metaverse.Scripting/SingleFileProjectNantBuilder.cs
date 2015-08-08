using System;
using System.Xml;
using System.Collections;
using Metaverse.Common;
using Metaverse.Common;

namespace Metaverse.Scripting
{
	
	public class SingleFileProjectNantBuilder : INantBuilder 
	{
		
		private SingleFileScriptPackage _package;
		
		public void LoadProject( SingleFileScriptPackage package ) {
			_package = package;
		}
		
		
		
		public XmlDocument Generate()
		{
			ArrayList references = new ArrayList();
			
			XmlDocument doc = new XmlDocument();
			
			XmlNode root = doc.DocumentElement;
           
			XmlElement projectNode = doc.CreateElement( "project" );
			projectNode.SetAttribute( "name", _package.Name );
			projectNode.SetAttribute( "default", "build" );
			projectNode.SetAttribute( "basedir", "." );
            root.AppendChild( projectNode );
           
            XmlElement targetBuildNode = doc.CreateElement( "target" );
            targetBuildNode.SetAttribute( "name", "build" );
            projectNode.AppendChild( targetBuildNode );
          
            XmlElement cscOperationNode = doc.CreateElement( "csc" );
            cscOperationNode.SetAttribute( "target", "library" );
            cscOperationNode.SetAttribute( "output", "bin/" + _package.Name + ".dll" );
            targetBuildNode.AppendChild( cscOperationNode );
            
            XmlElement cscReferencesNode = doc.CreateElement( "references" );
            cscOperationNode.AppendChild( cscReferencesNode );
            
            foreach( string referencePath in references ) {
            	 XmlElement cscReferenceNode = doc.CreateElement( "include" );
            	 cscReferenceNode.SetAttribute( "name", referencePath );
           		 cscReferencesNode.AppendChild( cscReferenceNode );
            }
            
            XmlElement cscSourcesNode = doc.CreateElement( "sources" );
            cscOperationNode.AppendChild( cscSourcesNode );
            
             foreach( IScriptFile file in _package.Files ) {
            	 XmlElement cscSourceNode = doc.CreateElement( "include" );
            	 cscSourceNode.SetAttribute( "name", file.Path );
           		 cscSourcesNode.AppendChild( cscSourceNode );
            }
            	
			return doc;
		}
	}
		
}
