//
// BinaryPackerTest.cs - NUnit Test Cases for BinaryPacker class
//
// Daedius (richard.anaya@gmail.com)
//
// (C) Richard Anaya
// 

using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using Metaverse.Utility;
using System;
using NUnit.Framework;
using OSMP;

// all test namespaces start with "Testing."  Append the 
// Namespace that contains the class you are testing, e.g. 
// Metaverse.Utility
namespace Testing.Metaverse.Utility
{
	[TestFixture]
	// the class name should end with "Test" and start with the name 
    	// of the class you are testing, e.g. CollectionBaseTest
	public class BinaryPackerTest 
	{
		
		[AttributeUsage( AttributeTargets.Field | AttributeTargets.Property )]
        class AttributePack : Attribute
        {
        }

        [StructLayout( LayoutKind.Sequential )]
        class TestClass
        {
            public int[] indexes;
            public string name;
            string country;
            [AttributePack]
            public string Country { get { return country; } set { country = value; } }
            public bool booleanvaluetrue;
            public bool booleanvaluefalse;

            public char charvalue;
            public int intvalue;
            public double doublevalue;
            public ChildClass childclass;
        }

        [StructLayout(LayoutKind.Sequential)]
        class ChildClass
        {
            public string name;
        }
	
		// this method is run before each Test* method is called. You 
		// can put variable initialization, etc. here that is common to 
		// each test.
		// Just leave the method empty if you don't need to use it.
		[SetUp]
		protected void SetUp() {}
	
		// this method is run after each Test* method is called. You 
		// can put clean-up code, etc. here.  Whatever needs to be done 
		// after each test. Just leave the method empty if you don't need 
		// to use it.
		 [TearDown]
		protected void TearDown() {}
		

		// this is just one of probably many test methods in your test 
		// class. Each method should be simple and descriptive and
		// should begin with "Test" to avoid confusion with helper 
		// functions.  All methods in your class which have the "Test" 
		// attribute will be automagically  called by the NUnit framework.
		[Test]
		public void TestGeneralBinaryPackerFunctionality() 
		{
			
			byte[] bytearray = new byte[1024];
	            	int position = 0;
		       TestClass testclass = new TestClass();
			testclass.booleanvaluetrue = true;
		       testclass.charvalue = 'C';
			testclass.doublevalue = 123.4567890;
			testclass.intvalue = 1234567890;
			testclass.Country = "Spain";
			testclass.name = "Test class name";
			testclass.indexes = new int[] { 5, 1, 4, 2, 3 };
			testclass.childclass = new ChildClass();
			testclass.childclass.name = "name inside child class";
			
			BinaryPacker bp = new BinaryPacker();
			
			//Write several objects into the buffer
			bp.WriteValueToBuffer(bytearray, ref position, testclass);
			bp.WriteValueToBuffer(bytearray, ref position, "The quick brown fox.");
			bp.WriteValueToBuffer(bytearray, ref position, "Rain in Spain.");
			
			bp.PackObjectUsingSpecifiedAttributes(bytearray, ref position, testclass, new Type[] { typeof(AttributePack) });
		
			byte[] bytearraytowrite = Encoding.UTF8.GetBytes("Hello world");
			bp.WriteValueToBuffer(bytearray, ref position, bytearraytowrite);
		
			
			FractalSplineBox box = new FractalSplineBox();
			
			box.name = "Test box";
			
			
			box.pos = new Vector3(1, 123.123, 150.150);
			
			bp.PackObjectUsingSpecifiedAttributes(bytearray, ref position, box, new Type[] { typeof(Replicate) });
			
			position = 0;
			
			//verify all the items we put in
			TestClass outputobject = (TestClass)new BinaryPacker().ReadValueFromBuffer(bytearray, ref position, typeof(TestClass));
			
			string error = "Trouble unpacking TestClass from BinaryPacker";
			Assert.AreEqual( true, outputobject.booleanvaluetrue, error );
			Assert.AreEqual( 'C', outputobject.charvalue,  error );
			Assert.AreEqual( 123.4567890, outputobject.doublevalue,   error );
			Assert.AreEqual( 1234567890 , outputobject.intvalue,  error);
			Assert.AreEqual( "Spain" , outputobject.Country,  error);
			Assert.AreEqual( "Test class name", outputobject.name,  error);
			Assert.AreEqual( 5, outputobject.indexes.Length, error);
			Assert.AreEqual( 5, outputobject.indexes[0], error);
			Assert.AreEqual( 1, outputobject.indexes[1], error);
			Assert.AreEqual( 4, outputobject.indexes[2],  error);
			Assert.AreEqual( 2, outputobject.indexes[3],  error);
			Assert.AreEqual( 3, outputobject.indexes[4],  error);
			Assert.AreEqual( "name inside child class", outputobject.childclass.name,  error);

			
			string sout = (string)new BinaryPacker().ReadValueFromBuffer(bytearray, ref position, typeof(string));
		        Assert.AreEqual( "The quick brown fox.", sout, "Could not unpack string");
			
			sout = (string)new BinaryPacker().ReadValueFromBuffer(bytearray, ref position, typeof(string));
			Assert.AreEqual( "Rain in Spain.", sout, "Could not unpack second string");
			
			outputobject = new TestClass();
			bp.UnpackIntoObjectUsingSpecifiedAttributes(bytearray, ref position, outputobject, new Type[] { typeof(AttributePack) });
			Assert.AreEqual( null, outputobject.name, "Error in value of unset value in attribute packing"); // should be blank, because name member has a AttributePack attribute
			Assert.AreEqual( "Spain", outputobject.Country, "Error in value of set value in attribute packing" ); //Country does have an AttributePack attribute

			bytearraytowrite = (byte[])new BinaryPacker().ReadValueFromBuffer(bytearray, ref position, typeof(byte[]));
			Assert.AreEqual( "Hello world", Encoding.UTF8.GetString(bytearraytowrite), "Error in unpacking UTF8"  );
			FractalSplineBox boxobject = new FractalSplineBox();
			new BinaryPacker().UnpackIntoObjectUsingSpecifiedAttributes(bytearray, ref position, boxobject, new Type[] { typeof(Replicate) });
			Assert.AreEqual( "Test box", boxobject.name, "Error in unpacking FractelSplineBox name" );
			Assert.AreEqual( new Vector3(1,123.123, 150.150),  box.pos, "Error in unpacking FractelSplineBox position" );
			
		}
	}
}

    /*
    public class BinaryPackerTest : TestCase {
	
	// there should be two constructors for your class.  The first 
	// one (without parameters) should set the name to something 
	// unique.
	// Of course the name of the method is the same as the name of 
	// the class
	public BinaryPackerTest() : base ("[Namespace.Class]") {}
	public BinaryPackerTest(string name) : base(name) {}

	// this method is run before each Test* method is called. You 
	// can put variable initialization, etc. here that is common to 
	// each test.
	// Just leave the method empty if you don't need to use it.
	protected override void SetUp() {}

	// this method is run after each Test* method is called. You 
	// can put clean-up code, etc. here.  Whatever needs to be done 
	// after each test. Just leave the method empty if you don't need 
	// to use it.
	protected override void TearDown() {}

	
	public void TestBinaryPacker() {
		Assert.IsTrue( true );
	}
    }
}














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



/*namespace OSMP
{
    // call this to test binary packer
    [TestFixture]
    class TestBinaryPacker
    {
        

        [Test]
        public void BinaryPackerTest()
        {
        	
        	
         
            
            
        }
    }
}
*/
