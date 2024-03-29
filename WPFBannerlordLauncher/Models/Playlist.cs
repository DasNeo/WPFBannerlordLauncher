using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPFBannerlordLauncher.Controls;

namespace WPFBannerlordLauncher.Models
{
    public class Playlist
    {
        public string Name { get; set; }
        public Dictionary<int, Tuple<string, bool>> Modules { get; set; } = new Dictionary<int, Tuple<string, bool>>();
    }
}
