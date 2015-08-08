// Copyright Hugh Perkins 2006
// hughperkins at gmail http://hughperkins.com
//
// This program is free software; you can redistribute it and/or modify it
// under the terms of the GNU General Public License version 2 as published by the
// Free Software Foundation;
//
// This program is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for
//  more details.
//
// You should have received a copy of the GNU General Public License along
// with this program in the file licence.txt; if not, write to the
// Free Software Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-
// 1307 USA
// You can find the licence also on the web at:
// http://www.opensource.org/licenses/gpl-license.php
//

using System;
using System.IO;
using Metaverse.Utility;

namespace OSMP
{    
    class ProjectFileController
    {
        public Uri ProjectDirectoryUri;
        
        static ProjectFileController instance = new ProjectFileController();
        public static ProjectFileController GetInstance(){ return instance; }
        // This is kindof a hack really
        // we have a global projectfilecontroller instance so that the texturerelativefilepaths
        // property in prims can convert paths to/from relative paths during xmlserialization
        // prior to xmlserialization, we set the project directory to the path of the
        // world file to be saved/loaded
        public ProjectFileController()
        {
            //ProjectDirectoryUri = new Uri( Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase) + "\\DefaultWorld" );
            ProjectDirectoryUri = new Uri( Path.GetFullPath( "." ) );
            Test.Debug( ProjectDirectoryUri.ToString() );
        }
        public void SetProjectPath( Uri projectpath )
        {
            LogFile.WriteLine( "project path: " + projectpath );
            this.ProjectDirectoryUri = projectpath;
        }
        public string GetRelativePath( Uri uri )
        {
            LogFile.WriteLine( ProjectDirectoryUri + " " + uri );
            //string relativepath = ProjectDirectoryUri.MakeRelativeUri( uri ).ToString();
            string relativepath = ProjectDirectoryUri.MakeRelative( uri );
            Test.Debug( uri + " -> " + relativepath );
            return relativepath;
        }
        public Uri GetFullPath( string relativepath )
        {
            Uri newuri = new Uri( ProjectDirectoryUri, relativepath );
            Test.Debug( newuri.ToString() );
            return newuri;
        }
    }
}
