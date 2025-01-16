﻿namespace ASP_P22.Data.Entities
{
    public class UserAccess
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }    
        public string Login { get; set; }
        public string DK{ get; set; }
        public string Salt{ get; set; }
        public User User { get; set; }
    }
}
