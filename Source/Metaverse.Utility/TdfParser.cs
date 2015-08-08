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
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Metaverse.Utility {
    // note: treats file as UTF8.  International charsets possible.  Yay!
    // note: section and key names are key insensitive.  Great!
    public class TdfParser
    {
        public string rawtdf = "";
        public TdfParser()
        {
        }
        public TdfParser(string tdfcontents)
        {
            this.rawtdf = tdfcontents;
            Parse();
        }

        public static TdfParser FromFile(string filename)
        {
            string rawtdf = "";
            StreamReader sr = new StreamReader(filename, Encoding.UTF8);
            rawtdf = sr.ReadToEnd();
            sr.Close();
            return new TdfParser(rawtdf);
        }
        public static TdfParser FromBuffer(byte[] data, int size)
        {
            return new TdfParser( Encoding.UTF8.GetString(data, 0, size) );
        }

        string[] splitrawtdf;

        void GenerateSplitArray()
        {
            splitrawtdf = rawtdf.Split("\n".ToCharArray());
            for (int i = 0; i < splitrawtdf.GetLength(0); i++)
            {
                splitrawtdf[i] = splitrawtdf[i].Trim();
            }
        }

        enum State
        {
            InSectionHeader,
            NormalParse
        }

        State currentstate;
        Section currentsection;
        public Section RootSection;
        int level;

        public class Section
        {
            public string Name;
            public Section Parent = null;
            public Dictionary<string, Section> SubSections = new Dictionary<string, Section>(); // section by sectionname
            public Dictionary<string, string> Values = new Dictionary<string, string>(); // we dont bother parsing the values, that's done by the relevant parse
            // the reason is that only the client knows what type the variable actually is
            // ( unless we supply a DTD or equivalent )

            public Section SubSection(string name)
            {
                try
                {
                    return GetSectionByPath(name);
                }
                catch
                {
                    return null;
                }
            }

            public Section()
            {
            }
            public Section(string name)
            {
                this.Name = name;
            }
            public double GetDoubleValue(string name)
            {
                return GetDoubleValue(0, name);
            }
            public int GetIntValue(string name)
            {
                return GetIntValue(0, name);
            }
            public string GetStringValue(string name)
            {
                return GetStringValue("", name);
            }
            public double[] GetDoubleArray(string name)
            {
                return GetDoubleArray(new double[] { }, name);
            }
            public double GetDoubleValue(double defaultvalue, string name)
            {
                try
                {
                    string stringvalue = GetValueByPath(name);
                    return Convert.ToDouble(stringvalue);
                }
                catch
                {
                    return defaultvalue;
                }
            }
            public int GetIntValue(int defaultvalue, string name)
            {
                try
                {
                    string stringvalue = GetValueByPath(name);
                    return Convert.ToInt32(stringvalue);
                }
                catch
                {
                    return defaultvalue;
                }
            }
            public string GetStringValue(string defaultvalue, string name)
            {
                try
                {
                    return GetValueByPath(name);
                }
                catch
                {
                    return defaultvalue;
                }
            }
            public double[] GetDoubleArray(double[] defaultvalue, string name)
            {
                try
                {
                    string stringvalue = GetValueByPath(name);
                    string[] splitvalue = stringvalue.Trim().Split(" ".ToCharArray());
                    int length = splitvalue.Length;
                    double[] values = new double[length];
                    for (int i = 0; i < length; i++)
                    {
                        values[i] = Convert.ToDouble(splitvalue[i]);
                    }
                    return values;
                }
                catch
                {
                    return defaultvalue;
                }
            }
            List<string> GetPathParts(string path)
            {
                string[] splitpath = path.Split("/".ToCharArray());
                List<string> pathparts = new List<string>();
                foreach (string subpath in splitpath)
                {
                    string[] splitsubpath = subpath.Trim().Split("\\".ToCharArray());
                    foreach (string subsubpath in splitsubpath)
                    {
                        pathparts.Add(subsubpath.Trim().ToLower());
                    }
                }
                return pathparts;
            }
            Section GetSectionByPath(string path)
            {
                List<string> pathparts = GetPathParts(path);
                Section thissection = this;
                // we're just going to walk, letting exception fly if they want
                // this is not a public function, and we'll catch the exception in public function
                for (int i = 0; i < pathparts.Count; i++)
                {
                    thissection = thissection.SubSections[pathparts[i]];
                }
                return thissection;
            }
            string GetValueByPath(string path)
            {
                List<string> pathparts = GetPathParts(path);
                Section thissection = this;
                // we're just going to walk, letting exception fly if they want
                // this is not a public function, and we'll catch the exception in public function
                for (int i = 0; i < pathparts.Count - 1; i++)
                {
                    thissection = thissection.SubSections[pathparts[i]];
                }
                return thissection.Values[pathparts[pathparts.Count - 1]];
            }
        }
        void ParseLine(ref int linenum, string line)
        {
            //Console.WriteLine("ParseLine " + linenum + " " + line);
            switch (currentstate)
            {
                case State.NormalParse:
                    {
                        if (line.IndexOf("[") == 0)
                        {
                            currentstate = State.InSectionHeader;
                            string sectionname = ( (line.Substring(1) + "]").Split("]".ToCharArray())[0] ).ToLower();
                            Section subsection = new Section( sectionname );
                            subsection.Parent = currentsection;
                            if (!currentsection.SubSections.ContainsKey(sectionname))
                            {
                                currentsection.SubSections.Add(sectionname, subsection);
                            }
                            else
                            {
                                // silently ignore
                            }
                           // Console.WriteLine("section header found: " + sectionname);
                            currentsection = subsection;
                        }
                        else if( line.IndexOf("}") == 0 )
                        {
                            level--;
                           // Console.WriteLine("section } found, new level:" + level);
                            if (currentsection.Parent != null)
                            {
                                currentsection = currentsection.Parent;
                            }
                            else
                            {
                                // silently ignore
                            }
                        }
                        else if( line != "" )
                        {
                            if (line.IndexOf("//") != 0 && line.IndexOf("/*") != 0)
                            {
                                int equalspos = line.IndexOf("=");
                                if (equalspos >= 0)
                                {
                                    string valuename = line.Substring(0, equalspos).ToLower();
                                    string value = line.Substring(equalspos + 1);
                                    value = (value + ";").Split(";".ToCharArray())[0]; // remove trailing ";"
                                   // Console.WriteLine("   value found [" + valuename + "] = [" + value + "]");
                                    if (!currentsection.Values.ContainsKey(valuename))
                                    {
                                        currentsection.Values.Add(valuename, value);
                                    }
                                }
                            }
                        }
                        break;
                    }

                case State.InSectionHeader:
                    {
                        if (line.IndexOf("{") == 0)
                        {
                            currentstate = State.NormalParse;
                            level++;
                           // Console.WriteLine("section { found, new level:" + level);
                        }
                        break;
                    }
            }
        }

        void Parse()
        {
            GenerateSplitArray();
            RootSection = new Section(); ;
            level = 0;
            currentsection = RootSection;
            currentstate = State.NormalParse;
            for (int linenum = 0; linenum < splitrawtdf.GetLength(0); linenum++)
            {
                ParseLine(ref linenum, splitrawtdf[linenum]);
            }
        }
    }

}
