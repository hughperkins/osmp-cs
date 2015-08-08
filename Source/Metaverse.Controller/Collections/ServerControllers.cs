/*
 * Created by SharpDevelop.
 * User: Administrator
 * Date: 12/28/2006
 * Time: 11:47 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using Metaverse.Common.Controller;
using OSMP;

namespace Metaverse.Controller
{
	/// <summary>
	/// Description of ServerControllers.
	/// </summary>
	public class ServerControllers : IServerControllers
	{
		private static IServerControllers _instance = null;
		
		public static IServerControllers Instance {
			get {
				if( _instance == null ) {
					_instance = new ServerControllers();
				}
				return _instance;
			}
		}
		
		private ServerControllers() { }
		
		public IServerController Server {
			get {
				return ServerController.Instance;
			}
		}
		
		public IPluginController Plugin {
			get {
				return PluginController.Instance;
			}
		}
	}
}
