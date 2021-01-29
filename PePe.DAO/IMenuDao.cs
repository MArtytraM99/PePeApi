using PePe.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace PePe.DAO {
    public interface IMenuDao {
        Menu GetMenuByDate(DateTime dateTime);
        Menu Save(Menu menu);
    }
}
