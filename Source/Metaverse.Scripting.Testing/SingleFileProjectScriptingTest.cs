/*
 * Created by SharpDevelop.
 * User: Administrator
 * Date: 12/13/2006
 * Time: 11:26 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections;
using NUnit.Framework;
using Metaverse.Scripting;
using Metaverse.Common;

namespace Metaverse.Scripting.Testing
{
 

  [TestFixture]
  public class SingleFileProjectScriptingTest
  {
   
    [SetUp]
    public void Init()
    {
    	Console.WriteLine( "Setting up a test" );
    }

    [Test]
    public void Simulation0()
    {
    	// server side
    	
    	SingleSimServer server = new SingleSimServer();
    	
    	//register this server and attach a sim to it
    	server.Initialize();
    	
    	MiniSimulator simulator = server.CreateSimulator();
    	
    	//client side
    	Controller.Client.FileInsertNewSingleFileScript( "/Simulation0.cs",
    	    @"
    	    	using System;
    	    	
    	    	public class Simulation0Script : AGenericScript
    	    	{
    	    		public bool Active 
					{ 
						get { return true; } 
					}

					public void Run( float delta ) {
						Sim.Trace( ""Incremending world data"" );
						void IncrementSimWorldData();
					}
    	    	}
    	    " );
    	
    	//server side again to run the newly added script
    	
    	Assert.AreEqual(1, server.Model.Data);
    	simulator.ScriptExecutor.Run();
		Assert.AreEqual(2, server.Model.Data);
    	simulator.ScriptExecutor.Run();
		Assert.AreEqual(3, server.Model.Data);
		simulator.ScriptExecutor.Run();
		Assert.AreEqual(4, server.Model.Data);
    	
    	return;
    }

  }
}
