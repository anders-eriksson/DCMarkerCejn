using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Contracts;
using Configuration;

namespace CommunicationService
{
    public class ArtNoCommand : Command
    {
        public string Parameter { get; set; }

        public ArtNoCommand(ICommunicationModule comm)
        {
            _comm = comm;
        }

        public override void Run()
        {
            DateTime startTime = DateTime.Now;
            byte data = Constants.ArtNrCode;
            bool timeout = false;
            _comm.Write(Constants.DOstartAddress, data);
            // Wait for ACK
            do
            {
                data = _comm.Read(Constants.DOstartAddress, Constants.DOtotalPoints);
                timeout = IsTimeout(startTime);
            } while (!timeout && data != Constants.ACK);
            _comm.Write(Constants.DOstartAddress, data);
        }

        private static bool IsTimeout(DateTime startTime)
        {
            TimeSpan ts = DateTime.Now - startTime;
            bool result = ts.Seconds > DCConfig.Instance.AdamErrorTimeout;
            return result;
        }
    }

    public class ProvbitCommand : Command
    {
        public bool Parameter { get; set; }

        public override void Run()
        {
            throw new NotImplementedException();
        }
    }

    public class OkCommand : Command
    {
        public bool Parameter { get; set; }

        public override void Run()
        {
            throw new NotImplementedException();
        }
    }

    public class ItemInPlaceCommand : Command
    {
        public bool Parameter { get; set; }

        public override void Run()
        {
            throw new NotImplementedException();
        }
    }

    public class StartMarkingCommand : Command
    {
        public bool Parameter { get; set; }

        public override void Run()
        {
            throw new NotImplementedException();
        }
    }

    public class EndMarkingCommand : Command
    {
        public bool Parameter { get; set; }

        public override void Run()
        {
            throw new NotImplementedException();
        }
    }

    public class RestartApplicationCommand : Command
    {
        public bool Parameter { get; set; }

        public override void Run()
        {
            throw new NotImplementedException();
        }
    }

    public class SetKantCommand : Command
    {
        public byte Parameter { get; set; }

        public override void Run()
        {
            throw new NotImplementedException();
        }
    }

    public class BatchNotReadyCommand : Command
    {
        public bool Parameter { get; set; }

        public override void Run()
        {
            throw new NotImplementedException();
        }
    }

    public class ReadyToMarkCommand : Command
    {
        public bool Parameter { get; set; }

        public override void Run()
        {
            throw new NotImplementedException();
        }
    }

    public class ErrorCommand : Command
    {
        public byte Parameter { get; set; }

        public override void Run()
        {
            throw new NotImplementedException();
        }
    }
}