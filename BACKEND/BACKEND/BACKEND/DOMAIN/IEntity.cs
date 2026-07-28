using System;
using System.Collections.Generic;
using System.Text;

namespace BACKEND.DOMAIN
{
    public  interface IEntity
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } 
    }
}
