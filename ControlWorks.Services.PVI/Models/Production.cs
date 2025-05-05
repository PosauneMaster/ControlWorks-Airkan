using System;

namespace ControlWorks.Services.PVI.Models
{

    public class Production
    {
        private string _dateTime;
        private string _customerOrder;
        private string _shiptoName;
        private string _billtoName;
        private string _yourReference;
        private string _entryNo;
        private string _jobName;
        private string _qty;
        private string _sizeA;
        private string _sizeB;
        private string _coilNumber;
        private string _coilGauge;
        private string _coilWidth;
        private string _misc1;
        private string _misc2;
        private string _misc3;

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

        public string DateTime
        {
            get => _dateTime;
            set =>
                _dateTime = CheckLength(value);
        }

        public string CustomerOrder
        {
            get => _customerOrder;
            set =>
                _customerOrder = CheckLength(value);
        }

        public string ShiptoName
        {
            get => _shiptoName;
            set =>
                _shiptoName = CheckLength(value);
        }

        public string BilltoName
        {
            get => _billtoName;
            set =>
                _billtoName = CheckLength(value);
        }

        public string YourReference
        {
            get => _yourReference;
            set =>
                _yourReference = CheckLength(value);
        }

        public string EntryNo
        {
            get => _entryNo;
            set =>
                _entryNo = CheckLength(value);
        }

        public string JobName
        {
            get => _jobName;
            set =>
                _jobName = CheckLength(value);
        }


        public string Qty
        {
            get => _qty;
            set =>
                _qty = CheckLength(value);
        }

        public string SizeA
        {
            get => _sizeA;
            set =>
                _sizeA = CheckLength(value);
        }

        public string SizeB
        {
            get => _sizeB;
            set =>
                _sizeB = CheckLength(value);
        }

        public string CoilNumber
        {
            get => _coilNumber;
            set =>
                _coilNumber = CheckLength(value);
        }

        public string CoilGauge
        {
            get => _coilGauge;
            set =>
                _coilGauge = CheckLength(value);
        }

        public string CoilWidth
        {
            get => _coilWidth;
            set =>
                _coilWidth = CheckLength(value);
        }

        public string Misc1
        {
            get => _misc1;
            set =>
                _misc1 = CheckLength(value);
        }

        public string Misc2
        {
            get => _misc2;
            set =>
                _misc2 = CheckLength(value);
        }

        public string Misc3
        {
            get => _misc3;
            set =>
                _misc3 = CheckLength(value);
        }
    }
}
