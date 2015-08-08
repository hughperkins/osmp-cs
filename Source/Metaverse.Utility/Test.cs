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

namespace Metaverse.Utility
{
    public class Test
    {
        public static int iDebugLevel = 3;
            
        public static void Init()
        {
            iDebugLevel = Config.GetInstance().iDebugLevel;
        }
        
        public static string ToString( object item )
        {
            if( item.GetType() == typeof( bool ) )
            {
                if( (bool)item )
                {
                      return "#";
                }
                else
                {
                      return "-";
                }
            }
            else if( item.GetType().IsArray && ( (Array)item ).Rank == 1 )
            {
                string output = "";
                foreach( object subitem in (Array)item )
                {
                    output += subitem.ToString() + ", ";
                }
                return output;
            }
            else
            {
                return item.ToString();
            }
        }
        
        public static object GetArraySlice( Array inputarray, int sliceindex )
        {
            int numoutputdimensions = inputarray.Rank - 1;
            int[] newdimensions = new int[ numoutputdimensions ];
            for( int i = 1; i < numoutputdimensions + 1; i++ )
            {
                newdimensions[ i - 1 ] = inputarray.GetUpperBound( i ) + 1;
            }
            Array newarray = Array.CreateInstance( inputarray.GetType().GetElementType(), newdimensions );
            
            int[]traverseinputindex = new int[ numoutputdimensions + 1 ];
            int[]traverseoutputindex = new int[ numoutputdimensions ];
            traverseinputindex[0] = sliceindex;
            bool bDone = false;
            while( !bDone )
            {
                newarray.SetValue( inputarray.GetValue( traverseinputindex ), traverseoutputindex );
                bool bUpdatedtraverseindex = false;
                for( int i = numoutputdimensions - 1; i >= 0 && !bUpdatedtraverseindex; i-- )
                {
                    traverseinputindex[i + 1]++;
                    traverseoutputindex[i]++;
                    if( traverseoutputindex[i] >= newdimensions[i] )
                    {
                        if( i == 0 )
                        {
                            bDone = true;
                        }
                        else
                        {
                            traverseinputindex[i + 1] = 0;
                            traverseoutputindex[i ] = 0; 
                        }
                    }
                    else
                    {
                        bUpdatedtraverseindex = true;
                    }
                }
            }
            
            return newarray;
        }
        
        public static void WriteOut( object inputobject )
        {
            if( inputobject.GetType().IsArray )
            {
                Array inputarray = (Array)inputobject;
                if( inputarray.Rank == 1 )
                {
                    string sCombinedString = "";
                    foreach( object item in inputarray )
                    {
                        if( item.GetType() == typeof( bool ) )
                        {
                             sCombinedString += ToString( item );
                        }
                        else
                        {
                             sCombinedString += ToString( item ) + ", ";
                        }
                    }
                    Debug( sCombinedString );
                }
                else if(  inputarray.Rank == 2 )
                {
                    for( int i = 0; i < inputarray.GetUpperBound(0) + 1; i++ )
                    {
                        Test.WriteOut( GetArraySlice(inputarray, i) );
                    }
                }
                else if(  inputarray.Rank == 3 )
                {
                    for( int k = 0; k < inputarray.GetUpperBound(0) + 1; k++ )
                    {
                        LogFile.WriteLine( "i = " + k.ToString() );
                        for( int i = 0; i < inputarray.GetUpperBound(1) + 1; i++ )
                        {
                            string sLine = "";
                            for( int j = 0; j < inputarray.GetUpperBound(2) + 1; j++ )
                            {
                                sLine += ToString( inputarray.GetValue( new int[]{k,i,j}) ) + ", ";
                            }
                            Debug( sLine );
                        }
                        Debug("");
                    }
                }
            }
            else if( inputobject.GetType() == typeof( ArrayList ) )
            {
                string sCombinedString = "";
                foreach( object item in (ArrayList)inputobject )
                {
                    sCombinedString += item.ToString() + ", ";
                }
                Debug( sCombinedString );			
            }
            else if( typeof( ICollection ).IsInstanceOfType( inputobject ) )
            {
                string sCombinedString = "";
                foreach( object item in (ICollection)inputobject )
                {
                    if( item.GetType() == typeof( DictionaryEntry ) )
                    {
                         sCombinedString += "{" + ((DictionaryEntry)item).Key.ToString() + ", " + ((DictionaryEntry)item).Value.ToString() + "},";
                    }
                    else
                    {
                         sCombinedString += item.ToString() + ", ";
                    }
                }
                Debug( sCombinedString );			
            }
            else
            {
                Debug( inputobject );
            }
        }
//        public static void WriteOut( string message )
   //     {
      //      if( iDebugLevel >= 3 )
         //   {
            //    LogFile.WriteLine( "Debug: " + message );
           // }
       // }
        public static void Debug( object message )
        {
            if( iDebugLevel >= 3 )
            {
                LogFile.WriteLine( "Debug: " + message.ToString() );
            }
        }
        public static void Info( string message )
        {
            LogFile.WriteLine( "Info: " + message );
        }
        public static void Warning( string message )
        {
            LogFile.WriteLine( "Warning: " + message );
        }
        public static void Error( string message )
        {
            LogFile.WriteLine( "ERROR: " + message );
        }
    }
}

