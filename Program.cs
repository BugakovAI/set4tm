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
            // 0. Данные соединения
            string comNum = "COM8"; //Номер COM порта, к которму подключен счётчик
            byte id = 0x13;         //сетевой адрес счётчика          
            
            // 1. Открываем последовательный порт 
            ComControl COM8 = new ComControl(comNum);
            COM8.Open();
            
            // 2. Открываем канал связи счётчика
            bool isOpen = COM8.OpenChannel(id);
            if (isOpen==true) 
            {
                Console.WriteLine("Канал открыт!");
            
                // 3. Формируем запрос на чтение данных и отправляем его в порт
                byte[] request = COM8.GetReq(id);
                COM8.Write(request);

                // 4. Теперь читаем ответ из порта
                byte[] response = COM8.Read(id);

                // 5. Выводим значение в консоль
                byte[] byteValue = new byte[4]; // массив для значения параметра
                Array.Copy(response, 1, byteValue, 0, 4);
                float value = BitConverter.ToSingle(byteValue,0);
                Console.WriteLine($"Текущее значение напряжения на фазе А = {value}");

                Console.WriteLine("Для выхода нажмите любую клавишу ...");
                Console.ReadKey();
            }
            else {Console.WriteLine("Канал не открыт...");}

            COM8.Close();
        }
    }
}
