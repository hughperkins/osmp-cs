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
using System.Collections;
using System.Text; // for Encoding
using System.Net;
using Metaverse.Utility;
    
namespace OSMP
{
    public class PacketHandlerArgs
    {
        public int PacketKey;
        public char PacketCode;
        public short PacketRef;
        public byte[] Data;
        public int NextPosition;
        public PacketHandlerArgs( int PacketKey, char PacketCode, short PacketRef, byte[] packet, int nextposition )
        {
            this.PacketKey = PacketKey;
            this.PacketCode = PacketCode;
            this.PacketRef = PacketRef;
            this.Data = packet;
            this.NextPosition = nextposition;
        }
    }

    public delegate void PacketHandler( object source, PacketHandlerArgs e );
    
    // This is going to handle a single network connection
    public class NetworkLevel2Connection
    {
        public INetworkImplementation networkimplementation;
        public NetworkLevel2Controller parent;
        public bool isserver;

        //public IPAddress RemoteIPAddress;
        //public int RemotePort;
        
        //public object connection;

        BinaryPacker binarypacker = new BinaryPacker();

        public DateTime lasttimestamp;
        public ConnectionInfo connectioninfo;
        
        NetPacketReferenceController packetreferencecontroller;
        public NetSharedSecretExchange sharedsecretexchange;  // responsible for exchanging and validating shared secret
        
        Hashtable unsafepackethandlers = new Hashtable();     // packets which dont have correct sharedsecret, or sharedsecret not yet validated
        Hashtable packethandlers = new Hashtable();     
        
        public NetworkLevel2Connection( NetworkLevel2Controller parent, ConnectionInfo connectioninfo, bool isserver )
        {
            LogFile.WriteLine("NetworkLevel2Connection()");

            networkimplementation = parent.networkimplementation;
            this.connectioninfo = connectioninfo;
            
            this.parent = parent;
            //this.connection = connectioninfo.Connection;
            this.isserver = isserver;
            
            lasttimestamp = DateTime.Now;
            
            packetreferencecontroller = new NetPacketReferenceController( this, isserver );
            sharedsecretexchange = new NetSharedSecretExchange( this, isserver );
            //sharedsecretexchange.Tick();
        }
        
        public void Tick()
        {
            sharedsecretexchange.Tick();
            packetreferencecontroller.Tick();
        }
        
        public void ReceivedPacketHandler( ConnectionInfo connection, byte[]packet, int nextposition, int length )
        {
            //object connection = e.Connection;
            //byte[] packet = e.Data;
            //int nextposition = e.DataStartIndex;
            
            lasttimestamp = DateTime.Now;
            
            if( packet.Length >= 4 + 2 + 1 )
            {
                int packetkey = (int)binarypacker.ReadValueFromBuffer(packet, ref nextposition, typeof(int));
                short packetref = (short)binarypacker.ReadValueFromBuffer(packet, ref nextposition, typeof(short));
                char packetcode = (char)binarypacker.ReadValueFromBuffer(packet, ref nextposition, typeof(char));
                
                //LogFile.WriteLine( "Packet key: " + packetkey.ToString() + " packetref: " + packetref.ToString() + " packetcode: " + packetcode );

                if( unsafepackethandlers.Contains( packetcode ) )
                {
                    //LogFile.WriteLine("calling unsafepackethandler...");
                    if( packetreferencecontroller.ValidateIncomingReference( packetref ) )
                    {
                        //LogFile.WriteLine("Incoming reference validated");
                        ((PacketHandler)unsafepackethandlers[packetcode])(this, new PacketHandlerArgs(
                            packetkey, packetcode, packetref, packet, nextposition ) );
                    }
                }
                else
                {
                    if( sharedsecretexchange.ValidateIncomingPacketKey( packetkey ) )
                    {
                        if( packetreferencecontroller.ValidateIncomingReference( packetref ) )
                        {
                            if( packethandlers.Contains( packetcode ) )
                            {
                                ((PacketHandler)packethandlers[ packetcode ])( this, new PacketHandlerArgs(
                                    packetkey, packetcode, packetref, packet, nextposition ) );
                            }
                            else if (parent.packetconsumers.ContainsKey(packetcode))
                            {
                                parent.packetconsumers[packetcode](this, packet, nextposition, length - nextposition);
                            }
                            else
                            {
                                LogFile.WriteLine("Warning: unknown packet code " + packetcode.ToString() + " " + Encoding.ASCII.GetString(packet, 0, packet.Length));
                            }
                        }// else silently ignore duplicate packet
                    }
                    else
                    {
                        if (isserver)
                        {
                            LogFile.WriteLine("WARNING: server received potentially spoofed packet allegedly from " + connection.ToString() + " " + Encoding.ASCII.GetString(packet, 0, packet.Length));
                        }
                        else
                        {
                            LogFile.WriteLine("WARNING: client received potentially spoofed packet allegedly from " + connection.ToString() + " " + Encoding.ASCII.GetString(packet, 0, packet.Length));
                        }
                    }
                }
            }
        }
        
