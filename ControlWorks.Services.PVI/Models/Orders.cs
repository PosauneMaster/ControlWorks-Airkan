using System;

namespace ControlWorks.Services.PVI.Models
{
    public class Orders
    {
        private string _customerOrder;
        private string _status;
        private string _dateTime;
        private string _misc1;
        private string _misc2;
        private string _misc3;
        private string _misc4;
        private string _misc5;

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
        public string Status
        {
            get => _status;
            set => _status = CheckLength(value);
        }

        public string DateTime
        {
            get => _dateTime;
            set => _dateTime = CheckLength(value);
        }

        public string Misc1
        {
            get => _misc1;
            set => _misc1 = CheckLength(value);
        }


        public string Misc2
        {
            get => _misc2;
            set => _misc2 = CheckLength(value);
        }

        public string Misc3
        {
            get => _misc3;
            set => _misc3 = CheckLength(value);
        }

        public string Misc4
        {
            get => _misc4;
            set => _misc4 = CheckLength(value);
        }

        public string Misc5
        {
            get => _misc5;
            set => _misc5 = CheckLength(value);
        }
    }
}

