//using Bundle.Abstractions;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Bundle.Client.Handlers
//{
//    public class DefualtPermanentActionHandler : IPermanentActionHandler
//    {
//        public event IPermanentActionHandler.UpdateHandler OnUpdate;

//        private readonly Thread _thread;

//        public DefualtPermanentActionHandler()
//        {
//            _thread = new Thread(new ThreadStart(Update));
//            _thread.Name = "PermanentActionHandler";
//        }

//        public void Start()
//        {
//            _thread.Start();
//        }

//        private void Update()
//        {
//            OnUpdate();
//        }
//    }
//}
