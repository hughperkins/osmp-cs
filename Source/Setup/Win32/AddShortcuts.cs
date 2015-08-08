using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;

//using MSjogren.Samples.ShellLink;

// add shortcut to start menu
    class AddShortcuts
    {
        void CreateShortcut( string shortcutpath, string targetpath, string arguments, string iconpath )
        {
            ProcessStartInfo psi = new ProcessStartInfo( EnvironmentHelper.GetExeDirectory() + "/shortcut.exe" );
            psi.Arguments = "/F:\"" + shortcutpath + "\" /A:C /T:\"" + targetpath + "\" /P:\"" +
                arguments.Replace( "\"", "\"\"" ) + "\" /I:\"" + iconpath + "\"";
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;

            Process process = new Process();
            process.StartInfo = psi;
            process.Start();
        }

        public void Go()
        {
            string targetpath = "";
            string arguments = "";

            if (EnvironmentHelper.IsMonoRuntime)
            {
                targetpath = EnvironmentHelper.GetClrDirectory() + "\\mono.exe";
                arguments = "--debug \"" + EnvironmentHelper.GetExeDirectory() + "\\metaverse.exe\"";
            }
            else
            {
                targetpath = EnvironmentHelper.GetExeDirectory() + "\\metaverse.exe";
            }

            string shortcutpath = System.Environment.GetFolderPath(Environment.SpecialFolder.Programs ) +
                "\\Osmp\\Osmp.lnk";
            Directory.CreateDirectory( Path.GetDirectoryName( shortcutpath ) );
            //shortcutpath = "d:\\test.lnk";
            Console.WriteLine( shortcutpath );

            CreateShortcut( shortcutpath, targetpath, arguments, EnvironmentHelper.GetExeDirectory() + "\\Metaverse.ico" );
        }
    }
