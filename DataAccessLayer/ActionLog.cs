//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DataAccessLayer
{
    using System;
    using System.Collections.Generic;
    
    public partial class ActionLog
    {
        public System.DateTime date_time { get; set; }
        public string user_email { get; set; }
        public string action_type { get; set; }
        public string description { get; set; }
        public string sql_command { get; set; }
    }
}
