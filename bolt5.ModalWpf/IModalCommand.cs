using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bolt5.ModalWpf
{
    public interface IModalCommand
    {
        Action<ModalResult, object> ExecuteResult { get; set; }
    }
}
