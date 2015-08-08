// Copyright Hugh Perkins 2006
// hughperkins at gmail http://hughperkins.com
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
using Metaverse.Utility;
    
namespace OSMP
{
    // sharedkey, spoofing protection
    // ============================
    //
    // sharedkey, int32, is used to make spoofing harder.  No attempt to eliminate man-in-the-middle.
    // If the sharedkey on an incoming packet doesnt match that for the connection, it is rejected
    //
    // Protocol for key exchange:
    //   client sends R packet to server.  This requests the server to send it the shared key, and contains a temporary key to authenticate the server reply
    //    neither client or server knows key at this point
    //    Packet format:   [int32 xxx][short xxx][char 'R'][temporary client key]
    //
    //   server sends C packet to client, which contains the shared key, and also the temporary client key the client sent
    //    server knows key now, client will receive it
    //    Packet format:   [int32 xxx][short xxx][char 'C'][temporary client key][shared key]
    //    server replies to any R packet, generating the shared key if the connection didnt exist before
    //    server marks connection open
    //
    //   client recieves C packet, only accepts C packets with the correct tempclientkey
    //    client marks connection verified on receipt of C packet
    //
    // R and C packets are non-ackable.  Client will just keep generating R packets till connection opened
    //

    // what we have achieved is to avoid the client pretending to be someone else, since the client can only find out the sharedkey if it provides
    // a valid return ip address to the server
    // server spoofing is prevented in most cases because it is the client making the request, and because the server has to receive the temporary,
    // client key sent to its address by the client, in order to create a valid A packet
    // we can mitigate this later using an IM (IRC, Jabber...) authentication scheme.
    public class NetSharedSecretExchange
    {
        public int RPacketIntervalSeconds = 1;        
        
        public bool Validated = false;
        public int SharedSecretKey;

        BinaryPacker binarypacker = new BinaryPacker();

        NetworkLevel2Connection parent;
        bool isserver;
        int tempclientkey;
        bool initialrpacketsent;
        DateTime lastRpacket; // for client
        
        static MyRand rand;
        
        public NetSharedSecretExchange( NetworkLevel2Connection networkmodelconnection, bool isserver )
        {
            //LogFile.WriteLine("NetSharedSecretExchange()");
            this.parent = networkmodelconnection;
            this.isserver = isserver;
            
            parent.RegisterUnsafePacketHandler( 'R', new PacketHandler( RPacketHandler ) );
            parent.RegisterUnsafePacketHandler( 'C', new PacketHandler( CPacketHandler ) );
            // parent.RegisterUnsafePacketHandler('D', new PacketHandler(DPacketHandler));
            
            if( rand == null )
            {
                rand = new MyRand( (int)System.DateTime.Now.Ticks );
            }
            
            lastRpacket = new DateTime();
            
            if( isserver )
            {
                SharedSecretKey = GenerateSharedKey();
            }
            else
            {
                tempclientkey = rand.GetRandomInt( 0, int.MaxValue - 1 );
                //SendRPacketToServer();
            }
        }
        
        public bool ValidateIncomingPacketKey( int key )
        {
            if( Validated && SharedSecretKey == key )
            {
                return true;
            }
            return false;
        }

        //   server sends C packet to client, which contains the shared key, and also the temporary client key the client sent
        //    server knows key now, client will receive it
        //    Packet format:   [int32 xxx][short xxx][char 'C'][temporary client key][shared key]
        //    server replies to any R packet, generating the shared key if the connection didnt exist before
        public void RPacketHandler(object source, PacketHandlerArgs e)
        {
            if( isserver )
            {
                byte[] packet = e.Data;
                int tempnextposition = e.NextPosition;
                int tempclientkey = (int)binarypacker.ReadValueFromBuffer( packet, ref tempnextposition, typeof( int ) );

                LogFile.WriteLine( "R packet received, sending C packet, tempclientkey: " + tempclientkey.ToString() + " sharedkey: " + SharedSecretKey.ToString() );

                byte[] newpacket = new byte[8];
                tempnextposition = 0;
                binarypacker.WriteValueToBuffer(newpacket, ref tempnextposition, tempclientkey);
                binarypacker.WriteValueToBuffer(newpacket, ref tempnextposition, SharedSecretKey);

                parent.SendNonAckable('C', newpacket );
                if( !Validated )
                {
                   Validated = true;
                   LogFile.WriteLine("Shared key sent; marking connection open" );
                   parent.OnConnectionValidated();
                }                
            }
        }

        public void CPacketHandler(object source, PacketHandlerArgs e)
        {
            //   client recieves C packet, only accepts C packets with the correct tempclientkey
            //    client marks connection verified on receipt of C packet
            //    client sends D packet to the server, which is empty packet
            //    Client now considers connection "open"
            if (!isserver) // client
            {
                byte[] packet = e.Data;
                int tempnextposition = e.NextPosition;
                int tempclientkey = (int)binarypacker.ReadValueFromBuffer(packet, ref tempnextposition, typeof(int));
                LogFile.WriteLine("C packet received, tempclientkey: " + tempclientkey.ToString() + " our key: " + this.tempclientkey);

                if (tempclientkey == this.tempclientkey)
                {
                    SharedSecretKey = (int)binarypacker.ReadValueFromBuffer(packet, ref tempnextposition, typeof(int));
                    LogFile.WriteLine("Connection to server confirmed, sharedkey: " + SharedSecretKey.ToString());
                    Validated = true;
                    parent.OnConnectionValidated();
                    //parent.Send('D', new byte[] { });
                }
                else
                {
                    LogFile.WriteLine("SharedSecretExchange. WARNING: potential spoof packet detected, allegedly from server " + Encoding.ASCII.GetString(e.Data, 0, e.Data.Length));
                }
            }
        }

        ////   server receives D packet and considers connection open
        ////    server marks connection verified
        //public void DPacketHandler(object source, PacketHandlerArgs e)
        //{
          //  if( isserver )
            //{
              //  if( !Validated && e.PacketKey == SharedSecretKey )
                //{
                  //  Validated = true;
//                    LogFile.WriteLine("Shared key confirmed; sending ConnectionValidated event to parent" );
  //                  parent.OnConnectionValidated();
    //            }                
      //      }
        //}
        
        void SendRPacketToServerIfNecessary()
        {
            if( !isserver )
            {
                if( !Validated )
                {
                    if( (int)DateTime.Now.Subtract( lastRpacket ).TotalMilliseconds > RPacketIntervalSeconds * 1000 || !initialrpacketsent )
                    {
                        SendRPacketToServer();
                    }
                }
            }
        }

        //   client sends R packet to server.  This requests the server to send it the shared key, and contains a temporary key to authenticate the server reply
        //    neither client or server knows key at this point
        //    Packet format:   [int32 xxx][short xxx][char 'R'][temporary client key]
        void SendRPacketToServer()
        {
            LogFile.WriteLine( "Sending R packet to server key " + tempclientkey + " ...");
            byte[] packet = new byte[4];
            int tempnextposition = 0;
            binarypacker.WriteValueToBuffer(packet, ref tempnextposition, tempclientkey);
            parent.SendNonAckable( 'R', packet );
            lastRpacket = DateTime.Now;
            initialrpacketsent = true;
        }
        
        int GenerateSharedKey()
        {
            return rand.GetRandomInt( 0, int.MaxValue - 1 );
        }
        
        public void Tick()
        {
            if( Validated )
            {
                return;
            }
            SendRPacketToServerIfNecessary();
        }
    }
}
