SilkroadSecurityApi 1.4
pushedx

This is my latest Silkroad Security API ported to C#. This version is a greatly updated version of the last SilkroadSecurity for C# released. The project was compiled using Visual Studio 2010 and targets the .Net 4.0 runtime. If you need earlier versions, you will have to recompile and fix any code that might not be compatible with earlier versions of the .Net library.

Here is a quick overview of the classes provided.

Blowfish.cs - Contains a little-endian blowfish implementation that Silkroad uses.
Packet.cs - The main packet object used for building and parsing data.
PacketReader.cs - An internal class used for reading.
PacketWriter.cs - An internal class used for writing.
Security.cs - The Silkroad security logic. This file contains everything related to the security protocol.
TransferBuffer.cs - A helper class for working with buffers and accompanying state data.
Utility.cs - A helper class that contains a HexDump function useful for debugging.

The Security class supports both Client and Server side processing. This means you can use the class for everything related to Silkroad. Likewise, the Blowfish class is reusable for other applications as well. The Packet class contains Silkroad specific methods for Strings, but with a little modification it can be reused for other applications.

[Quick Start Guide]

1a. Allocate a new Security object for your connection.

	Security security = new Security();

1b. If this object is for a server connection (i.e. emu), call the GenerateSecurity function to setup the security mode that you wish to use. In this example, handshake + blowfish + security bytes are being used. Do not call this function if you are connecting to a server.

	security.GenerateSecurity(true, true, true);

2. Whenever you receive network data from the server, pass it through the Recv function of the Security object. You may use a TransferBuffer for convenience or pass the raw buffer itself.

	TransferBuffer recv_buffer = new TransferBuffer(4096, 0, 0);
	recv_buffer.Size = s.Receive(recv_buffer.Buffer, 0, recv_buffer.Buffer.Length, SocketFlags.None, out err);
	security.Recv(recv_buffer);

3. Obtain all queued incoming packets and store them for processing. If there are none, a null object will be returned.

	List<Packet> packets = security.TransferIncoming();
	if(packets != null)
        {
        	// TODO: Process each Packet or store them for later use.
	}

4. To send packets, pass a Packet object via the Send function (ne sure to build the Packet object properly). NOTE: This function does not actually "send" data across the network; it just stores the data internally for the next step.

	Packet response = new Packet(0x6100, true, false);
	response.WriteUInt8(locale);
	response.WriteAscii("SR_Client");
	response.WriteUInt32(version);
	security.Send(response);

5. Finally, dispatch all pending buffers to be sent by calling the TransferOutgoing function. If no buffers are available for sending, a null object will be returned.

	// Check to see if we have any packets to send
	List<KeyValuePair<TransferBuffer, Packet>> buffers = security.TransferOutgoing();
	if (buffers != null)
	{
		// TODO: Process each buffer and Send the data. The Packet object in the Value 
		// field of the pair is the Packet object that was used to build the buffer.
	}

[Packet Quick Reference Guide]

To create packets:

1. Allocate a new Packet object with the opcode of the packet at minimal as well as any other flags. These flags cannot be changed.

	Packet packet = new Packet(0x0000);

2. Call the "Write" family style of functions to write data.

	packet.WriteInt8(-2);
	packet.WriteSingle(3.14);

	// This will write a byte array as an upcasted array of UInt16s. This syntax is required to write between types.
	packet.WriteUInt16Array(new byte[] { 0xFF, 0xFF, 0xFF }.Cast<object>().ToArray());

	packet.WriteAscii("Zeus");

	// This writes Unicode text as the proper encoded ANSI required. You must specify the correct codepage!
	packet.WriteAscii("Зевс", 1251); // russian
	packet.WriteAscii("어울림", 949); // korean

	// This writes Unicode text as 2 byte UTF-16 Unicode text.
	packet.WriteUnicode("Зевс");

3. To overwrite data, use the SeekWrite function.

	// First store the current location (if required)
	long index = packet.SeekWrite(0, SeekOrigin.Current);

	// Seek to the new location
	packet.SeekWrite(0, SeekOrigin.Begin);

	// TODO: Call a Write function to overwrite data. NOTE: Overwrite allows adding new data, so if you
	// Write additional data, when you seek back to the original position you are now in the middle of
	// the data you just wrote. Keep that in mind. For example, let's say you want to overwrite a Byte but
	// write an ushort. When you seek back, you are now in the middle of the ushort, not after it. Simply do not
	// seek back to the original position in this case.

	// Seek back to the original posiiton (if required)
	packet.SeekWrite(index, SeekOrigin.Begin);

4. If you are not passing the Packet through the Security object, you can call Lock to prevent any further writing and to allow Packet reading.

	packet.Lock();

To parse packets:

1. The packet must be in a Locked state to read.

	Packet packet = new Packet(0x0000);
	packet.WriteAscii("Зевс", 1251);
	packet.Lock();

2. Call a "Read" family style of functions to read data.

	// In this case, we want to read ASCII bytes and convert it using the 
	// specified code page. As a result, we get "Зевс". If we were to use
	// the default codepage, 1252, we would get "Çåâñ".
	String str = packet.ReadAscii(1251);

To debug packets:

1. Use the Utility.HexDump function.

	byte[] packet_bytes = packet.GetBytes();
	Console.WriteLine("[S->C][{0:X4}][{1} bytes]{2}{3}{4}{5}{6}", packet.Opcode, packet_bytes.Length, packet.Encrypted ? "[Encrypted]" : "", packet.Massive ? "[Massive]" : "", Environment.NewLine, Utility.HexDump(packet_bytes), Environment.NewLine);

[License]

This work is being released to the Public Domain.

[End]
