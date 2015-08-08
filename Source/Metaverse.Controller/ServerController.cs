/*
 * Created by SharpDevelop.
 * User: Administrator
 * Date: 12/28/2006
 * Time: 10:12 PM
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
using System.Threading;
using Metaverse.Common.Controller;
using Nini.Config;
using Metaverse.Utility;

namespace Metaverse.Controller
{
	/// <summary>
	/// Description of MetaverseServerController.
	/// </summary>
	public class ServerController : IServerController
	{
		private static IServerController _instance = null;
		private IConfigSource _commandlineConfig;
		
		public static IServerController Instance {
			get {
				if( _instance == null ) {
					_instance = new ServerController();
				}
				return _instance;
			}
		}
		
		private ServerController() { }
		
		
		
		public void Initialize( IConfigSource commandlineConfig ) {
			_commandlineConfig = commandlineConfig;
		}
		
		public void InitializeServer() {
			MetaverseServer.GetInstance().Init(_commandlineConfig, ServerControllers.Instance);
            
			while (true)
            {
                MetaverseServer.GetInstance().OnTick();
                Thread.Sleep(50);
            }
		}
		
	}
}
