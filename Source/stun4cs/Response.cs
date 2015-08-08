using System;

namespace net.voxx.stun4cs 
{
	/**
	 * A response descendant of the message class. The primary purpose of the
	 * Response class is to allow better functional definition of the classes in the
	 * stack package.
	 *
	 * <p>Organisation: <p> Louis Pasteur University, Strasbourg, France</p>
	 * <p>Network Research Team (http://www-r2.u-strasbg.fr)</p></p>
	 * @author Emil Ivov
	 * @version 0.1
	 */

	public class Response : Message
	{

		public Response()
		{

		}

		/**
		 * Checks whether responseType is a valid response type and if yes sets it
		 * as the type of the current instance.
		 * @param responseType the type to set
		 * @throws StunException ILLEGAL_ARGUMENT if responseType is not a valid
		 * response type
		 */
		public override void SetMessageType(int responseType)
		{
			if(!IsResponseType(responseType))
				throw new StunException(StunException.ILLEGAL_ARGUMENT,
					(int)(responseType)
					+ " - is not a valid response type.");


			base.SetMessageType(responseType);
		}

	}
}