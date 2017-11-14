namespace DCCNodeLib
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;

    public class TestMulticastOption
    {
        private static IPAddress mcastAddress;
        private static int mcastPort;
        private static Socket mcastSocket;
        private static MulticastOption mcastOption;

        public static void Main(string[] args)
        {
            // Initialize the multicast address group and multicast port.
            // Both address and port are selected from the allowed sets as
            // defined in the related RFC documents. These are the same 
            // as the values used by the sender.
            mcastAddress = IPAddress.Parse("224.168.100.2");
            mcastPort = 11000;

            // Start a multicast group.
            StartMulticast();

            // Display MulticastOption properties.
            MulticastOptionProperties();

            // Receive broadcast messages.
            ReceiveBroadcastMessages();
        }


        private static void MulticastOptionProperties()
        {
            Console.WriteLine("Current multicast group is: " + mcastOption.Group);
            Console.WriteLine("Current multicast local address is: " + mcastOption.LocalAddress);
        }


        private static void StartMulticast()
        {
            try
            {
                mcastSocket = new Socket(AddressFamily.InterNetwork,
                    SocketType.Dgram,
                    ProtocolType.Udp);

                Console.Write("Enter the local IP address: ");

                IPAddress localIPAddr = IPAddress.Parse(Console.ReadLine());

                //IPAddress localIP = IPAddress.Any;
                EndPoint localEP = (EndPoint) new IPEndPoint(localIPAddr, mcastPort);

                mcastSocket.Bind(localEP);


                // Define a MulticastOption object specifying the multicast group 
                // address and the local IPAddress.
                // The multicast group address is the same as the address used by the server.
                mcastOption = new MulticastOption(mcastAddress, localIPAddr);

                mcastSocket.SetSocketOption(SocketOptionLevel.IP,
                    SocketOptionName.AddMembership,
                    mcastOption);
            }

            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void ReceiveBroadcastMessages()
        {
            bool done = false;
            byte[] bytes = new byte[100];
            IPEndPoint groupEP = new IPEndPoint(mcastAddress, mcastPort);
            EndPoint remoteEP = (EndPoint) new IPEndPoint(IPAddress.Any, 0);


            try
            {
                while (!done)
                {
                    Console.WriteLine("Waiting for multicast packets.......");
                    Console.WriteLine("Enter ^C to terminate.");

                    mcastSocket.ReceiveFrom(bytes, ref remoteEP);

                    Console.WriteLine("Received broadcast from {0} :\n {1}\n",
                        groupEP.ToString(),
                        Encoding.ASCII.GetString(bytes, 0, bytes.Length));
                }

                mcastSocket.Close();
            }

            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}