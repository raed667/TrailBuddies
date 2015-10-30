using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackgroundGps.WinRT.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public int NbrEasyTrails { get; set; }
        public int NbrMediumTrails { get; set; }
        public int NbrHardTrails { get; set; }
        public string CreationDate { get; set; }
        public string LastUpdateDate { get; set; }

        public User()
        {

        }

        public User(string name, string phoneNumber)
        {
            this.Name = name;
            this.PhoneNumber = phoneNumber;
            this.NbrEasyTrails = 0;
            this.NbrMediumTrails = 0;
            this.NbrHardTrails = 0;
            CreationDate = DateTime.Now.ToString();
            LastUpdateDate = DateTime.Now.ToString();
        }

        public virtual ICollection<Trail> Trails { get; set; }
    }
}
