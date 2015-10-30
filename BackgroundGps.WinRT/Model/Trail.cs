using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackgroundGps.WinRT.Model
{
    public class Trail
    {
        public int Id { get; set; }
        public int Duration { get; set; }
        public float Distance { get; set; }
        public float Altitude { get; set; }
        public string CreationDate { get; set; }
        public string LastUpdateDate { get; set; }

        public Trail()
        {

        }

        public Trail(int duration, float distance, float altitude)
        {
            this.Duration = duration;
            this.Distance = distance;
            this.Altitude = altitude;
            CreationDate = DateTime.Now.ToString();
            LastUpdateDate = DateTime.Now.ToString();
        }

        public virtual User User { get; set; }
    }
}
