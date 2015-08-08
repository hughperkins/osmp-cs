using System;
using System.Collections.Generic;
using System.Text;

namespace RpcProxyGenerator
{
    public class EntryPoint
    {
        public static void Main(string[] args)
        {
            try
            {
                //if (args.GetLength(0) < 3)
                //{
                  //  Console.WriteLine("Usage: RpcProxyBuilder <assembly filepath> <namespacename> <interfacename>");
                    //return;
                //}
                //string assemblyfilepath = args[0];
                //string interfacename = args[1];
                //string targettypename = args[2];

                // new NetworkProxyBuilder().Go(assemblyfilepath, interfacename, targettypename);

                new RunGenerator().Go();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
