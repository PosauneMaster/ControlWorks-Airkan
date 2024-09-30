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
        private string _qty;
        private string _sizeA;
        private string _sizeB;
        private string _coilNumber;
        private string _coilGauge;
        private string _coilWidth;
        private string _misc1;
        private string _misc2;
        private string _misc3;

        public string DateTime
        {
            get => _dateTime;
            set =>
                _dateTime = String.IsNullOrWhiteSpace(value)
                    ? String.Empty
                    : value.Substring(0, 49);
        }

        public string CustomerOrder
        {
            get => _customerOrder;
            set =>
                _customerOrder = String.IsNullOrWhiteSpace(value)
                    ? String.Empty
                    : value.Substring(0, 49);
        }

        public string ShiptoName
        {
            get => _shiptoName;
            set =>
                _shiptoName = String.IsNullOrWhiteSpace(value)
                    ? String.Empty
                    : value.Substring(0, 49);
        }

        public string BilltoName
        {
            get => _billtoName;
            set =>
                _billtoName = String.IsNullOrWhiteSpace(value)
                    ? String.Empty
                    : value.Substring(0, 49);
        }

        public string YourReference
        {
            get => _yourReference;
            set =>
                _yourReference = String.IsNullOrWhiteSpace(value)
                    ? String.Empty
                    : value.Substring(0, 49);
        }

        public string EntryNo
        {
            get => _entryNo;
            set =>
                _entryNo = String.IsNullOrWhiteSpace(value)
                    ? String.Empty
                    : value.Substring(0, 49);
        }

        public string Qty
        {
            get => _qty;
            set =>
                _qty = String.IsNullOrWhiteSpace(value)
                    ? String.Empty
                    : value.Substring(0, 49);
        }

        public string SizeA
        {
            get => _sizeA;
            set =>
                _sizeA = String.IsNullOrWhiteSpace(value)
                    ? String.Empty
                    : value.Substring(0, 49);
        }

        public string SizeB
        {
            get => _sizeB;
            set =>
                _sizeB = String.IsNullOrWhiteSpace(value)
                    ? String.Empty
                    : value.Substring(0, 49);
        }

        public string CoilNumber
        {
            get => _coilNumber;
            set =>
                _coilNumber = String.IsNullOrWhiteSpace(value)
                    ? String.Empty
                    : value.Substring(0, 49);
        }

        public string CoilGauge
        {
            get => _coilGauge;
            set =>
                _coilGauge = String.IsNullOrWhiteSpace(value)
                    ? String.Empty
                    : value.Substring(0, 49);
        }

        public string CoilWidth
        {
            get => _coilWidth;
            set =>
                _coilWidth = String.IsNullOrWhiteSpace(value)
                    ? String.Empty
                    : value.Substring(0, 49);
        }

        public string Misc1
        {
            get => _misc1;
            set =>
                _misc1 = String.IsNullOrWhiteSpace(value)
                    ? String.Empty
                    : value.Substring(0, 49);
        }

        public string Misc2
        {
            get => _misc2;
            set =>
                _misc2 = String.IsNullOrWhiteSpace(value)
                    ? String.Empty
                    : value.Substring(0, 49);
        }

        public string Misc3
        {
            get => _misc3;
            set =>
                _misc3 = String.IsNullOrWhiteSpace(value)
                    ? String.Empty
                    : value.Substring(0, 49);
        }
    }
}
