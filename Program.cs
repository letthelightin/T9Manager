// https://convertio.co/png-ihttps://convertio.co/png-ico/co/

namespace Franklin_T9_Manager
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new MyCustomApplicationContext());
        }
    }
}