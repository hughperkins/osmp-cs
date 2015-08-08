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
using System.Collections;
using System.Threading;
using System.Net;
using OSMP;
using Metaverse.Utility;
using Metaverse.Communication;
using Metaverse.Common.Controller;
using Nini.Config;

namespace OSMP
{
    public class MetaverseClient
    {
        static MetaverseClient instance = new MetaverseClient();
        public static MetaverseClient GetInstance() { return instance; }

        MetaverseClient() // protected constructor, enforce singleton
        {
        }

        public double fHeight = -1.0;    //!< height of hardcoded land
        //public int iMyReference = 0;  //!< iReference of user's avatar
        public Avatar myavatar;
    
        public Config config;
        public IRenderer renderer;
        public PlayerMovement playermovement;
        public WorldModel worldstorage; // client's copy of worldmodel
        public WorldView worldview;

        public NetworkLevel2Controller network;
        public RpcController rpc;
        public NetReplicationController netreplicationcontroller;
        
        //! Manages world processing; things like: moving avatar, managing camera, animation, physics, etc...
        void ProcessWorld()
        {
            //Avatar avatar = (Avatar)worldstorage.GetEntityByReference( iMyReference );
            if( myavatar != null )
            {
                //Test.Debug("x: " << int(playermovement.avatarxpos) << " y: " << int(playermovement.avatarypos) << " z: " << int(playermovement.avatarzpos));
                myavatar.pos = playermovement.avatarpos;
                
                // PLACEHOLDER
                
                playermovement.avatarpos = myavatar.pos;
            }
    
            playermovement.MovePlayer();
          //  LogFile.WriteLine( playermovement.avatarpos );
        }

        public delegate void TickHandler();
        public event TickHandler Tick;
    
        //! Main loop, called by SDL once a frame
        void MainLoop()
        {
            //LogFile.WriteLine("Tick");
            if (targettoload != "" && !waitingforserverconnection )
            {
                // note to self: should shift this comparison into WorldPersist really
                WorldPersistToXml.GetInstance().Load( targettoload );
                targettoload = "";
            }
            ProcessWorld();
            if (Tick != null)
            {
                Tick();
            }
            network.Tick();
        }

        public void Shutdown()
        {
            renderer.Shutdown();
        }
    
        //! Gets world state from server
        void InitializePlayermovement()
        {
            playermovement.avatarpos = new Vector3( 20, 20, 20 );
            playermovement.avatarzrot = 45;
            playermovement.avataryrot = 0;
        }

        public IChat imimplementation;
        public UserChatDialog userchatdialog = null;
        void LoadChat()
        {
            imimplementation = ChatImplementationFactory.CreateInstance();
            userchatdialog = new UserChatDialog();
        }

        string targettoload = ""; // incoming url from osmp:// or possibly commandline

        public bool waitingforserverconnection = true;
        
        public int Init( IConfigSource commandlineConfig, IClientControllers controllers )
        {
        Type type = Type.GetType( "OSMP.ObjectReplicationClientToServer" );
        LogFile.WriteLine( "type: [" + type + "] " + type.AssemblyQualifiedName );
        //System.Environment.Exit( 0 );

        	Tao.DevIl.Il.ilInit();
           	Tao.DevIl.Ilu.iluInit();

            config = Config.GetInstance();

            string serverip = commandlineConfig.Configs["CommandLineArgs"].GetString( "serverip", config.ServerIPAddress );
            int port = commandlineConfig.Configs["CommandLineArgs"].GetInt( "serverport", config.ServerPort );

            network = new NetworkLevel2Controller();
            network.NewConnection += new Level2NewConnectionHandler(network_NewConnection);

            network.ConnectAsClient(serverip, port);

            rpc = new RpcController(network);
            netreplicationcontroller = new NetReplicationController(rpc);

            renderer = RendererFactory.GetInstance();
            renderer.Tick += new OSMP.TickHandler( MainLoop );
            renderer.Init();

            worldstorage = new WorldModel(netreplicationcontroller);
            worldview = new WorldView( worldstorage );
            playermovement = PlayerMovement.GetInstance();

            InitializePlayermovement();

            myavatar = new Avatar();
            worldstorage.AddEntity(myavatar);
 			
           
           controllers.Plugin.LoadClientPlugins();
            if (!commandlineConfig.Configs["CommandLineArgs"].Contains("nochat" ))
            {
                LoadChat();
            }

            if( commandlineConfig.Configs["CommandLineArgs"].Contains("url" ) )
            {
			string url = commandlineConfig.Configs["CommandLineArgs"].GetString("url" );
                LogFile.WriteLine( "url: " +  url);

                if (url.StartsWith( "osmp://" ))
                {
                    targettoload = "http://" + url.Substring( "osmp://".Length );
                    LogFile.WriteLine( "target: " + targettoload );
                }
                else
                {
                    targettoload = url;
                }
            }

            renderer.StartMainLoop();

            return 0;
        }

        public void ConnectToServer(string ipaddressstring, int port)
        {
            IPAddress[] addresses = System.Net.Dns.GetHostAddresses(ipaddressstring);
            if (addresses.GetLength(0) == 0)
            {
                return;
            }
            IPAddress ipaddress = addresses[0];
            LogFile.WriteLine("Resolved server address to : " + ipaddressstring);

            try
            {
                network.ConnectAsClient(ipaddress.ToString(), port);
            }
            catch (Exception e)
            {
                DialogHelpers.ShowErrorMessageModal( null, "Failed to connect to server");
                LogFile.WriteLine(e.ToString());
            }

            SelectionModel.GetInstance().Clear();
            worldstorage.Clear();
        }

        void network_NewConnection(NetworkLevel2Connection net2con, ConnectionInfo connectioninfo)
        {
            LogFile.WriteLine("client connected to server");
            waitingforserverconnection = false;

            InitializePlayermovement();

            myavatar = new Avatar();
            worldstorage.AddEntity(myavatar);

            new NetworkInterfaces.WorldControl_ClientProxy(rpc, connectioninfo.Connection)
                .RequestResendWorld();
        }
    }
}
   
