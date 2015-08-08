// Copyright Hugh Perkins 2006
// hughperkins at gmail http://hughperkins.com
//
// This program is free software; you can redistribute it and/or modify it
// under the terms of the GNU General Public License as published by the
// Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
// or FITNESS FOR A PARTICULAR PURVector3E. See the GNU General Public License for
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
using System.Collections.Generic;
using SdlDotNet;
using Metaverse.Utility;

namespace OSMP
{
    // This class points to the renderer obtained by RendererFactory
    // It converts the key codes returned by the renderer, which are assumed to be in System.Windows.Forms format
    // into the commandstrings defined in the configuration file (config.xml, via Config.cs )
    // This is a critical part of the keyboard chain, because it handles keycode combos, eg "shift-escape" is converted into "quit", based on current config.xml
    // This reads the mousebuttons too, so it's possible to include the mousebuttons in these combos, eg "leftmousebutton"
    public class KeyCombos
    {
        //public delegate void KeyCommandHandler(string command, bool commanddown);

        public delegate void ComboDownHandler(string command);
        public delegate void ComboUpHandler(string command);

        public event ComboDownHandler ComboDown;
        public event ComboUpHandler ComboUp;

        Config config;
        static KeyCombos instance = new KeyCombos();

        // public List<string> CurrentPressedKeys = new List<string>();
        public List<string> currentcommands = new List<string>();

        //public List<CommandCombo> currentcombos = new List<CommandCombo>();
        //public Dictionary<string, List<CommandCombo>> currentcombosbycommand = new Dictionary<string, List<CommandCombo>>();
        //public List<string> _CurrentPressedKeys = new List<String>();

        KeyCombos()
        {
            config = Config.GetInstance();

            KeyNameCache.GetInstance().KeyDown += new KeyNameCache.KeyDownHandler(KeyCombos_KeyDown);
            KeyNameCache.GetInstance().KeyUp += new KeyNameCache.KeyUpHandler(KeyCombos_KeyUp);
        }

        void KeyCombos_KeyUp(string keyname)
        {
            CheckCombos();
        }

        void KeyCombos_KeyDown(string keyname)
        {
            CheckCombos();
        }

        public static KeyCombos GetInstance()
        {
            return instance;
        }

        //public List<String> AllPressedCommandKeys{
          //  get { return _CurrentPressedKeys; }
        //}

        public bool IsPressed( string commandstring )
        {
            return currentcommands.Contains(commandstring);
            //if( currentcombosbycommand.ContainsKey( commandstring ) && currentcombosbycommand[ commandstring ].Count > 0 )
            //{
              //  return true;
            //}
            //return false;
        }

        void CheckCombos()
        {
            List<string> newcombos = new List<string>();
            foreach (CommandCombo commandcombo in config.CommandCombos)
            {
                if (commandcombo.Matches( KeyNameCache.GetInstance().keynamesdown))
                {
                    if (!newcombos.Contains(commandcombo.command))
                    {
                        newcombos.Add(commandcombo.command);
                    }
                }
            }
            List<string> combosdown = new List<string>();
            List<string> combosup = new List<string>();
            foreach (string command in newcombos)
            {
                if (!currentcommands.Contains(command))
                {
                    //LogFile.WriteLine("combo down: " + command);
                    combosdown.Add(command);
                }
            }
            foreach (string command in currentcommands)
            {
                if (!newcombos.Contains(command))
                {
                    //LogFile.WriteLine("combo up: " + command);
                    combosup.Add(command);
                }
            }
            currentcommands = newcombos;
            if (ComboUp != null)
            {
                foreach (string command in combosup)
                {
                    ComboUp(command);
                }
            }
            if (ComboDown != null)
            {
                foreach (string command in combosdown)
                {
                    ComboDown(command);
                }
            }
        }
    }
}
