using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToRefactor
{
    public class SocketModel
    {
        private DateTime _created;
        private string _createdDisplay;
        public string Id { get; set; }
        public DateTime Created
        {
            get { return _created; }
            set { _created = value; }
        }
        public string CreatedDisplay
        {
            get
            {
                return _createdDisplay;

            }
            set
            {
                _createdDisplay = value;
            }
        }
        public string State { get; set; }
        public int RemotePort { get; set; }
    }
}
