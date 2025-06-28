using System;

namespace ControlWorks.Services.PVI.Models
{
    public class Orders
    {
        private string _customerOrder;
        private string _statusOrder;
        private string _dateTime;
        private string _pieceErp;
        private string _pieceErpNumber;
        private string _started;
        private string _printed;
        private string _completed;
        private string _misc1;
        private string _side;

        private string CheckLength(string value)
        {
            if (String.IsNullOrWhiteSpace(value))
            {
                return String.Empty;
            }
            if (value.Length > 50)
            {
                return value.Substring(0, 49);
            }
            return value;
        }

        public string CustomerOrder
        {
            get => _customerOrder;
            set => _customerOrder = CheckLength(value);
        }
        public string StatusOrder
        {
            get => _statusOrder;
            set => _statusOrder = CheckLength(value);
        }

        public string DateTime
        {
            get => _dateTime;
            set => _dateTime = CheckLength(value);
        }

        public string PieceErp
        {
            get => _pieceErp;
            set => _pieceErp = CheckLength(value);
        }


        public string PieceErpNumber
        {
            get => _pieceErpNumber;
            set => _pieceErpNumber = CheckLength(value);
        }

        public string Started
        {
            get => _started;
            set => _started = CheckLength(value);
        }

        public string Printed
        {
            get => _printed;
            set => _printed = CheckLength(value);
        }

        public string Completed
        {
            get => _completed;
            set => _completed = CheckLength(value);
        }

        public string Misc1
        {
            get => _misc1;
            set => _misc1 = CheckLength(value);
        }

        public string Side
        {
            get => _side;
            set => _side = CheckLength(value);
        }
    }
}

