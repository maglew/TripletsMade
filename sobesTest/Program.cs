/*
Тестовое задание:
Необходимо написать консольное приложение на C#, выполняющее частотный анализ текста.

Входные параметры: путь к текстовому файлу.

Выходные результаты: вывести на экран через запятую 10 самых часто встречающихся в тексте триплетов (3 идущих подряд буквы слова), 
и на следующей строке время работы программы в миллисекундах.

Требования: программа должна обрабатывать текст в многопоточном режиме.
 */

using System.Diagnostics;

Stopwatch st = new Stopwatch();


Console.Write("Введите путь к файлу: ");
string? path = Console.ReadLine();

//string path = @"C:\Users\vladislav\source\repos\sobesTest\sobesTest\text.txt";//для более быстрого задания пути

st.Start();
List<string> words = new List<string>();//список всех слов из текста
List<string> allTriplets = new List<string>();//список всех триплетов
List<string> topTriplets = new List<string>();//список 10 самых частых триплетов

string fileText = await File.ReadAllTextAsync(path);//переменная для хранения текста


char[] symbols = new char[12]{ ' ', '.', ',', '!', '(', ')', ':','?', ';','\n', '\r', '-' }; //символы разделителя

int processorsCount = Environment.ProcessorCount;//количество ядер процессора
//int processorsCount = 1; //для проверки однопоточности

wordsclean();
TripletsMade();
topTripletsMade();
printRezult();

void wordsclean()//разбиение текста на слова и добавление в лист если длина слова >= 3
{
    words.AddRange((fileText.Split(symbols)).Where(x => x.Length >= 3).ToList());
}


void TripletsMade()//создание триплетов из слов(с выбором однопоточного и многопоточного режима в зависимости от кол-ва процессоров)
{
    if (processorsCount > 1)
    {
        multiThreadTripletsMade();
    }
    else 
    {
        OneThreadTripletsMade();
    }
}

void multiThreadTripletsMade()//создание триплетов из слов (многопоточно)
{
    Task[] tasks = new Task[processorsCount];
    int pocketlen = words.Count / processorsCount;//размер пакета элементов которые будут обрабатывать в каждом пакете(деление нацело)
    int remain = words.Count - (pocketlen * processorsCount);//кол-во не вошедших элементов в размер пакета(остаток от деления нацело)
    for (int i = 0; i < tasks.Length; i++)
    {
        int indexI = i; // т.к. значение i может поменяться при EndInvoke!!!!!!!!!!!!

        if (indexI != (tasks.Length - 1))
        { tasks[indexI] = new Task(() => pocketTripletsMade(indexI * pocketlen, pocketlen)); }
        else
        { tasks[indexI] = new Task(() => pocketTripletsMade(indexI * pocketlen, pocketlen + remain)); }//последний поток обрабатывает и невошедшие значения в пакет
        tasks[indexI].Start();
    }
    Task.WaitAll(tasks);
}

void OneThreadTripletsMade()//создание триплетов из слов (однопоточно)
{

    foreach (string str in words)
    {
        int numTrip = str.Length - 2;//определиние кол-ва триплетов в слове
        for (int i = 0; i < numTrip; i++)
        {
            allTriplets.Add(str.Substring(i,3));//добавление каждого триплета в слове в список всех триплетов
        }
    }
}

void pocketTripletsMade(int i, int num)//создание триплетов из слов(для многопоточного режима)
{
    
    for (int k = i; i < k+num; i++)
    {
        int numTrip = words[i].Length - 2;//определиние кол-ва триплетов в слове
        for (int j = 0; j < numTrip; j++)
        {
            lock (allTriplets)//локирование листа для доступа при нескольких потоках
            allTriplets.Add(words[i].Substring(j, 3));//добавление каждого триплета в слове в список всех триплетов
        }
    }
}


void topTripletsMade()//создание 10 саых частых триплетов(запись в список первых десяти отсортированных триплетов на убывание по частоте)
{
    topTriplets.AddRange(allTriplets.GroupBy(x => x).OrderByDescending(x => x.Count()).Select(x => x.Key).Take(10).ToList()); 
}


void printRezult()// вывод результатов(топ 10 триплетов и время работы программы)
{ 
    Console.Write("топ10 триплетов: ");

    topTriplets.ForEach(s => Console.Write(s + ","));

    st.Stop();

    Console.WriteLine("\nВремя выполнения программы: {0} ms", st.Elapsed.TotalSeconds/10);
}


