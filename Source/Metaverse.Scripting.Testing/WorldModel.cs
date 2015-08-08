/*
 * Created by SharpDevelop.
 * User: Administrator
 * Date: 12/14/2006
 * Time: 1:41 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using Metaverse.Common;
using System.Collections;

namespace Metaverse.Scripting.Testing
{
	/// <summary>
	/// Description of WorldModel.
	/// </summary>
	public class WorldModel : IWorldModel
	{
		private int _data = 0;
		
		public int Data {
			get { return _data; }
			set { _data = value; }
		}
	}
}
