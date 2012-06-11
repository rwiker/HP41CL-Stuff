using System;
using System.IO;
using System.IO.Ports;
using System.Threading;

public class CLReader
{
    public static void Main(string [] args)
    {
    	int baudrate = 1200;
	if (args.Length < 2) {
	   Console.Error.WriteLine("Usage:");
	   Console.Error.WriteLine("    {0} file port [baudrate]", "CLReader");
	   Console.Error.WriteLine();
	   Console.Error.WriteLine("Where baud defaults to {0}", baudrate);
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

	if (File.Exists(filename)) {
	   Console.Error.WriteLine("File {0} already exists, will not overwrite.", 
				   filename);
	   return;
	}

	SerialPort serialport = new SerialPort();
	serialport.PortName = portname;
	serialport.BaudRate = baudrate;
	serialport.Parity = Parity.None;
	serialport.DataBits = 8;
	serialport.StopBits = StopBits.One;
	serialport.Handshake = Handshake.None;
	serialport.ReadTimeout = 30000; // milliseconds

	serialport.Open();

	byte[] buffer = new byte[8192];
	int pos = 0;

	try {
	  int count;
	  
	  do {
	    count = serialport.Read(buffer, pos, 8192 - pos);
	    
	    pos += count;
	    
	  } while (pos < 8192);
	  
        }
	catch (TimeoutException) {
	    // nada
	}

        serialport.Close();

	if (pos % 2 != 0) {
	  Console.Error.WriteLine("Odd number of bytes read.");
	   return;
	}

	// swap high & low bytes:
	for (int i = 0; i < pos; i+= 2) {
	  byte tmp = buffer[i];
	  buffer[i] = buffer[i+1];
	  buffer[i+1] = tmp;
	}	
	
	FileStream fstream = File.Open(filename, FileMode.CreateNew, 
				       FileAccess.Write);
	
	BinaryWriter binWriter = new BinaryWriter(fstream);
	
	binWriter.Write(buffer, 0, pos);

	binWriter.Close();
	fstream.Close();

    }
}
