using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;

namespace Setup
{
    // adds osmp:// protocol, win32 only
    // assume setup.exe was launched from directory containing metaverse.client.exe
    class AddOsmpProtocol
    {
        public void Go()
        {
            string metaversedirectory = EnvironmentHelper.GetExeDirectory();
            string metaverseclientexe = "\"" + metaversedirectory + "/metaverse.exe\"";
            if (EnvironmentHelper.IsMonoRuntime)
            {
                metaverseclientexe = "\"" + EnvironmentHelper.GetClrDirectory() + "\\mono.exe\" --debug " +
                    metaverseclientexe;
            }

            RegistryKey osmpkey = Registry.ClassesRoot.CreateSubKey( "osmp" );
            osmpkey.SetValue( "", "URL:OSMP Protocol", RegistryValueKind.String );
            osmpkey.SetValue( "URL Protocol", "", RegistryValueKind.String );

            RegistryKey defaulticonkey = osmpkey.CreateSubKey( "DefaultIcon" );
            defaulticonkey.SetValue( "", metaversedirectory + "\\Metaverse.ico", RegistryValueKind.String );

            RegistryKey shellkey = osmpkey.CreateSubKey( "shell" );
            RegistryKey openkey = shellkey.CreateSubKey( "open" );
            RegistryKey commandkey = openkey.CreateSubKey( "command" );
            commandkey.SetValue( "", metaverseclientexe + " -url \"%1\"" );
        }
    }
}
