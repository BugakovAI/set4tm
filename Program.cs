using System;
using System.IO;
using System.IO.Ports;
using System.Text;

namespace ComPort
{
    class Program
    {
        static void Main(string[] args)
        {
            /*
            ComControl COM = new ComControl();
            ComControl COM2 = new ComControl();
            
            COM.Open("COM10");
            COM2.Open("COM11");
            
            COM.Write("порт COM10 передаёт привет порту COM11");
            string answ = COM2.Read();

            Console.WriteLine(answ);

            COM.Close();
            COM2.Close();
            */

            // 1. Открываем последовательный порт 

            ComControl COM = new ComControl();
            COM.Open("COM8");

            // 2. Открываем канал связи счётчика

            byte id = 0x13;                                                         //адрес счётчика
            byte[] req = new byte[8] {id, 0x01, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30};//запрос на открытие канала связи, где 1ый байт - код функции, 2-7 байты - пароль "000000" в ASCII, 8,9 - CRC.
            byte[] crc = COM.CRC16Calc(req);                                        //получаем контрольную суммму для запроса 
            byte[] send = new byte[req.Length + crc.Length];                        //cоздаём массив для запроса+CRC 
            
            Array.Copy(req, 0, send, 0, req.Length);    //копируем из req с 0-ого индекса в send начиная с 0-ого индекса, 8 элементов.
            Array.Copy(crc, 0, send, req.Length, 2);    //копируем из crc с 0-ого индекса в send начиная с 8-ого индекса, 2 элемента.
            
            COM.Write(send);                            //отправляем запрос на открытие канала + CRC в порт.
            
            //выводим в консоль запрос который отправили в порт
            Console.WriteLine("Запрос на открытие канала (hex): ");
            foreach (byte i in send)
            {
                Console.Write(i.ToString("X")); 
            }
            System.Threading.Thread.Sleep(1000);        //делаем паузу в 1 сек

            
            // 3. Вычисляем CRC, для проверки корректности ответа счётчика

            byte[] forcrc = new byte[] {id, 0x00};      //тело ответа для вычисления CRC
            crc = COM.CRC16Calc(forcrc);                //считаем CRC ответа          
            Console.WriteLine("\nРасчётная контрольная сумма ответа счётчика: ");
            foreach (byte i in crc)
            {
                Console.Write(i.ToString("X"));
            }
            
            // 4. Читаем ответ счётчика с порта

            byte[] answ = COM.Read();                   //читаем порт
            Console.WriteLine("\nОтвет счётчика на запрос: ");
            foreach (byte i in answ)
            {
                Console.Write(i.ToString("X"));
            }
            
            // 5. Проверяем корректность ответа счётчика

            if (answ[0] == id && answ[1] == 0x00 && answ[2] == crc[0] && answ[3] == crc[1])
            {
                Console.WriteLine("\nПоздравляю, канал связи счётчика открыт!");
            }
            else {Console.WriteLine("\nНе удалось открыть канал связи...");}


            // 6. Формируем запрос на чтение данных и отправляем его в порт
            byte reqCode = 0x08;        // код запроса 08 - чтение праметров и данных 
            byte paramCode = 0x1B;      // код параметра 1В - чтение данных в формате float
            byte dataArrayCode = 0x00;  // код массива данных 00 - данные вспомогательных режимов измерения (RWRI) (по таблице 2-45)
            byte rwri = 0x11;             // код вспомогательного режима измерения (RWRI) 11 - напряжение фазное по фазе 1 (по таблице 2-38)

            byte[] reqData = new byte[5] {id, reqCode, paramCode, dataArrayCode, rwri};     //запрос на чтение данных
            byte[] crcData = COM.CRC16Calc(reqData);                                        //получаем контрольную суммму для запроса 
            byte[] sendData = new byte[reqData.Length + crcData.Length];                    //cоздаём массив для запроса + CRC 
            
            Array.Copy(reqData, 0, sendData, 0, reqData.Length);                 //копируем из req в send
            Array.Copy(crcData, 0, sendData, reqData.Length, crcData.Length);    //копируем из crc в send
            System.Threading.Thread.Sleep(1000);        //делаем паузу в 1 сек
            COM.Write(sendData);        //отправляем запрос на открытие канала + CRC в порт.
            
            //выводим в консоль запрос который отправили в порт
            Console.WriteLine("Запрос на чтение U фазы 1 (hex): ");
            foreach (byte i in sendData)
            {
                Console.Write(i.ToString("X")); 
            }

            // 7. Читаем ответ счётчика с порта
            System.Threading.Thread.Sleep(1000);        //делаем паузу в 1 сек
            byte[] answData = COM.Read();   //читаем порт
            Console.WriteLine("\nОтвет счётчика на запрос: ");
            foreach (byte i in answData)
            {
                Console.Write(i.ToString("X"));
            }
            

            // 8. Вычисляем CRC, для проверки корректности ответа счётчика
            byte[] forcrcData = new byte[answData.Length-2];            // массив для расчёта CRC длиной равной длине тела ответа
            Array.Copy(answData, 0, forcrcData, 0, forcrcData.Length);  // копируем только тело ответа в forcrcData без CRC
            crcData = COM.CRC16Calc(forcrcData);                        //считаем CRC ответа          
            Console.WriteLine("\nРасчётная контрольная сумма: ");
            foreach (byte i in crcData)
            {
                Console.Write(i.ToString("X"));
            }
            
            // 9. Проверяем корректность ответа счётчика

            if (answData[0] == id && answData[5] == crcData[0] && answData[6] == crcData[1])
            {
                Console.WriteLine("\nПоздравляю, ответ корректен!");
            }
            else {Console.WriteLine("\nУвы, ответ не корректен!...");}

            // 10. Выводим значение в консоль

            byte[] byteValue = new byte[4];            // массив для значения парамтера
            Array.Copy(answData, 1, byteValue, 0, byteValue.Length);
            float value = BitConverter.ToSingle(byteValue,0);
            Console.WriteLine($"\nТекущее значение напряжения на фазе А = {value}");

            Console.WriteLine("Для выхода нажмите любую клавишу ...");
            Console.ReadKey();

            COM.Close();
            
        }
    }
}
