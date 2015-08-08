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

//! \file
//! \brief Used to read config from config.xml

using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

namespace Metaverse.Utility
{
    public class CommandCombo
    {
        public string command;
        public List<string> keycombo;
        public CommandCombo() { }
        public CommandCombo(string command, List<string> keycombo)
        {
            this.command = command;
            this.keycombo = keycombo;
        }
        public bool Matches(List<string> keynames)
        {
            foreach (string keyname in keycombo)
            {
                if (!keynames.Contains(keyname))
                {
                    return false;
                }
            }
            return true;
        }
    }

    public class MouseMoveConfig
    {
        public string VerticalAxis = "";
        public string HorizontalAxis = "";
        public string Zoom = "";
        public bool InvertVertical = false;
        public bool InvertScroll = false;
        public MouseMoveConfig() { }
        public MouseMoveConfig(string verticalaxis, string horizontalaxis)
        {
            this.VerticalAxis = verticalaxis;
            this.HorizontalAxis = horizontalaxis;
        }
        public MouseMoveConfig(string verticalaxis, string horizontalaxis, string zoom, bool InvertVertical, bool InvertScroll)
        {
            this.VerticalAxis = verticalaxis;
            this.HorizontalAxis = horizontalaxis;
            this.Zoom = zoom;
            this.InvertVertical = InvertVertical;
            this.InvertScroll = InvertScroll;
        }
    }

    public class Config
    {
        string sFilePath = "config.xml";
        
        public int iDebugLevel;

        public Coordination coordination;

        public string ServerIPAddress = "";
        public int ServerPort = 2501;
            
        public int world_xsize;
        public int world_ysize;
        public double mingroundheight;
        public double maxgroundheight;
        public double ceiling;

        public List<CommandCombo> CommandCombos = new List<CommandCombo>();
        public Dictionary<string, MouseMoveConfig> MouseMoveConfigsByName = new Dictionary<string, MouseMoveConfig>();

        public int mousescrollmultiplier = 50; // makes mouse scroll value comparable with mousex and y movements

        XmlDocument configdoc;
        public XmlElement clientconfig;

        public int windowwidth;
        public int windowheight;

        public double HeightEditingSpeed;
        public int brushsize;

        static Config instance = new Config();
        public static Config GetInstance()
        {
            return instance;
        }
        
        public Config()
        {
            RefreshConfig();
        }

        public static double GetDouble(XmlElement xmlelement, string attributename)
        {
            try
            {
                double value = Convert.ToDouble(xmlelement.GetAttribute(attributename));
                return value;
            }
            catch
            {
                //EmergencyDialog.WarningMessage("In the config.xml file, the value " + attributename + " in section " + xmlelement.Name + " needs to be a number.  MapDesigner may not run correctly.");
                return 0;
            }
        }

        public static int GetInt( XmlElement xmlelement, string attributename )
        {
            try
            {
                int value = Convert.ToInt32(xmlelement.GetAttribute(attributename));
                return value;
            }
            catch
            {
                //EmergencyDialog.WarningMessage("In the config.xml file, the value " + attributename + " in section " + xmlelement.Name + " needs to be a whole number.  MapDesigner may not run correctly.");
                return 0;
            }
        }

        // <coordination ircserver="irc.gamernet.org" ircport="6667" ircchannel="#osmp" stunserver="stun.fwdnet.net" />
        public class Coordination
        {
            public string ircserver;
            public int ircport;
            public string ircchannel;
            public string stunserver;
            public Coordination() { }
            public Coordination( XmlElement coordinationelement )
            {
                ircserver = coordinationelement.GetAttribute( "ircserver" );
                ircport = GetInt( coordinationelement, "ircport" );
                ircchannel = coordinationelement.GetAttribute( "ircchannel" );
                stunserver = coordinationelement.GetAttribute( "stunserver" );
            }
        }

        public void RefreshConfig()
        {
            Test.Debug( "reading config.xml ..." );
            configdoc = XmlHelper.OpenDom( EnvironmentHelper.GetExeDirectory() + "/" + sFilePath );
        
            XmlElement systemnode = (XmlElement)configdoc.DocumentElement.SelectSingleNode( "config");
            iDebugLevel = Convert.ToInt32( systemnode.GetAttribute("debuglevel") );
            Test.Debug("DebugLevel " + iDebugLevel.ToString() );
        
            clientconfig = (XmlElement)configdoc.DocumentElement.SelectSingleNode( "client");

            coordination = new Coordination( (XmlElement)configdoc.DocumentElement.SelectSingleNode( "coordination") );

            XmlElement displaynode = clientconfig.SelectSingleNode("display") as XmlElement;
            windowwidth = GetInt(displaynode, "width");
            windowheight = GetInt(displaynode, "height");

            XmlElement worldnode = clientconfig.SelectSingleNode( "world" ) as XmlElement;
            world_xsize = GetInt( worldnode, "x_size" );
            world_ysize = GetInt( worldnode, "y_size" );
            mingroundheight = GetDouble( worldnode, "mingroundheight" );
            maxgroundheight = GetDouble( worldnode, "maxgroundheight" );
            ceiling = GetDouble( worldnode, "ceiling" );

            XmlElement heighteditingnode = clientconfig.SelectSingleNode( "heightediting" ) as XmlElement;
            HeightEditingSpeed = GetDouble( heighteditingnode, "speed" );
            brushsize = GetInt( heighteditingnode, "defaultbrushsize" );

            XmlElement servernode = (XmlElement)configdoc.DocumentElement.SelectSingleNode("server");
            ServerPort = Convert.ToInt32( servernode.GetAttribute("port"));
            ServerIPAddress = servernode.GetAttribute("ipaddress");

            foreach (XmlElement mappingnode in clientconfig.SelectNodes("keymappings/key"))
            {
                string sCommand = mappingnode.GetAttribute("command");
                string sKeyCodes = mappingnode.GetAttribute("keycode");
                string[] KeyCodes = sKeyCodes.Split("-".ToCharArray());
                List<string> keycodelist = new List<string>(KeyCodes);
                CommandCombos.Add(new CommandCombo(sCommand, keycodelist));
            }
            foreach (XmlElement mousemovenode in clientconfig.SelectNodes("mousemoveconfigs/mousemove"))
            {
                string name = mousemovenode.GetAttribute("name");
                string vertical = mousemovenode.GetAttribute("vertical");
                string horizontal = mousemovenode.GetAttribute("horizontal");
                string zoom = mousemovenode.GetAttribute("zoom");
                bool invertvertical = false;
                bool invertscroll = false;
                if (mousemovenode.HasAttribute("invertvertical") && mousemovenode.GetAttribute("invertvertical") == "yes")
                {
                    invertvertical = true;
                }
                if (mousemovenode.HasAttribute("invertscroll") && mousemovenode.GetAttribute("invertscroll") == "yes")
                {
                    invertscroll = true;
                }
                if (!MouseMoveConfigsByName.ContainsKey(name))
                {
                    MouseMoveConfigsByName.Add(name, new MouseMoveConfig(vertical, horizontal, zoom, invertvertical, invertscroll));
                }
            }
        
            Test.Debug("... config.xml read");
        }
        
        public string GetFactoryTargetClassname( string sfactoryname )
        {
            try
            {
                XmlElement factorynode = (XmlElement)configdoc.SelectSingleNode("root/factories/factory[name='" + sfactoryname.ToLower() + "']" );
                return factorynode.GetAttribute("select");
            }
            catch( Exception e )
            {
                LogFile.WriteLine( e.ToString() );
                return "";
            }
        }
    }
}
