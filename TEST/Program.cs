namespace TEST
{
    internal class Program
    {
        static void Main(string[] args)
        {
            DateTime a = DateTime.Now;
            DateTime b = DateTime.Parse("2024.05.08 7:00:00");

            Console.WriteLine(b.CompareTo(a));
            Console.WriteLine();
        }
    }
}
