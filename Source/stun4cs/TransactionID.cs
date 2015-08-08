using System;


/**
 * This class encapsulates a STUN transaction ID. It is useful for storing
 * transaction ids in collection objects as it implements the equals method.
 * It also provies a utility for creating unique transaction ids.
 *
 * <p>Organisation: <p> Louis Pasteur University, Strasbourg, France</p>
 * <p>Network Research Team (http://www-r2.u-strasbg.fr)</p></p>
 * @author Emil Ivov
 * @version 0.1
 */

namespace net.voxx.stun4cs 
{
	public class TransactionID
	{
		/**
		 * The id itself
		 */
		private byte[] transactionID = new byte[16];

		/**
		 * The object to use to generate the rightmost 8 bytes of the id.
		 */
		private static Random random = new Random();

		/**
		 * A hashcode for hashtable storage.
		 */
		private int hashCode = 0;

		private TransactionID()
		{
		}

		/**
		 * Creates a transaction id object.The transaction id itself is genereated
		 * using the folloing algorithm:
		 *
		 * The first 8 bytes of the id are given the value of System.currentTimeMillis()
		 * Putting the right most bits first so that we get a more optimized equals()
		 * method.
		 *
		 * @return A TransactionID object with a unique transaction id.
		 */
		public static TransactionID CreateTransactionID()
		{
			TransactionID tid = new TransactionID();

			long left  = DateTime.Now.Ticks;//the first 8 bytes of the id
			long right = random.Next();//the last 8 bytes of the id
			right *= 0x100000000L;
			right += Math.Abs(random.Next());

			for(int i = 0; i < 8; i++)
			{
				tid.transactionID[i]   = (byte)((left  >> (i*8))& 0xFFL);
				tid.transactionID[i+8] = (byte)((right >> (i*8))& 0xFFL);
			}

			//calculate hashcode for Hashtable storage.
			uint foo = tid.transactionID[3];
			foo <<= 24;
			foo &= 0xFF000000;
			tid.hashCode = (int) foo;
			int tmp;
			tmp = tid.transactionID[2];
			tmp <<= 16;
			tmp &= 0xFF0000;
			tid.hashCode |= tmp;
			tmp = tid.transactionID[1];
			tmp <<= 8;
			tmp &= 0xFF00;
			tid.hashCode |= tmp;
			tmp = tid.transactionID[0];
			tmp &= 0xFF;
			tid.hashCode |= tmp;

			return tid;
		}

		/**
		 * Creates a transaction identifier object with the specified id.
		 * @param transactionID the id to give to the new TransactionID
		 * @return a new TransactionID object with the specified id value;
		 */
		public static TransactionID CreateTransactionID(byte[] transactionID)
		{
			TransactionID tid = new TransactionID();

			for (int x = 0; x < 16; x++) 
			{
				tid.transactionID[x] = transactionID[x];
			}

			//calculate hashcode for Hashtable storage.
			uint foo = tid.transactionID[3];
			foo <<= 24;
			foo &= 0xFF000000;
			tid.hashCode = (int) foo;
			int tmp;
			tmp = tid.transactionID[2];
			tmp <<= 16;
			tmp &= 0xFF0000;
			tid.hashCode |= tmp;
			tmp = tid.transactionID[1];
			tmp <<= 8;
			tmp &= 0xFF00;
			tid.hashCode |= tmp;
			tmp = tid.transactionID[0];
			tmp &= 0xFF;
			tid.hashCode |= tmp;


			return tid;
		}


		/**
		 * Returns the transaction id byte array (length 16).
		 * @return the transaction ID byte array.
		 */
		public byte[] GetTransactionID()
		{
			return transactionID;
		}

		/**
		 * Compares two TransactionID objects.
		 * @param obj the object to compare with.
		 * @return true if the objects are equal and false otherwise.
		 */
		public override bool Equals(Object obj)
		{

			if(   obj == null
				|| !(obj is TransactionID))
				return false;

			if(this == obj)
				return true;

			byte[] targetBytes = ((TransactionID)obj).transactionID;

			if (targetBytes == transactionID) return true;
			if (targetBytes == null || transactionID == null) return false;
			if (targetBytes.Length != transactionID.Length) return false;
			int len = targetBytes.Length;
			for (int x = 0; x < len; x++) 
			{
				if (targetBytes[x] != transactionID[x]) return false;
			}

			return true;
		}

		/**
		 * Compares the specified byte array with this transaction id.
		 * @param targetID the id to compare with ours.
		 * @return true if targetID matches this transaction id.
		 */
		public virtual bool Equals(byte[] targetID)
		{
			if (targetID == transactionID) return true;
			if (targetID == null || transactionID == null) return false;
			if (targetID.Length != transactionID.Length) return false;
			int len = targetID.Length;
			for (int x = 0; x < len; x++) 
			{
				if (targetID[x] != transactionID[x]) return false;
			}

			return true;
		}

		/**
		 * Returns the first four bytes of the transactionID to ensure proper
		 * retrieval from hashtables;
		 * @return the hashcode of this object - as advised by the Java Platform
		 * Specification
		 */
		public override int GetHashCode()
		{
			return hashCode;
		}

		static char[] hexDigits = {
									  '0', '1', '2', '3', '4', '5', '6', '7',
									  '8', '9', 'A', 'B', 'C', 'D', 'E', 'F'};
 
		public static string ToHexString(byte[] bytes) 
		{
			char[] chars = new char[bytes.Length * 2];
			for (int i = 0; i < bytes.Length; i++) 
			{
				int b = bytes[i];
				chars[i * 2] = hexDigits[b >> 4];
				chars[i * 2 + 1] = hexDigits[b & 0xF];
			}
			return new string(chars);
		}

		/**
		 * Returns a string representation of the ID
		 * @return a hex string representing the id
		 */
		public override String ToString()
		{
			System.Text.StringBuilder idStr = new System.Text.StringBuilder();

			idStr.Append("0x");
			idStr.Append(ToHexString(transactionID));
				
			idStr.Append(" ");

			return idStr.ToString();
		}
	}
}