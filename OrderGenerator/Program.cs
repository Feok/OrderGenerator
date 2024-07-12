using QuickFix;

namespace OrderGenerator
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                SessionSettings settings = new SessionSettings("initiator.cfg");
                Initiator application = new Initiator();
                IMessageStoreFactory storeFactory = new FileStoreFactory(settings);
                ILogFactory logFactory = new ScreenLogFactory(settings);
                QuickFix.Transport.SocketInitiator initiator = new QuickFix.Transport.SocketInitiator(application, storeFactory, settings, logFactory);

                application.MyInitiator = initiator;

                initiator.Start();
                application.Run();
                initiator.Stop();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
            Environment.Exit(1);
        }
    }
}
