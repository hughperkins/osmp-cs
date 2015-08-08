/*
 * Created by SharpDevelop.
 * User: Administrator
 * Date: 12/13/2006
 * Time: 11:52 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections;
using Metaverse.Common;

namespace Metaverse.Scripting.Testing
{
	/// <summary>
	/// Description of MiniSimulator.
	/// </summary>
	public class SingleSimServer : IMetaverseServer
	{
		ArrayList _scripts;
		public IWorldModel Model;

		public void Initialize()
		{
			Model = new WorldModel();
			Model.Data = 1;
			_scripts = new ArrayList();
			Controller.Metaverse.RegisterMetaverseServer( this );
		}
		
		public MiniSimulator CreateSimulator() {
			MiniSimulator sim = new MiniSimulator();
			Controller.Metaverse.AttachSimulator( sim );
			return sim;
		}
		
		public ArrayList GetScripts()
		{
			return _scripts;
		}
		
		public void InsertScript(IScript script)
		{
			_scripts.Add( script );
		}
		
		public IWorldModel GetWorldModel(ISim simulator)
		{
			return Model;
		}
	}
	
	
	/// <summary>
	/// Description of MiniSimulator.
	/// </summary>
	public class MiniSimulator : ISim
	{
		public MiniSimulatorScriptExecutor ScriptExecutor;
		
		public MiniSimulator()
		{
			ScriptExecutor = new MiniSimulatorScriptExecutor( this, .5f );
		}
		
		public void Trace( string message ) {
			Console.WriteLine( message );
		}
		
		public void IncrementSimWorldData()
		{
			SimController.Singleton.GetSimulatorWorldModel( this ).Data++;
		}
	}
	
	/// <summary>
	/// Description of MiniSimulator.
	/// </summary>
	public class MiniSimulatorScriptExecutor : IScriptExecutor
	{
		ISim _simulator;
		private float _delta;
		
		public MiniSimulatorScriptExecutor( ISim simulator, float delta ) 
		{		
			_simulator = simulator;
			_delta = delta;	
		}
		
		public void Initialize()
		{
			throw new NotImplementedException();
		}
		
		public void Start()
		{
			throw new NotImplementedException();
		}
		
		public void Stop()
		{
			throw new NotImplementedException();
		}
		
		public void Run()
		{
			ArrayList scripts = SimController.Singleton.GetSimulatorScripts( _simulator );
			
			foreach( IScript script in scripts ) {
				script.Run( _delta );
			}
		}
		
	}
}
