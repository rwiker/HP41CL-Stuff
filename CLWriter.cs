using System;
using System.IO;
using System.IO.Ports;
using System.Threading;

public class CLWriter
{
    public static void Main(string [] args)
    {
    	int baudrate = 1200;
        int delay = 0;
	if (args.Length < 2) {
	   Console.Error.WriteLine("Usage:");
	   Console.Error.WriteLine("    {0} file port [baudrate [delay]]", "CLWriter");
	   Console.Error.WriteLine();
	   Console.Error.WriteLine("Where baud defaults to {0}", baudrate);
	   Console.Error.WriteLine("and delay defaults to {0}", delay);
	   Console.Error.WriteLine("Available Ports:");
	   Console.Error.WriteLine();
	   foreach (string s in SerialPort.GetPortNames())
	   {
		Console.Error.WriteLine("   {0}", s);
	   }
	   return;
	}
	string filename = args[0];
	string portname = args[1];
	if (args.Length > 2) {
	   baudrate = int.Parse(args[2]);
	   if (baudrate != 1200 && baudrate != 2400 && 
	       baudrate != 4800 && baudrate != 9600) {
	       Console.Error.WriteLine("Invalid baudrate {0}; should be one of", baudrate);
	       Console.Error.WriteLine("1200, 2400, 4800, 9600");
	       return; 
	   }
	}
        if (args.Length > 3) {
	   delay = int.Parse(args[3]);
	   if (delay > 10) {
	      Console.Error.WriteLine("delay {0} probably too large.", delay);
	      return;
	   }
	}	

	if (!File.Exists(filename)) {
	   Console.Error.WriteLine("File {0} does not exist.", filename);
	   return;
	}

	FileStream fstream = File.Open(filename, FileMode.Open);

	if (fstream.Length > 8192) {
	  Console.Error.WriteLine("WARNING: {0} is over 8192 bytes long ({1});", filename, fstream.Length);
	  Console.Error.WriteLine("Will only transfer the first 8192 bytes.");
	}

	BinaryReader binReader = new BinaryReader(fstream);

	SerialPort serialport = new SerialPort();
	serialport.PortName = portname;
	serialport.BaudRate = baudrate;
	serialport.Parity = Parity.None;
	serialport.DataBits = 8;
	serialport.StopBits = StopBits.One;
	serialport.Handshake = Handshake.None;

	serialport.Open();

	try {
	    byte[] buffer = new byte[8192];
	    int count = binReader.Read(buffer, 0, 8192);

	    // swap high & low bytes:
	    for (int i = 0; i < count; i+= 2) {
	        byte tmp = buffer[i];
		buffer[i] = buffer[i+1];
		buffer[i+1] = tmp;
	    }	    

	    for (int i = 0; i < count; i++) {
	    	Console.Write("{0:x2} ", buffer[i]);
		if (i % 16 == 15) {
		   Console.WriteLine();
		}
		serialport.Write(buffer, i, 1);
		if (delay > 0) {
		    Thread.Sleep(delay);
		}
	    }
	    Console.WriteLine();
        }
	catch (EndOfStreamException) {
	    // nada
	}
        serialport.Close();
    }
}
