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

        public bool OpenChannel(byte id)  //метод открытия канала связи со счётчиком
        {
            byte[] req = new byte[8] { id, 0x01, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30 };//запрос на открытие канала связи, где 1ый байт - код функции, 2-7 байты - пароль "000000" в ASCII, 8,9 - CRC.
            byte[] res = GetData(req);
            if (res[0] == 0x00)
                return true;
            else return false;
        }

        public (DateTime, DateTime) ReadJ(byte id, int param, byte row = 0) // метод чтения журналов. param = 0 - Time, 1 - OnOff, 2 - Cap; row = 1-9 от первой к последней записи в журнале
        {
            DateTime dateOff = new DateTime();
            DateTime dateOn = new DateTime();
            byte rwri = 0x00;
            switch (param)
            {
                case 0:
                    rwri = 0x00; break;
                case 1:
                    rwri = (byte)(0x10 + row); break;
                case 2:
                    rwri = (byte)(0xA0 + row); break;
            }
            byte[] request = { id, 0x04, rwri }; // 19 это два полубайта 1 и 9, 1 - журнал вкл//выкл, 9 - номер записи от 0(сатарая) до 9(новая)
            byte[] response = GetData(request);
            int[] intResponse = new int[response.Length];
            int j = 0;
            foreach (byte i in response)
            {
                intResponse[j] = Convert.ToInt32(i.ToString("X")); //переписываем байтовый массив в интовый через строку
                j++;
            }
            dateOn = new DateTime(2000 + intResponse[6], intResponse[5], intResponse[4], intResponse[2], intResponse[1], intResponse[0]);
            if (intResponse.Length>8) // если в ответе 8 байт, то это ответ на запрос текущего времени, в остальных случаях читатется журнал, а значит возвращается две даты.
            {
                dateOff = new DateTime(2000 + intResponse[13], intResponse[12], intResponse[11], intResponse[9], intResponse[8], intResponse[7]);
            }

            return (dateOn, dateOff);
        }

        public float ReadP(byte id, int param, int type = 0x00, int phase = 0x00) // метод чтения параметров/
        {
            byte rwri = 0x00;
            switch (param)  // 1 - PQS; 2 - Uф, Uл, U1; 3 - I; 4 - cos; 5 - f; 5 6 - t.
            {
                case 1:
                    switch (type) 
                    {
                        case 1:
                            switch (phase)
                            {
                                case 0: rwri = 0x00; break;
                                case 1: rwri = 0x01; break;
                                case 2: rwri = 0x02; break;
                                case 3: rwri = 0x03; break;
                            }
                            break;
                        case 2:
                            switch (phase)
                            {
                                case 0: rwri = 0x04; break;
                                case 1: rwri = 0x05; break;
                                case 2: rwri = 0x06; break;
                                case 3: rwri = 0x07; break;
                            }
                            break;
                        case 3:
                            switch (phase)
                            {
                                case 0: rwri = 0x08; break;
                                case 1: rwri = 0x09; break;
                                case 2: rwri = 0xA; break;
                                case 3: rwri = 0xB; break;
                            }
                            break;
                    } break; // 1 - активная, 2 - реактивная, 3 - полная
                case 2:
                    switch (type) 
                    {
                        case 1:
                            switch (phase)  // 1, 2, 3 - фазы A, B, C
                            {
                                case 1: rwri = 0x11; break;
                                case 2: rwri = 0x12; break;
                                case 3: rwri = 0x13; break;
                            }
                            break;
                        case 2:
                            switch (phase)  // 1 - AB, 2 - BC, 3 - CA 
                            {
                                case 1: rwri = 0x15; break;
                                case 2: rwri = 0x16; break;
                                case 3: rwri = 0x17; break;
                            }
                            break;
                        case 3: rwri = 0x18; break;
                    } break; // 1 - фазное, 2 - межфазное, 3 - нулевой последовательности
                case 3:
                    switch (phase)
                    {
                        case 1: rwri = 0x21; break;
                        case 2: rwri = 0x22; break;
                        case 3: rwri = 0x23; break;
                    } break;
                case 4:
                    switch (phase)
                    {
                        case 0: rwri = 0x30; break;
                        case 1: rwri = 0x31; break;
                        case 2: rwri = 0x32; break;
                        case 3: rwri = 0x33; break;
                    }
                    break;
                case 5:
                    rwri = 0x40; break;
                case 6:
                    rwri = 0x70; break;
            }
            byte[] request = { id, 0x08, 0x1B, 0x00, rwri };
            return BitConverter.ToSingle(GetData(request), 0);
        }

        public (float, float, float, float) ReadE(byte id, int type, byte num = 0x00, byte tar = 0x00) // метод чтения массивов энергии type (1-от сброса,2-месячный,3-суточный)
        {
            byte typeNum = 0x00;
            byte tarif = 0x00;
            byte[] response = new byte[16];

            if (type == 1 || type == 2)
            {
                switch (type)
                {
                    case 1: break;  //читаем энергию от сброса
                    case 2:         //читаем энергию месячную
                        typeNum = (byte)(0x30 + num);       //3 - массив энергии за предыдущие сутки
                        tarif = tar;                        //0 - по сумме тарифов
                        break;
                }
                byte[] request = { id, 0x05, typeNum, tarif };
                response = GetData(request);
            }
            else if (type == 3)
            {
                byte mask = 0x0F;   //это значит что в ответ будут включены 4 типа энергии
                byte format = 0x00; //
                byte[] request = { id, 0x0A, 0x06, num, tar, mask, format }; //06 это массив энергии за прошлые сутки
                response = GetData(request); 
            }
            
            byte[] reverse = new byte[16];
            for (int i = 0; i < response.Length; i++)
            {
                reverse[i] = response[(15-i)];
            }
            
            float Ad = (BitConverter.ToInt32(reverse, 12))/ 10000f;     // Active direct
            float Ar = (BitConverter.ToInt32(reverse, 8)) / 10000f;     // Active reverse
            float Rd = (BitConverter.ToInt32(reverse, 4)) / 10000f;     // Reactive direct
            float Rr = (BitConverter.ToInt32(reverse, 0)) / 10000f;     // Reactive reverse

            return (Ad, Ar, Rd, Rr);
        }
        
        public byte[] GetData(byte[] request)  // метод записи в порт и чтения ответа. В качестве ответа возрващает массив байт без айди и црц
        {
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

            System.Threading.Thread.Sleep(100);

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
        
        public byte[] AddCRC16(byte[] Message) // метод добавляет к запросу два байта CRC16
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
                        Register = (ushort)(Register >> 1); // сдвигаем регистр вправо
                    }
                }
            }

            CRC[1] = (byte)(Register >> 8);         // присваеваем старший байт полученного регистра младшему байту результата CRC (CRClow)
            CRC[0] = (byte)(Register & 0x00FF);     // присваеваем младший байт полученного регистра старшему байту результата CRC (CRCHi) это условность Modbus — обмен байтов местами.

            byte[] request = new byte[Message.Length + 2];      //cоздаём массив для запроса + CRC 
            Array.Copy(Message, 0, request, 0, Message.Length); //копируем из req в send
            Array.Copy(CRC, 0, request, Message.Length, 2);     //копируем из crc в send
            
            return request;
        }
        
        public static void Log(string message) // метод для записи логов в текстовый файл в корне проекта
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
