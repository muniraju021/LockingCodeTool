using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SerialPortComLib.Services
{
    public interface ISerialPortService
    {                        
        Action<string> CallBackOnDataReceived { get; set; }
        bool WriteToDevice<T>(T dataToWrite);

        void InitializeSerialPortComm(string portName, int baudrate = 9600,
           Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One,
           Action<string> callBackOnDataReceived = null);

        List<string> GetSerialComPorts();

        void CloseConnection();
    }
}
