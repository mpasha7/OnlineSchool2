namespace TEST
{
    internal class Program
    {
        static void Main(string[] args)
        {
            DateTime d = DateTime.Now;
            Console.WriteLine(d.ToString("yy_MM_dd"));
            Console.WriteLine(d.ToString("hh_mm_ss"));
        }
    }
}
