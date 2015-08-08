// Copyright Hugh Perkins 2006
// hughperkins@gmail.com http://manageddreams.com
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
//using System.Collections;
using System.Collections.Generic;
using System.Reflection;
//using System.Reflection.Emit;
using System.Threading;
using System.Text;
using System.Net;

using Metaverse.Utility;

namespace OSMP
{
    public class RpcController
    {
        // hack so this works now in separate assemblies
        public string TargetAssemblyName = "Metaverse.Client, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";

        const char RpcType = 'Y';

        public NetworkLevel2Controller network;

        BinaryPacker binarypacker = new BinaryPacker();

        public bool isserver = true;
            
        public RpcController( NetworkLevel2Controller network )
        {
            this.network = network;
            network.RegisterPacketConsumer(RpcType, new Level2ReceivedPacketHandler(network_ReceivedPacket));
            //network.ReceivedPacket += new Level2ReceivedPacketHandler(network_ReceivedPacket);
            //network.NetworkChangeState += new Level1NetworkChangeStateHandler( _NetworkChangeState );
            isserver = network.IsServer;
        }

        bool TypeIsAllowed(string typename)
        {
            return NetworkInterfaces.AuthorizedTypes.GetInstance().IsAuthorized(typename);
            //List<Type> allowedtypes = OSMP.NetworkInterfaces.AuthorizedTypes.GetInstance().AuthorizedTypeList;
            //foreach (Type type in allowedtypes)
            //{
              //  if (typename == type.ToString())
//                {
  //                  return true;
    //            }
      //      }
        //    return false;
        }

        void network_ReceivedPacket( NetworkLevel2Connection connection, byte[] data, int offset, int length )
        {
            //LogFile.WriteLine("rpc received packet " + Encoding.UTF8.GetString(data, offset, length));
            try
            {
                if (length > 1)
                {
                    int position = offset;
                    //char type = (char)binarypacker.ReadValueFromBuffer(data, ref position, typeof(Char));
                    //if (type == RpcType)
                    //{
                        string typename = (string)binarypacker.ReadValueFromBuffer(data, ref position, typeof(string));
                        string methodname = (string)binarypacker.ReadValueFromBuffer(data, ref position, typeof(string));
                //        LogFile.WriteLine("Got rpc [" + typename + "] [" + methodname + "]");
                        if( TypeIsAllowed( typename ) ) // security check to prevent arbitrary activation
                        //if (ArrayHelper.IsInArray(allowedtypes, typename))
                        {
                            int dotpos = typename.LastIndexOf(".");
                            string namespacename = "";
                            string interfacename;
                            if (dotpos >= 0)
                            {
                                namespacename = typename.Substring(0, dotpos );
                                interfacename = typename.Substring(dotpos + 1);
                            }
                            else
                            {
                                interfacename = typename;
                            }
            //                LogFile.WriteLine("[" + namespacename + "][" + interfacename + "]");

                            string serverwrapperclassname = "OSMP." + interfacename.Substring(1) + "";
              //              LogFile.WriteLine("serverwrapperclassname [" + serverwrapperclassname + "]");
                            //if (namespacename != "")
                            //{
                              //  serverwrapperclassname = namespacename + "." + serverwrapperclassname;
                            //}

                            Type interfacetype = Type.GetType(typename);

                            string typenametoinstantiate = serverwrapperclassname + ", " + TargetAssemblyName;
                            LogFile.WriteLine( "typenametoinstantiate: [" + typenametoinstantiate + "]" );
                            Type serverwrapperttype = Type.GetType(serverwrapperclassname + ", " + TargetAssemblyName);

                            if (isserver)
                            {
                                LogFile.WriteLine( "server RpcController, instantiating [" + serverwrapperttype + "]" );
                            }
                            else
                            {
                                LogFile.WriteLine( "client RpcController, instantiating [" + serverwrapperttype + "]" );
                            }
                            object serverwrapperobject = Activator.CreateInstance(serverwrapperttype, new object[] { connection.connectioninfo.Connection });
                            MethodInfo methodinfo = serverwrapperttype.GetMethod(methodname);

                            ParameterInfo[] parameterinfos = methodinfo.GetParameters();
                            object[] parameters = new object[parameterinfos.GetLength(0)];
                            for (int i = 0; i < parameters.GetLength(0); i++)
                            {
                                parameters[i] = binarypacker.ReadValueFromBuffer(data, ref position, parameterinfos[i].ParameterType);
                            }

                            //foreach (object parameter in parameters)
                            //{
                             //   LogFile.WriteLine(parameter.GetType().ToString() + " " + parameter.ToString());
                            //}
                            methodinfo.Invoke(serverwrapperobject, parameters);
                        }
                        else
                        {
                            LogFile.WriteLine("Warning: unauthorized RPC type " + typename + ". Check has attribute [AuthorizedRpcInterface]");
                        }
                    //}
                }
            }
            catch (Exception ex)
            {
                LogFile.WriteLine(ex);
            }
        }
        
        public bool IsServer{
            get{ return isserver; }
        }
        
        //public void _NetworkChangeState( object source, NetworkChangeStateArgs e )
        //{
          //  isserver = e.IsServer;
        //}
        
        // rpc format:
        // (classname)(methodname)(arg1)(arg2)(arg3)...
        // SendRpc( connection, "OSMP.Testing.ITestInterface", "SayHello",  new object[]{  } )
        public void SendRpc(IPEndPoint connection, string typename, string methodname, object[] args)
        {
            // Test.WriteOut( args );
            LogFile.WriteLine( "SendRpc " + typename + " " + methodname );
            //for( int i = 0; i < args.GetUpperBound(0) + 1; i++ )
            //{
              //  LogFile.WriteLine("  arg: " + args[i].ToString() );
            //}
            
            byte[]packet = new byte[1400]; // note to self: make this a little more dynamic...
            int nextposition = 0;

            //binarypacker.WriteValueToBuffer(packet, ref nextposition, RpcType);
            binarypacker.WriteValueToBuffer(packet, ref nextposition, typename);
            binarypacker.WriteValueToBuffer(packet, ref nextposition, methodname);
            foreach (object parameter in args)
            {
                binarypacker.WriteValueToBuffer(packet, ref nextposition, parameter);
            }

            //LogFile.WriteLine("Sending " + Encoding.UTF8.GetString(packet, 0, nextposition));
            //LogFile.WriteLine( nextposition + " bytes " + Encoding.ASCII.GetString( packet, 0, nextposition ) );
            network.Send(connection,RpcType, packet, 0, nextposition );
        }
    }
}
