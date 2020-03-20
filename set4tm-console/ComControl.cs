using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.Ports;
using System.Linq;


namespace set4tm_console
{
    public class ComControl
    {
        SerialPort port = new SerialPort(); //объявили порт
        string portName;
        public ComControl(string portName) { this.portName = portName; }

        public void Open() //метод открытия порта
        {
            try
            {
                port.PortName = portName;
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
                    Log($"Порт {port.PortName} открыт!");
            }
            catch (System.Exception e)
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

        public bool OpenChannel(byte id)  //метод записи в порт
        {
            byte[] req = new byte[8] { id, 0x01, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30 };//запрос на открытие канала связи, где 1ый байт - код функции, 2-7 байты - пароль "000000" в ASCII, 8,9 - CRC.
            try
            {
                port.Write(AddCRC16(req), 0, req.Length+2);
                Log("Запрос записан в порт");
            }
            catch (System.Exception e)
            {
                Console.WriteLine($"ERROR: невозможно произвести запись в порт: {e}");
                return false;
            }
            System.Threading.Thread.Sleep(50);

            byte[] answ = new byte[] { id, 0x00 };       //тело ответа для вычисления корректного CRC
            byte[] answcrc = AddCRC16(answ);            //получаем ответ вместе с корректным CRC

            if ((int)port.BytesToRead > 0)
            {
                byte[] answer = new byte[(int)port.BytesToRead];
                port.Read(answer, 0, port.BytesToRead);
                if (answer.SequenceEqual(answcrc))
                {
                    Log($"Канал связи счёчтчика {id} открыт!");
                    return true;
                }
                else return false;
            }
            else return false;
        }
        public DateTime ReadTime(byte id)  //метод чтения ЭНЕРГИИ
        {
            byte[] request = { id, 0x04, 0x00 };
            byte[] response = GetData(request);
            int[] intResponse = new int[response.Length];
            int j = 0;
            foreach (byte i in response)
            {
                intResponse[Array.IndexOf(response, i)] = Convert.ToInt32(i.ToString("X")); //переписываем байтовый массив в интовый через строку
                j++;
            }
            DateTime date = new DateTime(2000 + intResponse[6], intResponse[5], intResponse[4], intResponse[2], intResponse[1], intResponse[0]);
            return date;
        }
        public (DateTime, DateTime) ReadOnOff(byte id)  //метод чтения мгновенных значений доп ПАРАМЕТРОВ (U,I,P и т.д.)
        {
            byte[] request = { id, 0x04, 0x11}; // 19 это два полубайта 1 и 9, 1 - журнал вкл//выкл, 9 - номер записи.
            byte[] response = GetData(request);
            int[] intResponse = new int[response.Length];
            int j = 0;
            foreach (byte i in response)
            {
                intResponse[j] = Convert.ToInt32(i.ToString("X")); //переписываем байтовый массив в интовый через строку
                j++;
            }
            DateTime dateOn = new DateTime(2000 + intResponse[6], intResponse[5], intResponse[4], intResponse[2], intResponse[1], intResponse[0]);
            DateTime dateOff = new DateTime(2000 + intResponse[13], intResponse[12], intResponse[11], intResponse[9], intResponse[8], intResponse[7]);
            return (dateOn, dateOff);
        }
        public (DateTime, DateTime) ReadCap(byte id)  //метод чтения времени открытия крышки
        {
            byte[] request = { id, 0x04, 0xA2 }; // 19 это два полубайта 1 и 9, 1 - журнал вкл//выкл, 9 - номер записи.
            byte[] response = GetData(request);
            int[] intResponse = new int[response.Length];
            int j = 0;
            foreach (byte i in response)
            {
                intResponse[j] = Convert.ToInt32(i.ToString("X")); //переписываем байтовый массив в интовый через строку
                j++;
            }
            DateTime dateOn = new DateTime(2000 + intResponse[6], intResponse[5], intResponse[4], intResponse[2], intResponse[1], intResponse[0]);
            DateTime dateOff = new DateTime(2000 + intResponse[13], intResponse[12], intResponse[11], intResponse[9], intResponse[8], intResponse[7]);
            return (dateOn, dateOff);
        }

        //byte req = 0x08;        // код запроса 08 - чтение праметров и данных 
        //byte param = 0x1B;      // код параметра 1В - чтение данных в формате float
        //byte dataArray = 0x00;  // код массива данных 00 - данные вспомогательных режимов измерения (RWRI) (по таблице 2-45)
        //byte rwri = 0x11;       // код вспомогательного режима измерения (RWRI) 11 - напряжение фазное по фазе 1 (по таблице 2-38)

        public float ReadU(byte id, int phase)  //метод чтения напряжения
        {
            byte ph = 0x11;
            switch(phase)
            {
                case 1:
                    ph = 0x11;
                    break;
                case 2:
                    ph = 0x12;
                    break;
                case 3:
                    ph = 0x13;
                    break;
            }
            byte[] request = { id, 0x08, 0x1B, 0x00, ph};
            byte[] response = GetData(request);
            return BitConverter.ToSingle(response, 0);
        }


        public byte[] GetData(byte[] request)  //метод записи в порт и чтения ответа. В качестве ответа возрващает массив байт без айди и црц
        {
            //System.Threading.Thread.Sleep(20);
            try
            {
                port.Write(AddCRC16(request), 0, request.Length + 2);
                Log("Запрос записан в порт");
            }
            catch (System.Exception e)
            {
                Console.WriteLine($"ERROR: невозможно произвести запись в порт: {e}");
                Log("ОШИБКА! Запрос не записан в порт... ");
                return null;
            }

            System.Threading.Thread.Sleep(150);

            if ((int)port.BytesToRead > 0)
            {
                byte[] answer = new byte[(int)port.BytesToRead];
                port.Read(answer, 0, port.BytesToRead);
                byte[] body = new byte[answer.Length - 2];                  // массив для расчёта CRC длиной равной длине тела ответа
                Array.Copy(answer, 0, body, 0, body.Length);                // копируем только тело ответа в forcrcData без CRC
                byte[] AnswWithCalcCRC = AddCRC16(body);                    // получаем тело ответа с рассчитанным CRC          
                if (answer[0] == request[0] && answer.SequenceEqual(AnswWithCalcCRC))
                {
                    byte[] data = new byte[body.Length - 1];
                    Array.Copy(answer, 1, data, 0, data.Length);
                    return data;
                }
                return null;
            }
            else
            {
                return null;
            }

        }
        public byte[] AddCRC16(byte[] Message)
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

            byte[] request = new byte[Message.Length + 2];            //cоздаём массив для запроса + CRC 
            Array.Copy(Message, 0, request, 0, Message.Length);       //копируем из req в send
            Array.Copy(CRC, 0, request, Message.Length, 2);           //копируем из crc в send
            
            return request;
        }
        public static void Log(string message)
        {
            string writePath = @"D:\Dev\set4tm\set4tm-console\log.txt";
            try
            {
                using (StreamWriter sw = new StreamWriter(writePath, true, System.Text.Encoding.Default))
                {
                    sw.WriteLine(DateTime.Now + " " + message);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
