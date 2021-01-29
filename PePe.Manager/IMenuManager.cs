using PePe.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace PePe.Manager {
    public interface IMenuManager {
        Menu GetTodaysMenu();

        IEnumerable<Menu> Find(MenuSearch search);
    }
}
