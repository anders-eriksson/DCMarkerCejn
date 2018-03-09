using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Contracts;

namespace CommunicationService
{
    public class ArtNoCommand : Command
    {
        public string Parameter { get; set; }

        public override void Run()
        {
            throw new NotImplementedException();
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