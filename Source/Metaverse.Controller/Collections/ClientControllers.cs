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
	/// Description of ClientControllers.
	/// </summary>
	public class ClientControllers : IClientControllers
	{
		private static IClientControllers _instance = null;
		
		public static IClientControllers Instance {
			get {
				if( _instance == null ) {
					_instance = new ClientControllers();
				}
				return _instance;
			}
		}
		
		private ClientControllers() { }
		
		
		
		public IClientController Client {
			get {
				return ClientController.Instance;
			}
		}
		
		public IPluginController Plugin {
			get {
				return PluginController.Instance;
			}
		}
	}
}
