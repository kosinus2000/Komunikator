using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace KomunikatorClient.MVVM.Model
{
    public class MessageModel
    {
        public DateTime Time { get; set; }
        public string Username { get; set; }
        public string ImageSource { get; set; }
        public string Message { get; set; }
        public string UserColor { get; set; }
        public bool IsNativeOrigin { get; set; }
        public bool? FirstMessage { get; set; }
    }
}
