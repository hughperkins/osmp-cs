/*
 * Created by SharpDevelop.
 * User: Administrator
 * Date: 12/28/2006
 * Time: 10:58 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;

namespace Metaverse.Common.Controller
{
	/// <summary>
	/// Description of IPluginLoader.
	/// </summary>
	public interface IPluginController
	{
		void LoadServerPlugins();
		void LoadClientPlugins();
	}
}
