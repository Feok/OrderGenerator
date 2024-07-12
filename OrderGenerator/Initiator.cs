using QuickFix;
using QuickFix.Fields;

namespace OrderGenerator
{
    public class Initiator : MessageCracker, IApplication
    {
        Session _session = null;

        public IInitiator MyInitiator = null;

        readonly List<string> symbols = ["PETR4", "VALE3", "VIIA4"];
        readonly List<char> sides = new() { Side.BUY, Side.SELL };
        Random random = new Random();

        #region IApplication interface overrides

        public void OnCreate(SessionID sessionID)
        {
            _session = Session.LookupSession(sessionID);
        }

        public void OnLogon(SessionID sessionID) { Console.WriteLine("Logon - " + sessionID.ToString()); }
        public void OnLogout(SessionID sessionID) { Console.WriteLine("Logout - " + sessionID.ToString()); }

        public void FromAdmin(Message message, SessionID sessionID) { }
        public void ToAdmin(Message message, SessionID sessionID) { }

        public void FromApp(Message message, SessionID sessionID)
        {
            try
            {
                Crack(message, sessionID);
            }
            catch (Exception ex)
            {
                Console.WriteLine("==Cracker exception==");
                Console.WriteLine(ex.ToString());
                Console.WriteLine(ex.StackTrace);
            }
        }

        public void ToApp(Message message, SessionID sessionID)
        {
            try
            {
                bool possDupFlag = false;
                if (message.Header.IsSetField(Tags.PossDupFlag))
                {
                    possDupFlag = QuickFix.Fields.Converters.BoolConverter.Convert(
                        message.Header.GetString(Tags.PossDupFlag));
                }
                if (possDupFlag)
                    throw new DoNotSend();
            }
            catch (FieldNotFoundException)
            { }
            Console.WriteLine();
        }
        #endregion

        public void OnMessage(QuickFix.FIX44.ExecutionReport m, SessionID s) { }

        public void Run()
        {
            while (true)
            {
                try
                {
                    QuickFix.FIX44.NewOrderSingle m = QueryNewOrderSingle44();
                    m.Header.GetString(Tags.BeginString);
                    SendMessage(m);
                    Thread.Sleep(1000);
                }
                catch (System.Exception e)
                {
                    Console.WriteLine("Message Not Sent: " + e.Message);
                    Console.WriteLine("StackTrace: " + e.StackTrace);
                }
            }
        }

        private void SendMessage(Message m)
        {
            if (_session != null)
                _session.Send(m);
            else
            {
                Console.WriteLine("Can't send message: session not created.");
            }
        }

        private QuickFix.FIX44.NewOrderSingle QueryNewOrderSingle44()
        {
            QuickFix.FIX44.NewOrderSingle newOrderSingle = new QuickFix.FIX44.NewOrderSingle(
                new ClOrdID(Guid.NewGuid().ToString()),
                new Symbol(symbols[random.Next(3)]),
                new Side(sides[random.Next(2)]),
                new TransactTime(DateTime.Now),
                new OrdType(OrdType.LIMIT));

            newOrderSingle.Set(new HandlInst('1'));
            newOrderSingle.Set(new OrderQty(Convert.ToDecimal(random.Next(1, 99999))));
            newOrderSingle.Set(new Price(Convert.ToDecimal(random.Next(1, 99999)/100)));

            return newOrderSingle;
        }
    }
}
