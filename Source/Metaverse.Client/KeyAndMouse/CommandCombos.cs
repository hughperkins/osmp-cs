// Copyright Hugh Perkins 2006
// hughperkins@gmail.com http://manageddreams.com
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
using System.Text;
using Metaverse.Utility;

namespace OSMP
{
    public delegate void KeyCommandHandler(string command, bool commanddown);

    public class CommandCombos
    {
        abstract class Registration
        {
            public KeyCommandHandler handler;
            public virtual void Check() { }
        }
        class RegistrationExact : Registration
        {
            bool isdown = false; // state from previous check
            public string command;
            public RegistrationExact(string command, KeyCommandHandler handler) { this.command = command; this.handler = handler; }
            public override void Check()
            {
                bool newdown = GetNewDown();
                if (newdown)
                {
                    if (!isdown)
                    {
                        LogFile.WriteLine("Registration down: " + command);
                        handler(command, true);
                    }
                }
                else
                {
                    if (isdown)
                    {
                        LogFile.WriteLine("Registration up: " + command);
                        handler(command, false);
                    }
                }
                isdown = newdown;
            }
            bool GetNewDown()
            {
                if (!KeyCombos.GetInstance().IsPressed(command))
                {
                    return false;
                }
                List<string> allkeysforcommand = new List<string>();
                foreach (CommandCombo commandcombo in Config.GetInstance().CommandCombos)
                {
                    if (commandcombo.command == command)
                    {
                        foreach (string key in commandcombo.keycombo)
                        {
                            if (!allkeysforcommand.Contains(key))
                            {
                                allkeysforcommand.Add(key);
                            }
                        }
                    }
                }

                // check if eligible commands have any extra keys
                foreach (string key in KeyNameCache.GetInstance().keynamesdown)
                {
                    if (!allkeysforcommand.Contains(key))
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        class RegistrationAtLeastCommand : Registration
        {
            bool isdown = false; // state from previous check
            public string command;
            public RegistrationAtLeastCommand(string command, KeyCommandHandler handler) { this.command = command; this.handler = handler; }

            public override void Check()
            {
                bool newdown = KeyCombos.GetInstance().IsPressed( command );
                if (newdown )
                {
                    if (!isdown)
                    {
                        //LogFile.WriteLine("Registration down: " + command);
                        handler(command, true);
                    }
                }
                else
                {
                    if (isdown)
                    {
                        //LogFile.WriteLine("Registration up: " + command);
                        handler(command, false);
                    }
                }
                isdown = newdown;
            }
        }
        class RegistrationCommandGroup : Registration
        {
            List<string> commands = new List<string>();
            //KeyCommandHandler handler;

            List<string> currentdowncommands = new List<string>();

            public RegistrationCommandGroup(string[] commands, KeyCommandHandler handler)
            {
                //this.commands = commands;
                foreach (string command in commands)
                {
                    this.commands.Add(command);
                }
                this.handler = handler;
            }
            // we filter all keys out that dont match our commands
            // then we check if any commands exactly match the down keys, with no additional keys
            public override void Check()
            {
                // get allowed keys
                List<string> correspondingkeys = new List<string>();
                foreach (CommandCombo commandcombo in Config.GetInstance().CommandCombos )
                {
                    if (commands.Contains(commandcombo.command))
                    {
                        foreach (string key in commandcombo.keycombo)
                        {
                            if (!correspondingkeys.Contains(key))
                            {
                                correspondingkeys.Add(key);
                            }
                        }
                    }
                }
                // get eligible commands, must be down
                List<string>newdowncommands = new List<string>();
                foreach( string command in commands )
                {
                    if( KeyCombos.GetInstance().IsPressed( command ) )
                    {
                        newdowncommands.Add( command );
                    }
                }

                // build list of all possible keys for each command
                Dictionary<string, List<string>> allkeysforeachcommand = new Dictionary<string, List<string>>();
                foreach (CommandCombo commandcombo in Config.GetInstance().CommandCombos)
                {
                    if (newdowncommands.Contains(commandcombo.command))
                    {
                        if (!allkeysforeachcommand.ContainsKey(commandcombo.command))
                        {
                            allkeysforeachcommand.Add(commandcombo.command, new List<string>());
                        }
                        foreach (string key in commandcombo.keycombo)
                        {
                            if (!allkeysforeachcommand[commandcombo.command].Contains(key))
                            {
                                allkeysforeachcommand[commandcombo.command].Add(key);
                            }
                        }
                    }
                }

                // check if eligible commands have any extra keys
                foreach (string key in KeyNameCache.GetInstance().keynamesdown )
                {
                    if (correspondingkeys.Contains(key))
                    {
                        foreach (string command in allkeysforeachcommand.Keys)
                        {
                            if (!allkeysforeachcommand[command].Contains(key))
                            {
                                if (newdowncommands.Contains(command))
                                {
                                    newdowncommands.Remove(command);
                                }
                            }
                        }
                    }
                }

                foreach (string command in currentdowncommands)
                {
                    if (!newdowncommands.Contains(command))
                    {
                        //LogFile.WriteLine("Commandcombo up: " + command);
                        handler(command, false);
                    }
                }
                foreach (string command in newdowncommands)
                {
                    if (!currentdowncommands.Contains(command))
                    {
                        //LogFile.WriteLine("Commandcombo down: " + command);
                        handler(command, true);
                    }
                }
                currentdowncommands = newdowncommands;
            }
        }


        static CommandCombos instance = new CommandCombos();
        public static CommandCombos GetInstance() { return instance; }

        List<Registration> registrations = new List<Registration>();
        //List<Registration> registrationsdown = new List<Registration>();

        CommandCombos()
        {
            KeyCombos.GetInstance().ComboDown += new KeyCombos.ComboDownHandler(CommandCombos_ComboDown);
            KeyCombos.GetInstance().ComboUp += new KeyCombos.ComboUpHandler(CommandCombos_ComboUp);
        }

        void CommandCombos_ComboUp(string command)
        {
            CheckCombos();
        }

        void CommandCombos_ComboDown(string command)
        {
            CheckCombos();
        }

        void CheckCombos()
        {
            foreach (Registration registration in registrations)
            {
                //LogFile.WriteLine("checking registration " + registration);
                registration.Check();
            }
        }

        Dictionary<string, List<KeyCommandHandler>> CommandHandlers = new Dictionary<string, List<KeyCommandHandler>>();

        /// <summary>
        /// Register a single command, if the keys are pressed this is signalled as down
        /// regardless of any extra keys that may be present
        /// </summary>
        /// <param name="command"></param>
        /// <param name="handler"></param>
        public void RegisterAtLeastCommand(string command, KeyCommandHandler handler)
        {
            registrations.Add(new RegistrationAtLeastCommand(command, handler));
        }

        /// <summary>
        /// register a single command.  Extraneous keys will negate the down state
        /// </summary>
        /// <param name="command"></param>
        /// <param name="handler"></param>
        public void RegisterExactCommand(string command, KeyCommandHandler handler)
        {
            registrations.Add(new RegistrationExact(command, handler));
        }

        /// <summary>
        /// Register a group of mutually exclusive commands
        /// eg editpos vs editrot vs editscale
        /// </summary>
        /// <param name="commands"></param>
        /// <param name="handler"></param>
        public void RegisterCommandGroup(string[] commands, KeyCommandHandler handler)
        {
            registrations.Add(new RegistrationCommandGroup(commands, handler));
        }
    }
}
