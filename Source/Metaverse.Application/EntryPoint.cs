// Copyright Hugh Perkins 2004,2005,2006
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
using System.Threading;
using System.IO;
using System.Diagnostics;
using Metaverse.Utility;
using Metaverse.Controller;
using Metaverse.Common.Controller;
using Nini.Config;

namespace Metaverse.Application {
  	
	/// <summary>
	/// The entrypoint for the metaverse application
	/// </summary>
	public class EntryPoint {
	
		/// <summary>
		/// The main function for this application which calls controllers to get them setup
		/// </summary>
		/// <param name="args">Arguments from the commandline</param>
	        public static void Main( string[] args ) {
			
			ArgvConfigSource source = new ArgvConfigSource(args);
				
			source.AddSwitch("CommandLineArgs", "mode", "m");
			source.AddSwitch("CommandLineArgs", "config", "c");
			source.AddSwitch("CommandLineArgs", "logpath", "l");
			source.AddSwitch("CommandLineArgs", "help", "h");
			source.AddSwitch("CommandLineArgs", "serverip");
			source.AddSwitch("CommandLineArgs", "serverport", "p");
			source.AddSwitch("CommandLineArgs", "url");
			source.AddSwitch("CommandLineArgs", "nochat");
			
			bool help = source.Configs["CommandLineArgs"].Contains( "help" );
			
			if( help ) {
				Console.WriteLine( @"Help text goes here" );
				System.Environment.Exit( 0 );
			}
			
			string mode = source.Configs["CommandLineArgs"].GetString( "mode","clientandserver" );
			
			
			if( mode == "clientonly" ) {
				ClientController.Instance.Initialize( source );
				ClientController.Instance.InitializeClient();
			}
			else if ( mode == "serveronly"  ) {
				ServerController.Instance.Initialize( source );
				ServerController.Instance.InitializeServer();
			}
			else if ( mode == "clientandserver" ) {
				ClientController.Instance.Initialize( source );
				ServerController.Instance.Initialize( source );
				ClientController.Instance.InitializeClientWithServer();
			}
			else {
				Console.WriteLine( "You are trying to start Metaverse in an unknown mode. Please type \"Metaverse.exe -help\" for more options." );
				System.Environment.Exit( 0 );
			}
				
	          	return;
	        }
   	 }
}
