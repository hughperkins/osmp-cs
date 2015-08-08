using System;

/**
 * <p>Title: Stun4J</p>
 * <p>Description: Simple Traversal of UDP Through NAT</p>
 * <p>Copyright: Copyright (c) 2003</p>
 * <p>Organisation: <p> Louis Pasteur University, Strasbourg, France</p>
 * <p>Network Research Team (http://www-r2.u-strasbg.fr)</p></p>
 * @author Emil Ivov
 * @version 0.1
 */

namespace net.voxx.stun4cs 
{
	public interface RequestListener
	{
		void requestReceived(StunMessageEvent evt);
	}
}