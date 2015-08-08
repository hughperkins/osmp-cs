// Copyright Hugh Perkins 2006
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
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;

// creates file association for .osmp files
class FileAssociations
{
    void Add( string extension, string description, string action, string icon )
    {
        RegistryKey extensionkey = Registry.ClassesRoot.CreateSubKey( "." + extension );
        extensionkey.SetValue( "", extension, RegistryValueKind.String );

        RegistryKey filetypekey = Registry.ClassesRoot.CreateSubKey( extension );
        filetypekey.SetValue( "", description );

        RegistryKey commandkey = filetypekey.CreateSubKey( "shell" ).CreateSubKey( "open" ).CreateSubKey( "command" );
        commandkey.SetValue( "", action );

        RegistryKey defaulticonkey = filetypekey.CreateSubKey( "DefaultIcon" );
        defaulticonkey.SetValue( "", icon );
    }

    public void Go()
    {
        string metaverseclientexe = "\"" + EnvironmentHelper.GetExeDirectory() + "\\metaverse.exe\"";
        if (EnvironmentHelper.IsMonoRuntime)
        {
            metaverseclientexe = "\"" + EnvironmentHelper.GetClrDirectory() + "\\mono.exe\" --debug " +
                metaverseclientexe;
        }
        Add( "osmp", "OSMP Worldfile", metaverseclientexe + " -url \"%1\"", "\"" + EnvironmentHelper.GetExeDirectory() + "\\Metaverse.ico\"" );
    }
}
