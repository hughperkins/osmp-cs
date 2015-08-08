// this is a template for making NUnit tests.  Text enclosed 
// in square brackets (and the brackets themselves) should be 
// replaced by appropiate code.
//
// [File Name].cs - NUnit Test Cases for [explain here]
//
// [Author Name] ([Author email Address])
//
// (C) [Copyright holder]
// 

// these are the standard namespaces you will need.  You may 
// need to add more depending on your tests.


using System;
using NUnit.Framework;

// all test namespaces start with "Testing."  Append the 
// Namespace that contains the class you are testing, e.g. 
// Metaverse.Utility
namespace Testing.|namespace|
{
	[TestFixture]
	// the class name should end with "Test" and start with the name 
    	// of the class you are testing, e.g. CollectionBaseTest
	public class |ClassName|Test 
	{
	
		// this method is run before each Test* method is called. You 
		// can put variable initialization, etc. here that is common to 
		// each test.
		// Just leave the method empty if you don't need to use it.
		[SetUp]
		protected override void SetUp() {}
	
		// this method is run after each Test* method is called. You 
		// can put clean-up code, etc. here.  Whatever needs to be done 
		// after each test. Just leave the method empty if you don't need 
		// to use it.
		protected override void TearDown() {}
		

		// this is just one of probably many test methods in your test 
		// class. Each method should be simple and descriptive and
		// should begin with "Test" to avoid confusion with helper 
		// functions.  All methods in your class which have the "Test" 
		// attribute will be automagically  called by the NUnit framework.
		[Test]
		public void Test|Name|() 
		{
			// inside here you will exercise your class and then 
			// call Assert()
		}
	}

