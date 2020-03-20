using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.Ports;

namespace set4tm_gui
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
                    Console.WriteLine($"Порт {port.PortName} открыт!");
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
            byte[] crc = CRC16Calc(req);            //получаем контрольную суммму для запроса 
            byte[] send = new byte[req.Length + 2]; //cоздаём массив для запроса + CRC 
            Array.Copy(req, 0, send, 0, req.Length);//копируем из req с 0-ого индекса в send начиная с 0-ого индекса, 8 элементов.
            Array.Copy(crc, 0, send, req.Length, 2);//копируем из crc с 0-ого индекса в send начиная с 8-ого индекса, 2 элемента.
            byte[] forcrc = new byte[] { id, 0x00 };  //тело ответа для вычисления корректного CRC
            crc = CRC16Calc(forcrc);                //считаем корректный CRC ответа  
            try
            {
                Write(send);                        //отправляем запрос на открытие канала + CRC в порт.
            }
            catch (System.Exception e)
            {
                Console.WriteLine($"ERROR: невозможно произвести запись в порт: {e}");
            }
            System.Threading.Thread.Sleep(50);      //делаем паузу в 50 мс
            byte[] answ = Read(id);                   //читаем порт
            //Проверяем корректность ответа счётчика
            if (answ[0] == id && answ[1] == 0x00 && answ[2] == crc[0] && answ[3] == crc[1])
            {
                return true;
            }
            else return false;
        }

        public byte[] GetReq(byte id)  //метод формирования запроса
        {
            byte req = 0x08;        // код запроса 08 - чтение праметров и данных 
            byte param = 0x1B;      // код параметра 1В - чтение данных в формате float
            byte dataArray = 0x00;  // код массива данных 00 - данные вспомогательных режимов измерения (RWRI) (по таблице 2-45)
            byte rwri = 0x11;       // код вспомогательного режима измерения (RWRI) 11 - напряжение фазное по фазе 1 (по таблице 2-38)

            byte[] body = new byte[5] { id, req, param, dataArray, rwri };    //запрос на чтение данных
            byte[] crc = CRC16Calc(body);                                   //получаем контрольную суммму для запроса 
            byte[] request = new byte[body.Length + crc.Length];            //cоздаём массив для запроса + CRC 
            Array.Copy(body, 0, request, 0, body.Length);                   //копируем из req в send
            Array.Copy(crc, 0, request, body.Length, 2);                    //копируем из crc в send
            return request;
        }

        public void Write(byte[] data)  //метод записи в порт
        {
            try
            {
                port.Write(data, 0, data.Length);
                //Console.WriteLine("Запрос записан в порт");
            }
            catch (System.Exception e)
            {
                Console.WriteLine($"ERROR: невозможно произвести запись в порт: {e}");
                return;
            }
        }

        public byte[] Read(byte id)  //метод чтения из порта
        {
            System.Threading.Thread.Sleep(50);
            byte[] fail = new byte[3] { 0, 0, 0 };
            if ((int)port.BytesToRead > 0)
            {
                byte[] answer = new byte[(int)port.BytesToRead];
                port.Read(answer, 0, port.BytesToRead);
                //Вычисляем CRC, для проверки корректности ответа счётчика
                byte[] forcrc = new byte[answer.Length - 2];                // массив для расчёта CRC длиной равной длине тела ответа
                Array.Copy(answer, 0, forcrc, 0, forcrc.Length);            // копируем только тело ответа в forcrcData без CRC
                byte[] crc = CRC16Calc(forcrc);                             // считаем CRC ответа          
                // Проверяем корректность ответа счётчика
                if (answer[0] == id && answer[(answer.Length) - 2] == crc[0] && answer[(answer.Length) - 1] == crc[1])
                {
                    return answer;
                }
                return fail;
            }
            else
            {
                return fail;
            }

        }

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

