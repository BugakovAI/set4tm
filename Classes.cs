using System;
using System.IO;
using System.IO.Ports;
using System.Text;


namespace ComPort
{
    public class ComControl
    {
        SerialPort port = new SerialPort(); //объявили порт

        public void Open(string name) //метод открытия порта
        {
            try
            {
                port.PortName = name;
                port.BaudRate = 9600;
                port.DataBits = 8;
                port.Parity = Parity.Odd;
                port.StopBits = StopBits.One;
                port.ReadTimeout = 1000;
                port.WriteTimeout = 1000;
                if (port.IsOpen == true) 
                    port.Close();
                port.Open();
                if (port.IsOpen == true) 
                    Console.WriteLine($"Порт {port.PortName} открылся!");
            }
            catch (System.Exception e )
            {
                Console.WriteLine($"ERROR: невозможно открыть порт: {e}");
                return;
            }
        }

        public void Close()  //метод закрытия порта
        {
            port.Close();
            if (port.IsOpen == false) 
                Console.WriteLine($"\nПорт {port.PortName} закрылся!");
        }
        public void Write(byte[] data)  //метод записи в порт
        {
            try
            {
                port.Write (data, 0, data.Length);
            }
            catch (System.Exception e)
            {
                Console.WriteLine($"ERROR: невозможно произвести запись в порт: {e}");
                return;
            }
        }
        
        public byte[] Read()  //метод чтения из порта
        {
            if ((int)port.BytesToRead > 0)
            {
                byte[] answer = new byte[(int)port.BytesToRead];
                port.Read(answer, 0, port.BytesToRead);
                return answer;
            }
            else
            {
                byte[] fail = new byte[] {0,0,0};
                return fail;
            }

        }

        /*
        public void Write(string data)  //метод записи в порт
        {
            byte[] message = new byte[data.Length];
            message = Encoding.Default.GetBytes(data);
            port.Write (message, 0, message.Length);
            //System.Threading.Thread.Sleep(2000);
        }
        
        public string Read()  //метод чтения из порта
        {
            if (port.BytesToRead > 0)
            {
                byte[] answer = new byte[(int)port.BytesToRead];
                port.Read(answer, 0, port.BytesToRead);
                return Encoding.Default.GetString(answer);
            }
            return "0";
        }
        */

        public byte[] CRC16Calc(byte[] Message)
        {
            byte[] CRC = new byte[2];  
            ushort Register = 0xFFFF;                       // создаем регистр, в котором будем сохранять высчитанный CRC
            ushort Polynom = 0xA001;                        //Указываем полином, он может быть как 0xA001(старший бит справа), так и его реверс 0x8005(старший бит слева, здесь не рассматривается), при сдвиге вправо используется 0xA001
            for (int i = 0; i < Message.Length; i++)        // для каждого байта в принятом\отправляемом сообщении проводим следующие операции(байты сообщения без принятого CRC)
            {
                Register = (ushort)(Register ^ Message[i]); // Делим через XOR регистр на выбранный байт сообщения(от младшего к старшему)
                for (int j = 0; j < 8; j++)                 // для каждого бита в выбранном байте делим полученный регистр на полином
                {
                    if ((ushort)(Register & 0x01) == 1)     //если старший бит равен 1 то
                    {
                        Register = (ushort)(Register >> 1); //сдвигаем на один бит вправо
                        Register = (ushort)(Register ^ Polynom); //делим регистр на полином по XOR
                    }
                    else                                    //если старший бит равен 0 то
                    {
                    Register = (ushort)(Register >> 1);     // сдвигаем регистр вправо
                    }
                }
            }

            CRC[1] = (byte)(Register >> 8);         // присваеваем старший байт полученного регистра младшему байту результата CRC (CRClow)
            CRC[0] = (byte)(Register & 0x00FF);     // присваеваем младший байт полученного регистра старшему байту результата CRC (CRCHi) это условность Modbus — обмен байтов местами.

            return CRC;
        }
    }
}