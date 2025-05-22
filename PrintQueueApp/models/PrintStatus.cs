using PrintNode;
using PrintQueueApp.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PrintQueueApp.models
{
    public class PrintStatus : INotifyPropertyChanged
    {
        private string _printName;
        private string _printDescription;
        private string _statusTypeMessage;
        private int _listNums;
        private int _statusType;
        //唯一tag
        public string PrintName
        {
            set
            {
                if (_printName != value)
                {

                    _printName = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("PrintName"));

                }
            }
            get { return _printName; }
        }


        public string PrintDescription
        {
            set
            {
                if ( _printDescription != value)
                {
                    _printDescription = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("PrintDescription"));

                }
            }
            get { return _printDescription; }

        }

        // TODO: 需要完善其他

        //当前有多少任务已经在windows队列了
        public int ListNums
        {
            set
            {
                if (_listNums != value)
                {
                    _listNums = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("ListNums"));

                }
            }
            get { return _listNums; }

        }

        //繁忙啥的
        public string StatusTypeMessage
        {
            set
            {
                if (_statusTypeMessage != value)
                {
                    _statusTypeMessage = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("StatusTypeMessage"));

                }
            }
            get { return _statusTypeMessage; }

        }
        // 0:异常 1:空闲 2:繁忙 3:忙碌 4:爆满
        public int StatusType
        {
            set
            {
                if (_statusType != value)
                {
                    _statusType = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("StatusType"));

                }
            }
            get { return _statusType; }

        }

        public List<PrintRW> PrintJobs{set; get;}

        public event PropertyChangedEventHandler PropertyChanged = delegate { };


    }
}
