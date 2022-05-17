using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace FileManager
{
    class Program
    {
        // Дополнительный функционал:
        // Целых 4 (ЧЕТЫРЕ) возможные кодировки для чтения.
        // Копирование файла с указаннием названии копии.
        // Цветное, красивое, прелесное, шикарное, бесподобное оформление консоли (Одобрено Лебедевым).
        // Точки в конце каждого комментария.
        // Конкатенирование файлов в итоговый файл с указанной кодировкой.

        // Понимаю, что методы длиной не в 40 строк, но такое ограничение это лишь рекомендация, а не обязательство,
        // да и разбить методы на более маленькие почти невозможно.
        static void Help()
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"{Environment.NewLine}Главное меню{Environment.NewLine}Достпуные команды:");
            Console.WriteLine("\"-drivers\" - получение информации о дисках компьютера. Возможность просмотра содержимого директорий диска при его выборе");
            Console.WriteLine("\"-directory <путь_к_директории>\" - просмотр содержимого конкретной директории, расположенной по введенному пути");
            Console.WriteLine("\"-menu\" - переход к меню со списком всех доступных команд и их описанием.");
            Console.WriteLine("\"-exit\" - завершение работы программы.");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Команда: ");
        }

        static void Main(string[] args)
        {
            string command;
            do
            {
                Console.WriteLine("Приветствую в файловом менеджере. Для просмотра дисков введите команду \"-drivers\". Если вы хотите работать с файлами, сначала нужно указать директорию с помощью команды \"-directory\". Затем либо нужно ввести номер следующей директории в предложенном списке, либо номер файла. Функции создания и конкатенации файлов можно использовать при выборе какой-либо директории. Функции работы с файлами (вывод содержимого, копирование, перемещение, удаление) можно использовать при выборе какого-либо файла из директории");
                Help();
            ReadCommand: command = Console.ReadLine().Trim();
                string[] parts = command.Split(new char[] { ' ' });
                if (parts.Length == 0 || parts[0] == null)
                {
                    Console.Write("Неизвестная команда. Пожалуйста, повторите ввод: ");
                    goto ReadCommand;
                }
                switch (parts[0])
                {
                    // Информация о дисках
                    case "-drivers":
                        GetInfoAboutDrivers();
                        break;
                    // Попадание в введенную в формате d<№ папки> директорию
                    case "-directory":
                        if (parts.Length < 2)
                        {
                            Console.Write("Необходимо ввести путь директории. Пожалуйста, повторите ввод: ");
                            goto ReadCommand;
                        }
                        if (!GetInfoAboutDirectory(parts[1]))
                        {
                            goto ReadCommand;
                        }
                        break;
                    // Возврат в главное меню
                    case "-menu":
                        Console.Write("Вы итак находитесь в меню. Введите команду: ");
                        goto ReadCommand;
                    // Завершение программы
                    case "-exit":
                        Console.Write("Программа завершена. Удачного дня.");
                        Environment.Exit(0);
                        break;

                    default:
                        Console.Write("Неизвестная команда. Пожалуйста, повторите ввод: ");
                        goto ReadCommand;
                }
            }
            while (true);
        }

        /// <summary>
        /// Отображение инфорации о дисках с последующей возможностью его выбора.
        /// </summary>
        static void GetInfoAboutDrivers()
        {
            try
            {
                int ind = 0; // индекс для выбора диска
                DriveInfo[] drivers = DriveInfo.GetDrives();
                foreach (DriveInfo driver in drivers)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    ind++;
                    string strIsReady = driver.IsReady == true ? "Готов к работе" : "Не готов";
                    Console.WriteLine($"{ind}.  Диск {driver.Name}");
                    Console.WriteLine($"Тип диска: {driver.DriveType}");
                    Console.WriteLine($"Состояние: {strIsReady}");

                    if (driver.IsReady == true)
                    {
                        Console.WriteLine($"Метка тома: {driver.VolumeLabel}");
                        Console.WriteLine($"Формат диска: {driver.DriveFormat}");
                        Console.WriteLine($"Размер памяти на диске:  {driver.TotalSize} байт ");
                        Console.WriteLine($"Доступная для пользователя память: {driver.AvailableFreeSpace} байт");
                        Console.WriteLine($"Доступно свободной памяти: {driver.TotalFreeSpace} байт{Environment.NewLine}");
                    }
                    Console.ResetColor();
                }
                string insert = "";
                int readIndex = 1;
                do
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("Введите номер диска для просмотра его содержимого или \"-menu\" для перехода к списку команд: ");
                    insert = Console.ReadLine().Trim();
                    if (insert == "-menu")
                        return;
                    if (int.TryParse(insert, out readIndex) && readIndex > 0 && readIndex <= ind)
                        break;
                    Console.WriteLine("Неизвестная команда!");
                } while (true);
                Console.ResetColor();
                // Если пользователь ввел правильный индекс, осуществляется переход к работе с директориями
                GetInfoAboutDirectory(drivers[readIndex - 1].Name);
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Ошибка при работе с IO! {ex.Message}. Программа завершена с кодом 0");
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в процессе работы! {ex.Message}. Программа завершена с кодом 0");
                Environment.Exit(0);
            }

        }

        /// <summary>
        /// Метод для получения информации о введенной директории и последующей работой с ее содержимым.
        /// </summary>
        /// <param name="dirName">Путь к директории.</param>
        /// <returns></returns>
        static bool GetInfoAboutDirectory(string dirName)
        {
            try
            {
                if (Directory.Exists(dirName))
                {
                    // Вывод информации о директории и возможных командах
                CurrentCategory: Console.ForegroundColor = ConsoleColor.Yellow;
                    int dirInd = 0;
                    Console.WriteLine("Папки:");
                    string[] directories = Directory.GetDirectories(dirName);
                    foreach (string s in directories)
                        Console.WriteLine($"d{++dirInd}.  {s}");

                    int fileInd = 0;
                    Console.WriteLine($"{Environment.NewLine}Файлы:");
                    string[] files = Directory.GetFiles(dirName);
                    foreach (string s in files)
                        Console.WriteLine($"f{++fileInd}.  {s}");

                    Console.ResetColor();
                    string insert = "";
                    int readIndex = 0;
Help:                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("Доступные команды для работы с данной директорией: ");
                    Console.WriteLine("\"-close\" - возврат в директорию выше");
                    Console.WriteLine("\"d<номер_папки>\" - просмотр содержимого в папке с соответствующим номером");
                    Console.WriteLine("\"f<номер_файла>\" - работа с файлом с соответствующим номером");
                    Console.WriteLine("\"-create <имя_файла>\" - создание файла в этой директории в кодировке UTF-8. В названии файла не могут использоваться запрещенные символы, а также пробелы. Замените их подчеркиваниями. В названии также должно быть расширение файла. Пример: -create D://C#/my_file.txt");
                    Console.WriteLine("\"-create <имя_файла> <название_кодировки>\" - создание файла в этой директории в указанной кодировке. В названии файла не могут использоваться запрещенные символы, а также пробелы. Замените их подчеркиваниями. В названии также должно быть расширение файла. Пример: -create D://C#/my_file.txt");
                    Console.WriteLine("\"-concatenate <путь_к_первому_файлу> <путь_ко_второму_файлу> ... <путь_к_N-ому_файлу> <название_файла> <название_кодировки>\" - конкатенация N файлов, расположенных по указанным путям в один общий файл с введенным названием и кодирокой. В названии файла не могут использоваться запрещенные символы, а также пробелы. Замените их подчеркиваниями. В названии также должно быть расширение файла. Пример названия файла: D://C#/my_file_concatenated.txt");
                    Console.WriteLine("\"-help\" - отображение всех доступных команд для работы с данной директорией");
                    Console.WriteLine("\"-menu\" - выход в главное меню");


                    // Переменная для определения последующей команды (если она будет равна 0, значит введена строка "-menu",
                    // если 1, то осуществится работа со следующей директорией, если 2, то с файлом, если 3, то с директорией выше).
                    int isInsertCorrect = 0;

                    do
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write("Введите команду: ");
                        insert = Console.ReadLine().Trim();
                        if (insert == "-menu")
                            break;
                        if (insert == "-close")
                        {
                            isInsertCorrect = 3;
                            break;
                        }
                        if (insert == "-help")
                        {
                            goto Help;
                        }
                        if (insert.Length < 2)
                        {
                            Console.Write("Неизвестная команда. Пожалуйста, повторите ввод: ");
                            continue;
                        }
                        // если ввели команду для работу с файлом или папкой, отделим ее индекс от литерала
                        if (insert[0] == 'd')
                        {
                            // Отделяем букву от номера для получения номера папки.
                            insert = insert.Substring(1, insert.Length - 1);
                            if (int.TryParse(insert, out readIndex) && readIndex > 0 && readIndex <= dirInd)
                            {
                                isInsertCorrect = 1;
                                break;
                            }
                        }
                        else if (insert[0] == 'f')
                        {
                            // Отделяем букву от номера для получения номера файла.
                            insert = insert.Substring(1, insert.Length - 1);
                            if (int.TryParse(insert, out readIndex) && readIndex > 0 && readIndex <= fileInd)
                            {
                                isInsertCorrect = 2;
                                break;
                            }
                        }

                        string[] parts = insert.Split(new char[] { ' ' });
                        // Метод создания файла с кодировкой UTF-8
                        if (parts.Length == 2 && parts[0] == "-create" && parts[1] != "" && parts[1] != null)
                        {
                            if (TryCreateFile(dirName, parts[1], "UTF-8"))
                            {
                                Console.WriteLine("Файл успешно создан.");
                                goto CurrentCategory;
                            }
                            continue;

                        }
                        // Метод создания файла с указанной кодировкой
                        if (parts.Length == 3 && parts[0] == "-create" && parts[2] != "" && parts[2] != null)
                        {
                            if (TryCreateFile(dirName, parts[1], parts[2]))
                            {
                                Console.WriteLine("Файл успешно создан.");
                                goto CurrentCategory;
                            }
                            continue;
                        }
                        if (parts.Length > 4 && parts[0] == "-concatenate")
                        {
                            List<string> listOfFiles = new List<string>();
                            for (int i = 1; i < parts.Length - 2; i++)
                                listOfFiles.Add(parts[i]);

                            if (TryConcatenateFiles(dirName, listOfFiles, parts[parts.Length - 1], parts[parts.Length - 2]))
                            {
                                Console.WriteLine("Файлы успешно сконкатенированы.");
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.WriteLine("Содержимое итогового файла в кодировке UTF-8:");
                                Console.ForegroundColor = ConsoleColor.Gray;
                                TryReadFile(parts[parts.Length - 2], parts[parts.Length - 1]);
                            }
                            continue;
                        }

                        Console.Write("Неизвестная команда. Пожалуйста, повторите ввод: ");
                    } while (true);
                    Console.ResetColor();
                    // Если выбрана папка, то индекс 1, если файл, то индекс 2, если выход в меню, то 0
                    if (isInsertCorrect == 1)
                        GetInfoAboutDirectory(directories[readIndex - 1]);
                    if (isInsertCorrect == 2)
                        WorkWithFile(files[readIndex - 1], dirName);
                    if (isInsertCorrect == 3)
                    {
                        string newDir = Path.GetDirectoryName(dirName);
                        if (newDir == "" || newDir == null || Path.GetFullPath(newDir) == Path.GetFullPath(dirName))
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Папка является корневой.");
                            Console.ResetColor();
                        }
                        else GetInfoAboutDirectory(newDir);
                    }

                    goto CurrentCategory;
                }
                Console.Write("Не удается найти указанную директорию. Проверьте ее путь и повторите еще раз: ");
                return false;
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Ошибка при работе с IO! {ex.Message}. Программа завершена с кодом 0");
                Environment.Exit(0);
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в процессе работы! {ex.Message}. Программа завершена с кодом 0");
                Environment.Exit(0);
                return false;
            }
        }

        /// <summary>
        /// Метод для конкатенации нескольких файлов в один общий.
        /// </summary>
        /// <param name="dirName">Директория для общего файла.</param>
        /// <param name="listOfFiles">Список файлов для конкатенации.</param>
        /// <param name="strEncoding">Кодировка.</param>
        /// <param name="nameOfFinalFile">Название общего файла.</param>
        /// <returns></returns>
        private static bool TryConcatenateFiles(string dirName, List<string> listOfFiles, string strEncoding, string nameOfFinalFile)
        {
            try
            {
                if (!Directory.Exists(dirName))
                {
                    Console.WriteLine("Данной директории не существует!");
                    return false;
                }

                foreach (string fileName in listOfFiles)
                    if (!File.Exists(fileName))
                    {
                        Console.WriteLine($"Файла, расположенного по пути {fileName} не существует!");
                        return false;
                    }


                Encoding encoding;
                switch (strEncoding)
                {
                    case "UTF-8":
                        encoding = Encoding.UTF8;
                        break;
                    case "UTF-7":
                        encoding = Encoding.UTF7;
                        break;
                    case "Unicode":
                        encoding = Encoding.Unicode;
                        break;
                    case "ASCII":
                        encoding = Encoding.ASCII;
                        break;

                    default:
                        Console.WriteLine("Указанной кодировки не найдено. Файл создатся в кодировке UTF-8");
                        encoding = Encoding.UTF8;
                        break;
                }


                try
                {
                    foreach (string fileName in listOfFiles)
                    {
                        File.AppendAllText(Path.Combine(dirName, nameOfFinalFile), Environment.NewLine + File.ReadAllText(fileName), encoding);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ошибка при создании файла! " + ex.Message);
                    return false;
                }

                return true;
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Ошибка при работе с IO! {ex.Message}. Программа завершена с кодом 0");
                Environment.Exit(0);
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в процессе работы! {ex.Message}. Программа завершена с кодом 0");
                Environment.Exit(0);
                return false;
            }
        }
        
        /// <summary>
        /// Метод создания нового файла
        /// </summary>
        /// <param name="dirName">Название директории.</param>
        /// <param name="fileName">Название файла.</param>
        /// <param name="strEncoding">Коидровка.</param>
        /// <returns></returns>
        private static bool TryCreateFile(string dirName, string fileName, string strEncoding)
        {
            try
            {
                if (!Directory.Exists(dirName))
                {
                    Console.WriteLine("Данной директории не существует!");
                    return false;
                }
                if (File.Exists(Path.Combine(dirName, fileName)))
                {
                    Console.WriteLine("Файл с таким названием уже существует!");
                    return false;
                }


                Encoding encoding;
                switch (strEncoding)
                {
                    case "UTF-8":
                        encoding = Encoding.UTF8;
                        break;
                    case "UTF-7":
                        encoding = Encoding.UTF7;
                        break;
                    case "Unicode":
                        encoding = Encoding.Unicode;
                        break;
                    case "ASCII":
                        encoding = Encoding.ASCII;
                        break;

                    default:
                        Console.WriteLine("Указанной кодировки не найдено. Файл создатся в кодировке UTF-8");
                        encoding = Encoding.UTF8;
                        break;
                }

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("Введите содержимое файла в консоль. Введите \"'End of file'\" с новой строки для обозначения конца файла");
                Console.ForegroundColor = ConsoleColor.Gray;

                string line;
                List<string> content = new List<string>();
                do
                {
                    line = Console.ReadLine();
                    if (line == "'End of file'" && content.Count == 0)
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine("Файл не должен быть пустым");
                        Console.ForegroundColor = ConsoleColor.Gray;
                        line = "";
                        continue;
                    }

                    if (line != "'End of file'")
                        content.Add(line);
                } while (line != "'End of file'");
                Console.ForegroundColor = ConsoleColor.Green;

                try
                {
                    File.WriteAllLines(Path.Combine(dirName, fileName), content.ToArray(), encoding);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ошибка при создании файла! " + ex.Message);
                    return false;
                }

                return true;
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Ошибка при работе с IO! {ex.Message}. Программа завершена с кодом 0");
                Environment.Exit(0);
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в процессе работы! {ex.Message}. Программа завершена с кодом 0");
                Environment.Exit(0);
                return false;
            }
        }




        #region Files
        /// <summary>
        /// Меню работы с файлом.
        /// </summary>
        /// <param name="path">Путь.</param>
        /// <param name="directory">Директория.</param>
        static void WorkWithFile(string path, string directory)
        {
            try
            {

                string fileName = Path.GetFileName(path);
 Help:               Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("Доступные команды для работы с данным файлом: ");
                Console.WriteLine("\"-read\" - вывод содержимого файла в консоль в кодировке UTF-8");
                Console.WriteLine("\"-read <название_кодировки>\" - вывод содержимого файла в консоль в выбранной кодировке (UTF-8, UTF-7, Unicode, ASCII)");
                Console.WriteLine("\"-move <путь_к_директории>\" - Перемещение данного файла в указанную директорию. Если такой файл уже существует, он перезапишется");
                Console.WriteLine("\"-copy\" - создание копии файла в его директории. Если такая копия уже существует, она перезапишется");
                Console.WriteLine("\"-copy <путь_к_директории>\" - создание копии файла в выбранной директории. Если такая копия уже существует, она перезапишется");
                Console.WriteLine("\"-copy <путь_к_директории> <название_файла>\" - создание копии файла в выбранной директории с указанным названием. Если такая копия уже существует, она перезапишется");
                Console.WriteLine("\"-remove\" - удаление файла");
                Console.WriteLine("\"-close\" - возврат в директорию, из которой производились операции с файлом");
                Console.WriteLine("\"-help\" - отображение всех доступных команд для работы с данным файлом");
                
                string command;
                bool isInCycle = true;
                do
                {
                ReadCommand: Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("Введите команду: ");
                    command = Console.ReadLine().Trim();
                    if (command == "-help")
                        goto Help;
                    
                    string[] parts = command.Split(new char[] { ' ' });
                    if (parts.Length == 0 || parts[0] == null)
                    {
                        Console.Write("Неизвестная команда. Пожалуйста, повторите ввод: ");
                        goto ReadCommand;
                    }
                    switch (parts[0])
                    {
                        case "-read":
                            if (parts.Length < 2 || parts[1] == "" || parts[1] == null)
                            {
                                if (!TryReadFile(path, "UTF-8"))
                                    goto ReadCommand;
                            }
                            else
                            {
                                if (!TryReadFile(path, parts[1]))
                                    goto ReadCommand;
                            }
                            break;

                        case "-copy":
                            if (parts.Length < 2 || parts[1] == "" || parts[1] == null)
                            {
                                if (!TryCopyFile(path, directory, fileName))
                                    goto ReadCommand;
                                else Console.WriteLine("Файл успешно скопирован.");
                            }
                            else if (parts.Length < 3 || parts[2] == "" || parts[2] == null)
                            {
                                if (!TryCopyFile(path, parts[1], fileName))
                                    goto ReadCommand;
                                else Console.WriteLine("Файл успешно скопирован.");
                            }
                            else
                            {
                                if (!TryCopyFile(path, parts[2], fileName))
                                    goto ReadCommand;
                                else Console.WriteLine("Файл успешно скопирован.");
                            }
                            break;

                        case "-remove":
                            if (!TryRemoveFile(path))
                                goto ReadCommand;
                            else Console.WriteLine("Файл успешно удален.");
                            break;
                        case "-move":
                            if (parts.Length > 1 && parts[1] != "" && parts[1] != null)
                                if (path == Path.Combine(parts[1], fileName))
                                    Console.WriteLine("Указана та же директория, что и у выбранного файла.");
                                else if (!TryMoveFile(path, parts[1], fileName))
                                    goto ReadCommand;
                                else Console.WriteLine("Файл успешно перемещен.");


                            break;

                        case "-close":
                            return;
                        default:
                            Console.Write("Неизвестная команда. ");
                            goto ReadCommand;
                    }

                } while (isInCycle);
                Console.ResetColor();
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Ошибка при работе с IO! {ex.Message}. Программа завершена с кодом 0");
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в процессе работы! {ex.Message}. Программа завершена с кодом 0");
                Environment.Exit(0);
            }
        }
        static bool TryReadFile(string path, string strEncoding)
        {
            try
            {
                if (!File.Exists(path))
                {
                    Console.WriteLine("Данного файла не существует!");
                    return false;
                }

                Encoding encoding;
                switch (strEncoding)
                {
                    case "UTF-8":
                        encoding = Encoding.UTF8;
                        break;
                    case "UTF-7":
                        encoding = Encoding.UTF7;
                        break;
                    case "Unicode":
                        encoding = Encoding.Unicode;
                        break;
                    case "ASCII":
                        encoding = Encoding.ASCII;
                        break;

                    default:
                        Console.WriteLine("Указанной кодировки не найдено. Файл выведен в кодировке UTF-8");
                        encoding = Encoding.UTF8;
                        break;
                }

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("Содержимое файла:");

                Console.OutputEncoding = encoding;
                Console.ForegroundColor = ConsoleColor.Gray;
                string[] lines = File.ReadAllLines(path, encoding);
                if (lines.Length == 0)
                    Console.WriteLine("Файл пуст!");
                foreach (string line in lines)
                    Console.WriteLine(line);
                Console.ResetColor();
                Console.OutputEncoding = Encoding.Default;
                return true;
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Ошибка при работе с IO! {ex.Message}. Программа завершена с кодом 0");
                Environment.Exit(0);
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в процессе работы! {ex.Message}. Программа завершена с кодом 0");
                Environment.Exit(0);
                return false;
            }
        }

        static bool TryCopyFile(string path, string newPath, string fileName)
        {
            try
            {
                if (!File.Exists(path))
                {
                    Console.WriteLine("Данного файла не существует!");
                    return false;
                }
                FileInfo fileInfo = new FileInfo(path);

                // Отделение имени файла от его формата.
                string[] strNewFile = fileName.Split(new char[] { '.' });

                if (!Directory.Exists(newPath))
                {
                    Console.WriteLine("Данного пути не существует!");
                    return false;
                }
                string copiedFile = newPath + strNewFile[0] + "_copy." + strNewFile[1];
                if (File.Exists(copiedFile))
                    Console.WriteLine("Копия данного файла уже существует. Она перезаписана");

                File.Copy(path, copiedFile, true);
                return true;
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Ошибка при работе с IO! {ex.Message}. Программа завершена с кодом 0");
                Environment.Exit(0);
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в процессе работы! {ex.Message}. Программа завершена с кодом 0");
                Environment.Exit(0);
                return false;
            }
        }

        static bool TryRemoveFile(string path)
        {
            try
            {
                if (!File.Exists(path))
                {
                    Console.WriteLine("Данного файла не существует!");
                    return false;
                }
                File.Delete(path);
                return true;
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Ошибка при работе с IO! {ex.Message}. Программа завершена с кодом 0");
                Environment.Exit(0);
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в процессе работы! {ex.Message}. Программа завершена с кодом 0");
                Environment.Exit(0);
                return false;
            }

        }
        static bool TryMoveFile(string path, string newDir, string fileName)
        {
            try
            {
                if (!File.Exists(path))
                {
                    Console.WriteLine("Данного файла не существует!");
                    return false;
                }
                if (!Directory.Exists(newDir))
                {
                    Console.WriteLine("Данной директории не существует!");
                    return false;
                }
                File.Move(path, Path.Combine(newDir, fileName));
                return true;
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Ошибка при работе с IO! {ex.Message}. Программа завершена с кодом 0");
                Environment.Exit(0);
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в процессе работы! {ex.Message}. Программа завершена с кодом 0");
                Environment.Exit(0);
                return false;
            }

        }

        #endregion
    }
}
