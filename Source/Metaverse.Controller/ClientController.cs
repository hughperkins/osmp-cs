/*
 * Created by SharpDevelop.
 * User: Administrator
 * Date: 12/28/2006
 * Time: 10:03 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO;
using System.Diagnostics;
using OSMP;
using Metaverse.Common.Controller;
using Metaverse.Utility;
using Nini.Config;

namespace Metaverse.Controller
{
	/// <summary>
	/// Description of MetaverseController.
	/// </summary>
	public class ClientController : IClientController
	{
		private static IClientController _instance = null;
		
				
		private IConfigSource _commandlineConfig;
		
		public static IClientController Instance {
			get {
				if( _instance == null ) {
					_instance = new ClientController();
				}
				return _instance;
			}
		}
		
		private ClientController() { }
		
		public void Initialize( IConfigSource commandlineConfig ) {
			
			_commandlineConfig = commandlineConfig;
			
			System.Environment.SetEnvironmentVariable( "PATH", System.Environment.GetEnvironmentVariable( "PATH" ) + ";" + EnvironmentHelper.GetExeDirectory(), EnvironmentVariableTarget.Process );
	
			string logFilePath = Path.Combine( _commandlineConfig.Configs["CommandLineArgs"].GetString("logpath", EnvironmentHelper.GetExeDirectory()), "osmpclient_"+DateTime.Now.ToLongDateString()+".log");
			LogFile.GetInstance().Init( logFilePath );
			
			return;
		}
		
		public void InitializeClient() {
			
			throw new NotImplementedException( "We are not sure if implementing client only works at the moment" );
			//MetaverseClient.GetInstance().Init(_commandlineConfig, ClientControllers.Instance);
			
			return;
		}
		
		public void InitializeClientWithServer() {

				try {
					MetaverseServer.GetInstance().Init(_commandlineConfig, ServerControllers.Instance);
			            	MetaverseClient.GetInstance().Tick += new MetaverseClient.TickHandler(EntryPoint_Tick);
			            	MetaverseClient.GetInstance().Init(_commandlineConfig, ClientControllers.Instance);
				}
				catch ( Exception e )
				{
			            Console.WriteLine( e );
			            string errorlogpath = EnvironmentHelper.GetExeDirectory() + "/error.log";
			            StreamWriter sw = new StreamWriter( errorlogpath, false );
			            sw.WriteLine( LogFile.GetInstance().logfilecontents );
			            sw.WriteLine( e.ToString() );
			            sw.Close();
			
			            if (System.Environment.OSVersion.Platform != PlatformID.Unix)
			            {
			                ProcessStartInfo psi = new ProcessStartInfo( "notepad.exe", errorlogpath );
			                psi.UseShellExecute = true;
			                Process process = new Process();
			                process.StartInfo = psi;
			                process.Start();
			            }
			            else {
			            	throw new NotImplementedException( "An exception has occurred and we are not sure yet how to show this to you. Please look over your log files for errors" );
			            }
			        }
			
				return;
		}
		
		private void EntryPoint_Tick()
        {
            MetaverseServer.GetInstance().OnTick();
        }
	}
}