        byte[] GenerateOutgoingPacket( char packettype, byte[] data, int offset, int length )
        {
            byte[]packet = new byte[ length + 4 + 2 + 1 ];
            int nextposition = offset;

            short packetreference = packetreferencecontroller.NextReference;

            binarypacker.WriteValueToBuffer(packet, ref nextposition, sharedsecretexchange.SharedSecretKey);
            binarypacker.WriteValueToBuffer(packet, ref nextposition, packetreference);
            binarypacker.WriteValueToBuffer(packet, ref nextposition, packettype);
            Buffer.BlockCopy(data, offset, packet, nextposition, length);
            
            packetreferencecontroller.RegisterSentPacket( packetreference, packet );

            return packet;
        }

        public void Send(char packettype, byte[] data)
        {
            Send(packettype, data, 0, data.GetLength(0));
        }
        
        public void Send( char packettype, byte[] data, int offset, int length )
        {
            byte[]outgoingpacket = GenerateOutgoingPacket( packettype, data, offset, length );
            RawSend( outgoingpacket );
        }
        
        public void SendNonAckable( char packettype, byte[] data )
        {
            byte[]outgoingpacket = new byte[ data.Length + 4 + 2 + 1 ];
            int nextposition = 0;

            binarypacker.WriteValueToBuffer(outgoingpacket, ref nextposition, sharedsecretexchange.SharedSecretKey);
            binarypacker.WriteValueToBuffer(outgoingpacket, ref nextposition, (short)0);
            binarypacker.WriteValueToBuffer(outgoingpacket, ref nextposition, packettype);
            Buffer.BlockCopy( data, 0, outgoingpacket, nextposition, data.Length );
            
            RawSend( outgoingpacket );
        }
        
        // primarily for use of NetPacketReferenceController, or equivalent, to resent non-acked sent packets
        // note that we're still going to rewrite shared key
        public void RawSend( byte[]packet)
        {
            int offset = 0;
            new BinaryPacker().WriteValueToBuffer( packet, ref offset, sharedsecretexchange.SharedSecretKey );
            if( isserver )
            {
                networkimplementation.Send( connectioninfo.Connection, packet );
            }
            else
            {
                networkimplementation.Send( packet );
            }
        }
        
        public void RegisterUnsafePacketHandler( char packetcode, PacketHandler packethandler )
        {
            if( unsafepackethandlers.Contains( packetcode ) && (PacketHandler)unsafepackethandlers[ packetcode ] != packethandler )
            {
                throw new Exception( "Trying to register duplicate packetcode " + packetcode.ToString() + " by handler " + packethandler.ToString() + " conflicting handler: " + unsafepackethandlers[ packetcode ].ToString() );
            }
            if( !unsafepackethandlers.Contains( packetcode ) )
            {
                //LogFile.WriteLine("Registering unsafe-packet handler " + packetcode.ToString() + " " + packethandler.ToString() );
                unsafepackethandlers.Add( packetcode, packethandler );
            }
        }
        
        public void RegisterPacketHandler( char packetcode, PacketHandler packethandler )
        {
            if( packethandlers.Contains( packetcode ) && (PacketHandler)packethandlers[ packetcode ] != packethandler )
            {
                throw new Exception( "Trying to register duplicate packetcode " + packetcode.ToString() + " by handler " + packethandler.ToString() + " conflicting handler: " + packethandlers[ packetcode ].ToString() );
            }
            if( !packethandlers.Contains( packetcode ) )
            {
                packethandlers.Add( packetcode, packethandler );
            }
        }
        
        public void OnConnectionValidated()  // generated by NetSharedSecretExchange once key validated
        {
            parent.OnConnectionValidated(this);
        }
        
        public void _Disconnect()
        {
        }
        
        //void OnNewConnection()
        //{
            // notify observers of new connection
          //  if( NewConnection != null )
            //{
               // NewConnection( this, e );
           // }
        //}
    }
}
